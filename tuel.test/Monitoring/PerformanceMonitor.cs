using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TUEL.TestFramework.Logging;

namespace TUEL.TestFramework.Monitoring
{
    /// <summary>
    /// Performance monitoring utility for tracking test execution metrics.
    /// Provides comprehensive performance data collection and analysis.
    /// </summary>
    public static class PerformanceMonitor
    {
        private static readonly ConcurrentDictionary<string, PerformanceMetric> _metrics = new();
        private static readonly ConcurrentDictionary<string, Stopwatch> _activeTimers = new();
        private static bool _enabled = true;

        /// <summary>
        /// Enables or disables performance monitoring.
        /// </summary>
        public static bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                TestLogger.LogInformation("Performance monitoring {0}", value ? "enabled" : "disabled");
            }
        }

        /// <summary>
        /// Starts timing an operation.
        /// </summary>
        /// <param name="operationName">Name of the operation to time</param>
        /// <returns>Timer ID for stopping the timer</returns>
        public static string StartTimer(string operationName)
        {
            if (!_enabled) return string.Empty;

            var timerId = Guid.NewGuid().ToString();
            var stopwatch = Stopwatch.StartNew();
            _activeTimers[timerId] = stopwatch;

            TestLogger.LogDebug("Started timer for operation: {0}", operationName);
            return timerId;
        }

        /// <summary>
        /// Stops timing an operation and records the metric.
        /// </summary>
        /// <param name="timerId">Timer ID returned from StartTimer</param>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="additionalData">Additional data to associate with the metric</param>
        public static void StopTimer(string timerId, string operationName, Dictionary<string, object>? additionalData = null)
        {
            if (!_enabled || string.IsNullOrEmpty(timerId)) return;

            if (_activeTimers.TryRemove(timerId, out var stopwatch))
            {
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.ElapsedMilliseconds, additionalData);
                TestLogger.LogDebug("Stopped timer for operation: {0}, duration: {1}ms", operationName, stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Records a performance metric.
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="durationMs">Duration in milliseconds</param>
        /// <param name="additionalData">Additional data to associate with the metric</param>
        public static void RecordMetric(string operationName, long durationMs, Dictionary<string, object>? additionalData = null)
        {
            if (!_enabled) return;

            _metrics.AddOrUpdate(operationName,
                new PerformanceMetric(operationName, durationMs, additionalData),
                (key, existing) => existing.AddMeasurement(durationMs, additionalData));

            TestLogger.LogDebug("Recorded metric: {0} = {1}ms", operationName, durationMs);
        }

        /// <summary>
        /// Records a performance metric with automatic timing.
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="action">Action to execute and time</param>
        /// <param name="additionalData">Additional data to associate with the metric</param>
        public static void TimeOperation(string operationName, Action action, Dictionary<string, object>? additionalData = null)
        {
            if (!_enabled)
            {
                action();
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                action();
            }
            finally
            {
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.ElapsedMilliseconds, additionalData);
            }
        }

        /// <summary>
        /// Records a performance metric with automatic timing for async operations.
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="action">Async action to execute and time</param>
        /// <param name="additionalData">Additional data to associate with the metric</param>
        public static async Task TimeOperationAsync(string operationName, Func<Task> action, Dictionary<string, object>? additionalData = null)
        {
            if (!_enabled)
            {
                await action();
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                await action();
            }
            finally
            {
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.ElapsedMilliseconds, additionalData);
            }
        }

        /// <summary>
        /// Records a performance metric with automatic timing for functions that return values.
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="function">Function to execute and time</param>
        /// <param name="additionalData">Additional data to associate with the metric</param>
        /// <returns>Result of the function</returns>
        public static T TimeOperation<T>(string operationName, Func<T> function, Dictionary<string, object>? additionalData = null)
        {
            if (!_enabled) return function();

            var stopwatch = Stopwatch.StartNew();
            try
            {
                return function();
            }
            finally
            {
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.ElapsedMilliseconds, additionalData);
            }
        }

        /// <summary>
        /// Gets performance statistics for a specific operation.
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <returns>Performance statistics or null if not found</returns>
        public static PerformanceStatistics? GetStatistics(string operationName)
        {
            if (_metrics.TryGetValue(operationName, out var metric))
            {
                return new PerformanceStatistics(metric);
            }
            return null;
        }

        /// <summary>
        /// Gets all performance statistics.
        /// </summary>
        /// <returns>Dictionary of operation names to performance statistics</returns>
        public static Dictionary<string, PerformanceStatistics> GetAllStatistics()
        {
            return _metrics.ToDictionary(
                kvp => kvp.Key,
                kvp => new PerformanceStatistics(kvp.Value));
        }

        /// <summary>
        /// Generates a comprehensive performance report.
        /// </summary>
        /// <returns>Formatted performance report</returns>
        public static string GenerateReport()
        {
            if (!_enabled || _metrics.IsEmpty)
            {
                return "Performance monitoring is disabled or no metrics have been recorded.";
            }

            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Performance Monitoring Report ===");
            report.AppendLine($"Total Operations Tracked: {_metrics.Count}");
            report.AppendLine($"Report Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine();

            var allStats = GetAllStatistics();
            var sortedStats = allStats.OrderByDescending(kvp => kvp.Value.TotalDuration);

            foreach (var (operationName, stats) in sortedStats)
            {
                report.AppendLine($"Operation: {operationName}");
                report.AppendLine($"  Executions: {stats.ExecutionCount}");
                report.AppendLine($"  Total Duration: {stats.TotalDuration}ms");
                report.AppendLine($"  Average Duration: {stats.AverageDuration:F2}ms");
                report.AppendLine($"  Min Duration: {stats.MinDuration}ms");
                report.AppendLine($"  Max Duration: {stats.MaxDuration}ms");
                report.AppendLine($"  Standard Deviation: {stats.StandardDeviation:F2}ms");
                report.AppendLine();
            }

            return report.ToString();
        }

        /// <summary>
        /// Clears all performance metrics.
        /// </summary>
        public static void ClearMetrics()
        {
            _metrics.Clear();
            _activeTimers.Clear();
            TestLogger.LogInformation("Performance metrics cleared");
        }

        /// <summary>
        /// Gets the number of active timers.
        /// </summary>
        public static int ActiveTimerCount => _activeTimers.Count;

        /// <summary>
        /// Gets the total number of metrics recorded.
        /// </summary>
        public static int TotalMetricsCount => _metrics.Count;
    }

    /// <summary>
    /// Represents a single performance metric with multiple measurements.
    /// </summary>
    public class PerformanceMetric
    {
        public string OperationName { get; }
        public List<long> Durations { get; }
        public List<Dictionary<string, object>?> AdditionalData { get; }
        public DateTime FirstRecorded { get; }
        public DateTime LastRecorded { get; }

        public PerformanceMetric(string operationName, long durationMs, Dictionary<string, object>? additionalData = null)
        {
            OperationName = operationName;
            Durations = new List<long> { durationMs };
            AdditionalData = new List<Dictionary<string, object>?> { additionalData };
            FirstRecorded = DateTime.UtcNow;
            LastRecorded = DateTime.UtcNow;
        }

        public PerformanceMetric AddMeasurement(long durationMs, Dictionary<string, object>? additionalData = null)
        {
            Durations.Add(durationMs);
            AdditionalData.Add(additionalData);
            LastRecorded = DateTime.UtcNow;
            return this;
        }
    }

    /// <summary>
    /// Represents performance statistics for an operation.
    /// </summary>
    public class PerformanceStatistics
    {
        public string OperationName { get; }
        public int ExecutionCount { get; }
        public long TotalDuration { get; }
        public double AverageDuration { get; }
        public long MinDuration { get; }
        public long MaxDuration { get; }
        public double StandardDeviation { get; }
        public DateTime FirstRecorded { get; }
        public DateTime LastRecorded { get; }

        public PerformanceStatistics(PerformanceMetric metric)
        {
            OperationName = metric.OperationName;
            ExecutionCount = metric.Durations.Count;
            TotalDuration = metric.Durations.Sum();
            AverageDuration = metric.Durations.Average();
            MinDuration = metric.Durations.Min();
            MaxDuration = metric.Durations.Max();
            FirstRecorded = metric.FirstRecorded;
            LastRecorded = metric.LastRecorded;

            // Calculate standard deviation
            var variance = metric.Durations.Sum(d => Math.Pow(d - AverageDuration, 2)) / ExecutionCount;
            StandardDeviation = Math.Sqrt(variance);
        }
    }
}
