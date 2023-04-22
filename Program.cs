// See https://aka.ms/new-console-template for more information
using System.Threading.RateLimiting;
using LazyCalculator;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var options = new SlidingWindowRateLimiterOptions
{
    PermitLimit = 1,
    Window = TimeSpan.FromSeconds(3),
    SegmentsPerWindow = 1,
    QueueLimit = 2,
    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
};

var rateLimiter = new SlidingWindowRateLimiter(options);

var calculator = new Calculator(rateLimiter);

var tasks = Enumerable.Range(0, 5)
    .Select(_ => AddRandomNumbersAsync(calculator));

await Task.WhenAll(tasks);

static async Task AddRandomNumbersAsync(Calculator calculator)
{
    try
    {
        var a = new Random().Next(1, 10);
        var b = new Random().Next(1, 10);

        var result = await calculator.AddAsync(a, b);

        Log.Information("Result for {a} and {b} is {result}", a, b, result);
    }
    catch (InvalidOperationException)
    {
        Log.Error("Calculator is busy");
    }
}