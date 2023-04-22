using System.Threading.RateLimiting;
using static System.Console;

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
            ForegroundColor = ConsoleColor.Magenta;
            WriteLine($"I am going to add two numbers {a} and {b}");
            ResetColor();

            using var lease = await _rateLimiter.AcquireAsync(permitCount: 1);

            if (lease.IsAcquired)
            {
                return a + b;
            }

            var retryMessage = lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                ? $"Retry after {retryAfterValue} ms"
                : "Retry later";

            throw new InvalidOperationException($"{a} + {b} cannot be calculated at this time. {retryMessage}");
        }
    }
}