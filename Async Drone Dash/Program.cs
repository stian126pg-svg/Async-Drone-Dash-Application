using AsyncDroneDash.ControlTower;
using AsyncDroneDash.Models;
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

AsyncDroneService droneService =
    new AsyncDroneService();

DroneModel falcon = new DroneModel(
    "Falcon-1",
    5,
    500);

Task<string?> weatherTask =
    towerClient.GetWeatherAsync();

Task<int?> routeTask =
    towerClient.GetRouteAsync(falcon.Name);

await Task.WhenAll(weatherTask, routeTask);

string? weather = await weatherTask;
int? checkpoints = await routeTask;

if (checkpoints.HasValue)
{
    falcon.MaxCheckpoints = checkpoints.Value;
}

falcon.DelayMs = weather switch
{
    "clear" => falcon.DelayMs,
    "wind" => falcon.DelayMs + 250,
    "storm" => falcon.DelayMs + 750,
    _ => falcon.DelayMs
};

Console.WriteLine();
Console.WriteLine($"Weather: {weather}");
Console.WriteLine($"Route checkpoints: {falcon.MaxCheckpoints}");
Console.WriteLine($"Adjusted delay: {falcon.DelayMs} ms");
Console.WriteLine();

using CancellationTokenSource cancellationSource =
    new CancellationTokenSource();

cancellationSource.CancelAfter(
    TimeSpan.FromSeconds(2));

try
{
    await droneService.FlyDroneAsync(
        falcon,
        cancellationSource.Token);

    Console.WriteLine();
    Console.WriteLine("Drone delivery completed!");
}

catch (OperationCanceledException)
{
    Console.WriteLine();
    Console.WriteLine("Drone flight cancelled!");
}

Console.WriteLine();
Console.WriteLine("Press Enter to stop...");
Console.ReadLine();