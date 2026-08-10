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

ThreadRaceService service = new ThreadRaceService();

Thread falconThread = new Thread(() =>
{
    service.FlyDrone(falcon);
});

Thread ravenThread = new Thread(() =>
{
    service.FlyDrone(raven);
});

falconThread.Start();
ravenThread.Start();

falconThread.Join();
ravenThread.Join();

Console.WriteLine("All drones finished!");