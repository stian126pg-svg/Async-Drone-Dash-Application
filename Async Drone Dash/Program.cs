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

string? weather =
    await towerClient.GetWeatherAsync();

Console.WriteLine($"Drone received weather: {weather}");

Console.WriteLine();
Console.WriteLine("Press Enter to stop...");
Console.ReadLine();