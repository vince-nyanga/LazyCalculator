// See https://aka.ms/new-console-template for more information
using System.Threading.RateLimiting;
using LazyCalculator;

Console.WriteLine("Hello, World!");

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

var tasks = new List<Task>();

for (int i = 0; i < 3; i++)
{
    tasks.Add(Task.Run(async () =>
    {
        try
        {
            var a = new Random().Next(1, 10);
            var b = new Random().Next(1, 10);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Requesting result for  {a} and {b}");

            var result = await calculator.AddAsync(a, b);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Result for {a} and {b} is {result}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Console.ResetColor();
        }
    }));
}

await Task.WhenAll(tasks);