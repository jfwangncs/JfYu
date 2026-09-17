namespace JfYu.UnitTests.Request
{
    /// <summary>
    /// Adds a small pause before each request test case so the shared local httpbin
    /// instance is not overwhelmed (firing too many requests too quickly can make it
    /// return HTTP 500). Increase <see cref="DelayMilliseconds"/> if 500s persist.
    /// </summary>
    internal static class RequestThrottle
    {
        /// <summary>
        /// Pause (in milliseconds) applied before each request test case.
        /// </summary>
        public const int DelayMilliseconds = 100;

        /// <summary>
        /// Blocks for <see cref="DelayMilliseconds"/> to throttle the request rate.
        /// </summary>
        public static void Wait() => System.Threading.Thread.Sleep(DelayMilliseconds);
    }
}
