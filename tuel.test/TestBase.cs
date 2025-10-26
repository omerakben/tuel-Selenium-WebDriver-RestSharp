using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TUEL.TestFramework
{
    /// <summary>
    /// Base class for all test classes providing common test functionality
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Gets or sets the test context which provides information about
        /// and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Test initialization method called before each test method
        /// </summary>
        [TestInitialize]
        public virtual void TestInitialize()
        {
            TestLogger.LogInformation("Starting test: {0}", TestContext?.TestName);
        }

        /// <summary>
        /// Test cleanup method called after each test method
        /// </summary>
        [TestCleanup]
        public virtual void TestCleanup()
        {
            TestLogger.LogInformation("Completed test: {0}", TestContext?.TestName);
        }
    }
}
