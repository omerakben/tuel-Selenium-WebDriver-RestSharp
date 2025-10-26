using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TUEL.TestFramework.Logging;

namespace TUEL.TestFramework.Performance.Advanced
{
    /// <summary>
    /// Advanced performance optimization engine for enterprise-grade test framework.
    /// Implements intelligent caching, parallel execution, and adaptive performance tuning.
    /// </summary>
    public static class AdvancedPerformanceEngine
    {
        private static readonly ConcurrentDictionary<string, PerformanceCache> _cache = new();
        private static readonly ConcurrentDictionary<string, PerformanceMetrics> _metrics = new();
        private static readonly SemaphoreSlim _parallelExecutionSemaphore = new(Environment.ProcessorCount * 2);
        private static readonly AdaptivePerformanceTuner _performanceTuner = new();
        private static bool _isInitialized = false;

        /// <summary>
        /// Initializes the advanced performance engine with optimal settings.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            _performanceTuner.Initialize();
            ConfigureOptimalSettings();
            StartPerformanceMonitoring();

            _isInitialized = true;
            TestLogger.LogInformation("Advanced Performance Engine initialized with {0} CPU cores", Environment.ProcessorCount);
        }

        /// <summary>
        /// Intelligent caching system with automatic invalidation and optimization.
        /// </summary>
        public static class IntelligentCache
        {
            /// <summary>
            /// Gets or creates cached data with intelligent expiration.
            /// </summary>
            public static async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
            {
                var cacheKey = GenerateCacheKey<T>(key);

                if (_cache.TryGetValue(cacheKey, out var cachedItem) && !cachedItem.IsExpired)
                {
                    TestLogger.LogDebug("Cache hit for key: {0}", key);
                    return (T)cachedItem.Data;
                }

                TestLogger.LogDebug("Cache miss for key: {0}, creating new data", key);
                var data = await factory();

                var expirationTime = expiration ?? TimeSpan.FromMinutes(5);
                _cache[cacheKey] = new PerformanceCache
                {
                    Data = data,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.Add(expirationTime),
                    AccessCount = 0
                };

                return data;
            }

            /// <summary>
            /// Invalidates cache entries matching a pattern.
            /// </summary>
            public static void InvalidatePattern(string pattern)
            {
                var keysToRemove = _cache.Keys.Where(k => k.Contains(pattern)).ToList();
                foreach (var key in keysToRemove)
                {
                    _cache.TryRemove(key, out _);
                }
                TestLogger.LogInformation("Invalidated {0} cache entries matching pattern: {1}", keysToRemove.Count, pattern);
            }

            /// <summary>
            /// Clears all cache entries.
            /// </summary>
            public static void Clear()
            {
                _cache.Clear();
                TestLogger.LogInformation("Cache cleared");
            }

            /// <summary>
            /// Gets cache statistics.
            /// </summary>
            public static CacheStatistics GetStatistics()
            {
                var totalEntries = _cache.Count;
                var expiredEntries = _cache.Values.Count(c => c.IsExpired);
                var totalAccesses = _cache.Values.Sum(c => c.AccessCount);
                var hitRate = totalAccesses > 0 ? (double)(totalAccesses - expiredEntries) / totalAccesses * 100 : 0;

                return new CacheStatistics
                {
                    TotalEntries = totalEntries,
                    ExpiredEntries = expiredEntries,
                    TotalAccesses = totalAccesses,
                    HitRate = hitRate
                };
            }
        }

