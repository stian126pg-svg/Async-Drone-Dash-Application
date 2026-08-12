using AsyncDroneDash.ControlTower;
using AsyncDroneDash.Services;

ControlTowerServer server =
    new ControlTowerServer("http://localhost:5000/");

Task serverTask = server.StartAsync();

await Task.Delay(500);

HttpClient httpClient = new HttpClient
{
    BaseAddress = new Uri("http://localhost:5000/"),
    Timeout = TimeSpan.FromSeconds(5)
};

ControlTowerClient towerClient =
    new ControlTowerClient(httpClient);

Task<string?> weatherTask =
    towerClient.GetWeatherAsync();

Task<int?> routeTask =
    towerClient.GetRouteAsync("Falcon-1");

await Task.WhenAll(weatherTask, routeTask);

string? weather = await weatherTask;
int? checkpoints = await routeTask;

Console.WriteLine();
Console.WriteLine($"Weather: {weather}");
Console.WriteLine($"Falcon-1 checkpoints: {checkpoints}");

Console.WriteLine();
Console.WriteLine("Press Enter to stop...");
Console.ReadLine();