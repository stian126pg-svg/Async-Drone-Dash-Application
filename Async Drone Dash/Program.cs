using AsyncDroneDash.Models;
using AsyncDroneDash.Services;

DroneModel falcon = new DroneModel(
    "Falcon-1",
    5,
    500);

AsyncDroneService service = new AsyncDroneService();

await service.FlyDroneAsync(falcon);

Console.WriteLine("Drone task finished!");

