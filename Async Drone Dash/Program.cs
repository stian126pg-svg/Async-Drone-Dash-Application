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

AsyncDroneService service = new AsyncDroneService();

Task falconTask = service.FlyDroneAsync(falcon);
Task ravenTask = service.FlyDroneAsync(raven);

try
{
    await Task.WhenAll(falconTask, ravenTask);

    Console.WriteLine("All drones finished successfully!");
}
catch (InvalidOperationException exception)
{
    Console.WriteLine("A drone failed!");
    Console.WriteLine($"Error: {exception.Message}");
}