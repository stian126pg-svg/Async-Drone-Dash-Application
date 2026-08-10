using AsyncDroneDash.Models;
using AsyncDroneDash.Utilities;

namespace AsyncDroneDash.Services;

public class ThreadRaceService
{
    public void FlyDrone(DroneModel drone)
    {
        DroneLogger.Log($"{drone.Name} launched!");

        for (int checkpoint = 0;
             checkpoint <= drone.MaxCheckpoints;
             checkpoint++)
        {
            DroneLogger.Log(
                $"{drone.Name} → checkpoint {checkpoint}");

            Thread.Sleep(drone.DelayMs);
        }

        DroneLogger.Log($"{drone.Name} landed!");
    }
}