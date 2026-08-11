using AsyncDroneDash.Models;
using AsyncDroneDash.Utilities;

namespace AsyncDroneDash.Services;

public class AsyncDroneService
{
    public async Task FlyDroneAsync(DroneModel drone)
    {
        DroneLogger.Log($"{drone.Name} launched!");

        for (int checkpoint = 0;
             checkpoint <= drone.MaxCheckpoints;
             checkpoint++)
        {
            DroneLogger.Log(
                $"{drone.Name} → checkpoint {checkpoint}");

            await Task.Delay(drone.DelayMs);
        }

        DroneLogger.Log($"{drone.Name} landed!");
    }
}