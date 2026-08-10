using AsyncDroneDash.Models;

DroneModel drone = new DroneModel("Falcon-1", 5, 500);

Console.WriteLine($"Drone: {drone.Name}");
Console.WriteLine($"Checkpoints: {drone.MaxCheckpoints}");
Console.WriteLine($"Delay: {drone.DelayMs} ms");