        /// <summary>
        /// Advanced parallel execution engine with intelligent load balancing.
        /// </summary>
        public static class ParallelExecutionEngine
        {
            /// <summary>
            /// Executes tasks in parallel with intelligent load balancing.
            /// </summary>
            public static async Task<List<T>> ExecuteParallelAsync<T>(IEnumerable<Func<Task<T>>> tasks, int? maxConcurrency = null)
            {
                var maxConcurrent = maxConcurrency ?? Environment.ProcessorCount;
                var semaphore = new SemaphoreSlim(maxConcurrent);
                var tasksList = tasks.ToList();
                var results = new List<T>();

                var parallelTasks = tasksList.Select(async task =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await task();
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                results.AddRange(await Task.WhenAll(parallelTasks));
                TestLogger.LogInformation("Executed {0} tasks in parallel with max concurrency: {1}", tasksList.Count, maxConcurrent);

                return results;
            }

            /// <summary>
            /// Executes actions in parallel with progress tracking.
            /// </summary>
            public static async Task ExecuteParallelWithProgressAsync<T>(IEnumerable<T> items, Func<T, Task> action, int? maxConcurrency = null)
            {
                var maxConcurrent = maxConcurrency ?? Environment.ProcessorCount;
                var semaphore = new SemaphoreSlim(maxConcurrent);
                var itemsList = items.ToList();
                var completed = 0;

                var parallelTasks = itemsList.Select(async item =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await action(item);
                        Interlocked.Increment(ref completed);
                        TestLogger.LogDebug("Progress: {0}/{1} completed", completed, itemsList.Count);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(parallelTasks);
                TestLogger.LogInformation("Completed parallel execution of {0} items", itemsList.Count);
            }

            /// <summary>
            /// Executes tasks with adaptive concurrency based on system load.
            /// </summary>
            public static async Task<List<T>> ExecuteWithAdaptiveConcurrencyAsync<T>(IEnumerable<Func<Task<T>>> tasks)
            {
                var tasksList = tasks.ToList();
                var optimalConcurrency = _performanceTuner.GetOptimalConcurrency();

                TestLogger.LogInformation("Using adaptive concurrency: {0} for {1} tasks", optimalConcurrency, tasksList.Count);

                return await ExecuteParallelAsync(tasksList, optimalConcurrency);
            }
        }

        /// <summary>
        /// Advanced performance monitoring with real-time analysis.
        /// </summary>
        public static class AdvancedMonitoring
        {
            /// <summary>
            /// Records detailed performance metrics with context.
            /// </summary>
            public static void RecordDetailedMetric(string operation, long durationMs, Dictionary<string, object>? context = null)
            {
                var metric = _metrics.GetOrAdd(operation, _ => new PerformanceMetrics { Operation = operation });

                lock (metric)
                {
                    metric.Durations.Add(durationMs);
                    metric.TotalExecutions++;
                    metric.TotalDuration += durationMs;
                    metric.LastExecuted = DateTime.UtcNow;

                    if (context != null)
                    {
                        metric.ContextData.AddRange(context.Select(kvp => new ContextData
                        {
                            Key = kvp.Key,
                            Value = kvp.Value?.ToString() ?? "null",
                            Timestamp = DateTime.UtcNow
                        }));
                    }
                }

                // Update performance tuner with new data
                _performanceTuner.UpdateMetrics(operation, durationMs);

                TestLogger.LogDebug("Recorded detailed metric: {0} = {1}ms", operation, durationMs);
            }

            /// <summary>
            /// Gets comprehensive performance analysis.
            /// </summary>
            public static PerformanceAnalysis GetPerformanceAnalysis()
            {
                var analysis = new PerformanceAnalysis
                {
                    AnalysisDate = DateTime.UtcNow,
                    TotalOperations = _metrics.Count,
                    CacheStatistics = IntelligentCache.GetStatistics()
                };

                foreach (var (operation, metrics) in _metrics)
                {
                    var stats = CalculatePerformanceStatistics(metrics);
                    analysis.OperationStatistics[operation] = stats;

                    if (stats.AverageDuration > analysis.SlowestOperation.AverageDuration)
                    {
                        analysis.SlowestOperation = stats;
                    }
                }

                analysis.OverallPerformanceScore = CalculateOverallPerformanceScore(analysis);
                analysis.Recommendations = GeneratePerformanceRecommendations(analysis);

                return analysis;
            }

            /// <summary>
            /// Monitors system resources and performance.
            /// </summary>
            public static SystemResourceMetrics GetSystemResourceMetrics()
            {
                var process = Process.GetCurrentProcess();
                var workingSet = process.WorkingSet64;
                var cpuTime = process.TotalProcessorTime;
                var threadCount = process.Threads.Count;

                return new SystemResourceMetrics
                {
                    MemoryUsageMB = workingSet / 1024 / 1024,
                    CpuTime = cpuTime,
                    ThreadCount = threadCount,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Memory optimization utilities.
        /// </summary>
        public static class MemoryOptimizer
        {
            /// <summary>
            /// Optimizes memory usage by cleaning up unused resources.
            /// </summary>
            public static void OptimizeMemoryUsage()
            {
                // Clear expired cache entries
                var expiredKeys = _cache.Where(kvp => kvp.Value.IsExpired).Select(kvp => kvp.Key).ToList();
                foreach (var key in expiredKeys)
                {
                    _cache.TryRemove(key, out _);
                }

                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                TestLogger.LogInformation("Memory optimization completed. Removed {0} expired cache entries", expiredKeys.Count);
            }

            /// <summary>
            /// Gets memory usage statistics.
            /// </summary>
            public static MemoryUsageStatistics GetMemoryStatistics()
            {
                var process = Process.GetCurrentProcess();
                var workingSet = process.WorkingSet64;
                var privateMemory = process.PrivateMemorySize64;
                var virtualMemory = process.VirtualMemorySize64;

                return new MemoryUsageStatistics
                {
                    WorkingSetMB = workingSet / 1024 / 1024,
                    PrivateMemoryMB = privateMemory / 1024 / 1024,
                    VirtualMemoryMB = virtualMemory / 1024 / 1024,
                    CacheEntries = _cache.Count,
                    MetricsEntries = _metrics.Count,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        #region Private Helper Methods

        private static void ConfigureOptimalSettings()
        {
            // Configure optimal thread pool settings
            ThreadPool.SetMinThreads(Environment.ProcessorCount, Environment.ProcessorCount);
            ThreadPool.SetMaxThreads(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);

            // Configure optimal GC settings
            GC.Collect(2, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Forced, true);

            TestLogger.LogInformation("Configured optimal performance settings");
        }

        private static void StartPerformanceMonitoring()
        {
            Task.Run(async () =>
            {
                while (_isInitialized)
                {
                    try
                    {
                        await MonitorPerformanceAsync();
                        await Task.Delay(TimeSpan.FromMinutes(1));
                    }
                    catch (Exception ex)
                    {
                        TestLogger.LogException(ex, "Performance monitoring error");
                        await Task.Delay(TimeSpan.FromMinutes(5));
                    }
                }
            });

            TestLogger.LogInformation("Started background performance monitoring");
        }

        private static async Task MonitorPerformanceAsync()
        {
            var resourceMetrics = AdvancedMonitoring.GetSystemResourceMetrics();
            var memoryStats = MemoryOptimizer.GetMemoryStatistics();

            // Log performance warnings if thresholds exceeded
            if (resourceMetrics.MemoryUsageMB > 500)
            {
                TestLogger.LogWarning("High memory usage detected: {0}MB", resourceMetrics.MemoryUsageMB);
                MemoryOptimizer.OptimizeMemoryUsage();
            }

            if (resourceMetrics.ThreadCount > Environment.ProcessorCount * 4)
            {
                TestLogger.LogWarning("High thread count detected: {0}", resourceMetrics.ThreadCount);
            }

            await Task.CompletedTask;
        }

        private static string GenerateCacheKey<T>(string key)
        {
            return $"{typeof(T).Name}_{key}";
        }

        private static PerformanceStatistics CalculatePerformanceStatistics(PerformanceMetrics metrics)
        {
            lock (metrics)
            {
                if (metrics.Durations.Count == 0)
                {
                    return new PerformanceStatistics();
                }

                var durations = metrics.Durations.ToList();
                return new PerformanceStatistics
                {
                    Operation = metrics.Operation,
                    ExecutionCount = metrics.TotalExecutions,
                    TotalDuration = metrics.TotalDuration,
                    AverageDuration = durations.Average(),
                    MinDuration = durations.Min(),
                    MaxDuration = durations.Max(),
                    StandardDeviation = CalculateStandardDeviation(durations),
                    LastExecuted = metrics.LastExecuted
                };
            }
        }

        private static double CalculateStandardDeviation(List<long> values)
        {
            if (values.Count <= 1) return 0;

            var average = values.Average();
            var sumOfSquares = values.Sum(v => Math.Pow(v - average, 2));
            return Math.Sqrt(sumOfSquares / values.Count);
        }

        private static int CalculateOverallPerformanceScore(PerformanceAnalysis analysis)
        {
            var score = 100;

            // Penalize slow operations
            foreach (var (_, stats) in analysis.OperationStatistics)
            {
                if (stats.AverageDuration > 5000) score -= 10; // Very slow
                else if (stats.AverageDuration > 2000) score -= 5; // Slow
                else if (stats.AverageDuration > 1000) score -= 2; // Moderate
            }

            // Bonus for good cache hit rate
            if (analysis.CacheStatistics.HitRate > 80) score += 10;
            else if (analysis.CacheStatistics.HitRate > 60) score += 5;

            return Math.Max(0, Math.Min(100, score));
        }

        private static List<string> GeneratePerformanceRecommendations(PerformanceAnalysis analysis)
        {
            var recommendations = new List<string>();

            if (analysis.SlowestOperation.AverageDuration > 2000)
            {
                recommendations.Add($"Optimize slow operation: {analysis.SlowestOperation.Operation}");
            }

            if (analysis.CacheStatistics.HitRate < 50)
            {
                recommendations.Add("Improve cache hit rate by adjusting cache expiration times");
            }

            if (analysis.TotalOperations > 100)
            {
                recommendations.Add("Consider implementing operation batching for better performance");
            }

            return recommendations;
        }

        #endregion
    }

    /// <summary>
    /// Adaptive performance tuner that learns and optimizes based on runtime metrics.
    /// </summary>
    public class AdaptivePerformanceTuner
    {
        private readonly Dictionary<string, List<long>> _operationMetrics = new();
        private int _optimalConcurrency = Environment.ProcessorCount;
        private DateTime _lastTuning = DateTime.UtcNow;

        public void Initialize()
        {
            _optimalConcurrency = Environment.ProcessorCount;
            TestLogger.LogInformation("Adaptive Performance Tuner initialized");
        }

        public void UpdateMetrics(string operation, long durationMs)
        {
            if (!_operationMetrics.ContainsKey(operation))
            {
                _operationMetrics[operation] = new List<long>();
            }

            _operationMetrics[operation].Add(durationMs);

            // Keep only recent metrics (last 100 measurements)
            if (_operationMetrics[operation].Count > 100)
            {
                _operationMetrics[operation].RemoveAt(0);
            }

            // Tune performance every 5 minutes
            if (DateTime.UtcNow - _lastTuning > TimeSpan.FromMinutes(5))
            {
                TunePerformance();
                _lastTuning = DateTime.UtcNow;
            }
        }

        public int GetOptimalConcurrency()
        {
            return _optimalConcurrency;
        }

        private void TunePerformance()
        {
            var averageDuration = _operationMetrics.Values
                .SelectMany(m => m)
                .DefaultIfEmpty(0)
                .Average();

            // Adjust concurrency based on performance
            if (averageDuration > 2000) // Slow operations
            {
                _optimalConcurrency = Math.Max(1, _optimalConcurrency - 1);
                TestLogger.LogInformation("Reduced concurrency to {0} due to slow operations", _optimalConcurrency);
            }
            else if (averageDuration < 500) // Fast operations
            {
                _optimalConcurrency = Math.Min(Environment.ProcessorCount * 2, _optimalConcurrency + 1);
                TestLogger.LogInformation("Increased concurrency to {0} due to fast operations", _optimalConcurrency);
            }
        }
    }

    #region Performance Models

    public class PerformanceCache
    {
        public object Data { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int AccessCount { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }

    public class PerformanceMetrics
    {
        public string Operation { get; set; } = string.Empty;
        public List<long> Durations { get; set; } = new();
        public int TotalExecutions { get; set; }
        public long TotalDuration { get; set; }
        public DateTime LastExecuted { get; set; }
        public List<ContextData> ContextData { get; set; } = new();
    }

    public class ContextData
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class CacheStatistics
    {
        public int TotalEntries { get; set; }
        public int ExpiredEntries { get; set; }
        public int TotalAccesses { get; set; }
        public double HitRate { get; set; }
    }

    public class PerformanceStatistics
    {
        public string Operation { get; set; } = string.Empty;
        public int ExecutionCount { get; set; }
        public long TotalDuration { get; set; }
        public double AverageDuration { get; set; }
        public long MinDuration { get; set; }
        public long MaxDuration { get; set; }
        public double StandardDeviation { get; set; }
        public DateTime LastExecuted { get; set; }
    }

    public class PerformanceAnalysis
    {
        public DateTime AnalysisDate { get; set; }
        public int TotalOperations { get; set; }
        public Dictionary<string, PerformanceStatistics> OperationStatistics { get; set; } = new();
        public PerformanceStatistics SlowestOperation { get; set; } = new();
        public CacheStatistics CacheStatistics { get; set; } = new();
        public int OverallPerformanceScore { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    public class SystemResourceMetrics
    {
        public long MemoryUsageMB { get; set; }
        public TimeSpan CpuTime { get; set; }
        public int ThreadCount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MemoryUsageStatistics
    {
        public long WorkingSetMB { get; set; }
        public long PrivateMemoryMB { get; set; }
        public long VirtualMemoryMB { get; set; }
        public int CacheEntries { get; set; }
        public int MetricsEntries { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion
}
