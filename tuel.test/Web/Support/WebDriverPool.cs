using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;

namespace TUEL.TestFramework.Web.Support
{
    internal sealed class WebDriverPool : IDisposable
    {
        private readonly Dictionary<string, Queue<DriverLease>> _pool = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _syncRoot = new();
        private readonly int _maxPerKey;
        private readonly TimeSpan _maxIdleTime;
        private readonly Timer _scavengeTimer;

        public WebDriverPool(int maxPerKey, TimeSpan maxIdleTime)
        {
            _maxPerKey = Math.Max(1, maxPerKey);
            _maxIdleTime = maxIdleTime <= TimeSpan.Zero ? TimeSpan.FromMinutes(10) : maxIdleTime;
            _scavengeTimer = new Timer(Scavenge, null, _maxIdleTime, _maxIdleTime);
        }

        public IWebDriver Acquire(string key, Func<IWebDriver> factory)
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            lock (_syncRoot)
            {
                if (_pool.TryGetValue(key, out var queue))
                {
                    while (queue.Count > 0)
                    {
                        var lease = queue.Dequeue();
                        if (lease.TryTake(_maxIdleTime, out var driver))
                        {
                            return driver;
                        }
                    }
                }
            }

            return factory();
        }

        public void Release(string key, IWebDriver driver)
        {
            if (driver is null)
            {
                return;
            }

            lock (_syncRoot)
            {
                if (!_pool.TryGetValue(key, out var queue))
                {
                    queue = new Queue<DriverLease>();
                    _pool[key] = queue;
                }

                if (queue.Count >= _maxPerKey)
                {
                    DisposeDriver(driver);
                    return;
                }

                queue.Enqueue(new DriverLease(driver));
            }
        }

        private void Scavenge(object? state)
        {
            lock (_syncRoot)
            {
                foreach (var key in _pool.Keys.ToList())
                {
                    var queue = _pool[key];
                    var retained = new Queue<DriverLease>();

                    while (queue.Count > 0)
                    {
                        var lease = queue.Dequeue();
                        if (lease.IsExpired(_maxIdleTime) || !lease.IsHealthy())
                        {
                            lease.Dispose();
                            continue;
                        }

                        retained.Enqueue(lease);
                    }

                    if (retained.Count > 0)
                    {
                        _pool[key] = retained;
                    }
                    else
                    {
                        _pool.Remove(key);
                    }
                }
            }
        }

        public void Dispose()
        {
            _scavengeTimer.Dispose();
            lock (_syncRoot)
            {
                foreach (var queue in _pool.Values)
                {
                    while (queue.Count > 0)
                    {
                        queue.Dequeue().Dispose();
                    }
                }
                _pool.Clear();
            }
        }

        private static void DisposeDriver(IWebDriver driver)
        {
            try { driver.Quit(); } catch { /* ignore */ }
            try { driver.Dispose(); } catch { /* ignore */ }
        }

        private sealed class DriverLease : IDisposable
        {
            private IWebDriver? _driver;
            private DateTime _lastUsedUtc;

            public DriverLease(IWebDriver driver)
            {
                _driver = driver ?? throw new ArgumentNullException(nameof(driver));
                _lastUsedUtc = DateTime.UtcNow;
            }

            public bool TryTake(TimeSpan maxIdle, out IWebDriver driver)
            {
                driver = default!;
                if (_driver is null)
                {
                    return false;
                }

                if (IsExpired(maxIdle) || !IsHealthy())
                {
                    Dispose();
                    return false;
                }

                _lastUsedUtc = DateTime.UtcNow;
                driver = _driver;
                _driver = null;
                return true;
            }

            public bool IsExpired(TimeSpan maxIdle)
            {
                return DateTime.UtcNow - _lastUsedUtc > maxIdle;
            }

            public bool IsHealthy()
            {
                if (_driver is null)
                {
                    return false;
                }

                try
                {
                    var _ = _driver.WindowHandles;
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public void Dispose()
            {
                if (_driver is null)
                {
                    return;
                }

                DisposeDriver(_driver);
                _driver = null;
            }
        }
    }
}
