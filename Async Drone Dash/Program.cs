using AsyncDroneDash.ControlTower;

ControlTowerServer server =
    new ControlTowerServer("http://localhost:5000/");

await server.StartAsync();