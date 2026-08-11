using AsyncDroneDash.Models;
using AsyncDroneDash.Utilities;

namespace AsyncDroneDash.Services;

public class TaskDroneService
{
    public Task FlyDrone(DroneModel drone)
    {
        TaskCompletionSource<bool> completionSource = new();

        Thread thread = new Thread(() =>
        {
            try
            {
                DroneLogger.Log($"{drone.Name} launched!");

                for (int checkpoint = 0;
                     checkpoint <= drone.MaxCheckpoints;
                     checkpoint++)
                {
                    if (drone.Name == "Raven-2" && checkpoint == 3)
                    {
                        throw new InvalidOperationException(
                            $"{drone.Name} suffered a motor failure at checkpoint {checkpoint}!");
                    }

                    DroneLogger.Log(
                        $"{drone.Name} → checkpoint {checkpoint}");

                    Thread.Sleep(drone.DelayMs);
                }

                DroneLogger.Log($"{drone.Name} landed!");

                completionSource.SetResult(true);
            }
            catch (Exception exception)
            {
                completionSource.SetException(exception);
            }
        });

        thread.Start();

        return completionSource.Task;
    }
}