using AsyncDroneDash.Models;
using AsyncDroneDash.Services;

DroneModel falcon = new DroneModel(
    "Falcon-1",
    5,
    500);

DroneModel raven = new DroneModel(
    "Raven-2",
    5,
    700);

TaskDroneService service = new TaskDroneService();

Task falconTask = service.FlyDrone(falcon);
Task ravenTask = service.FlyDrone(raven);

Task allDrones = Task.WhenAll(falconTask, ravenTask);

try
{
    allDrones.Wait();

    Console.WriteLine("All drone tasks finished successfully!");
}
catch (AggregateException exception)
{
    Console.WriteLine("One or more drones failed!");

    foreach (Exception innerException in exception.InnerExceptions)
    {
        Console.WriteLine($"Error: {innerException.Message}");
    }
}