using System.Threading.RateLimiting;
using Serilog;

namespace LazyCalculator
{
    internal sealed class Calculator
    {
        private readonly RateLimiter _rateLimiter;

        public Calculator(RateLimiter rateLimiter)
        {
            _rateLimiter = rateLimiter;
        }

        public async ValueTask<int> AddAsync(int a, int b)
        {
            Log.Debug("Request to add {A} and {B}", a, b);

            using (var lease = await _rateLimiter.AcquireAsync(permitCount: 1))
            {
                if (lease.IsAcquired)
                {
                    Log.Debug("Acquired lease for {A} and {B}", a, b);
                    return a + b;
                }

                Log.Warning("Failed to acquire lease for {A} and {B}", a, b);

                var retryMessage = lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? $"Retry after {retryAfterValue.TotalMilliseconds} ms"
                    : "Retry later";

                throw new InvalidOperationException($"{a} + {b} cannot be calculated at this time. {retryMessage}");
            }
        }
    }
}