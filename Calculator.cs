using System.Threading.RateLimiting;
using Serilog;

namespace LazyCalculator
{
    public class Calculator
    {
        private readonly RateLimiter _rateLimiter;

        public Calculator(RateLimiter rateLimiter)
        {
            _rateLimiter = rateLimiter;
        }

        public async ValueTask<int> AddAsync(int a, int b)
        {
            Log.Debug("Request to add {a} and {b}", a, b);

            using var lease = await _rateLimiter.AcquireAsync(permitCount: 1);

            if (lease.IsAcquired)
            {
                Log.Debug("Acquired lease for {a} and {b}", a, b);
                return a + b;
            }

            Log.Warning("Lease not acquired for {a} and {b}", a, b);

            var retryMessage = lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                ? $"Retry after {retryAfterValue} ms"
                : "Retry later";

            throw new InvalidOperationException($"{a} + {b} cannot be calculated at this time. {retryMessage}");
        }
    }
}