using AsyncDroneDash.Models;
using AsyncDroneDash.Services;

DroneModel drone = new DroneModel(
    "Falcon-1",
    5,
    500);

TaskDroneService service = new TaskDroneService();

Task droneTask = service.FlyDrone(drone);

droneTask.Wait();

Console.WriteLine("Drone task finished!");