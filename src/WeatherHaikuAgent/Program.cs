using WeatherHaikuAgent;

var mode = args.Length > 0 ? args[0].ToLower() : "run";

try
{
    var config = ConfigLoader.Load();
    var runner = new AppRunner(config);

    switch (mode)
    {
        case "run":
            await runner.RunAsync();
            break;

        case "test-email":
            await runner.TestEmailAsync();
            break;

        case "dump-config":
            runner.DumpConfig();
            break;

        default:
            Console.WriteLine($"Unknown mode: {mode}");
            Console.WriteLine("Usage: WeatherHaikuAgent [run|test-email|dump-config]");
            Environment.Exit(1);
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}");
    if (args.Contains("--verbose"))
    {
        Console.WriteLine(ex.StackTrace);
    }
    Environment.Exit(1);
}
