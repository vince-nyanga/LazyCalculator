// See https://aka.ms/new-console-template for more information
using System.Threading.RateLimiting;
using LazyCalculator;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var fixedWindowRateLimiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
{
    PermitLimit = 1,
    Window = TimeSpan.FromSeconds(5),
    QueueLimit = 2,
    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
});

var calculator = new Calculator(fixedWindowRateLimiter);

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

        Log.Information("Result for {A} and {B} is {Result}", a, b, result);
    }
    catch (InvalidOperationException)
    {
        Log.Error("Calculator is busy");
    }
}