using AsyncDroneDash.Models;
using AsyncDroneDash.Utilities;

namespace AsyncDroneDash.Services;

public class AsyncDroneService
{
    public async Task FlyDroneAsync(
        DroneModel drone,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(drone.Name))
        {
            throw new ArgumentException(
                "Drone name cannot be empty.");
        }

        if (drone.MaxCheckpoints < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(drone.MaxCheckpoints),
                "Max checkpoints cannot be negative.");
        }

        if (drone.DelayMs < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(drone.DelayMs),
                "Drone delay cannot be negative.");
        }

        DroneLogger.Log($"{drone.Name} launched!");

        for (int checkpoint = 0;
             checkpoint <= drone.MaxCheckpoints;
             checkpoint++)
        {
            DroneLogger.Log(
                $"{drone.Name} → checkpoint {checkpoint}");

            await Task.Delay(
                drone.DelayMs,
                cancellationToken);
        }

        DroneLogger.Log($"{drone.Name} landed!");
    }
}