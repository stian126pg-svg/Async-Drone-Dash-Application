namespace AsyncDroneDash.Utilities;

public static class DroneLogger
{
    public static void Log(string message)
    {
        Console.WriteLine(
            $"[{DateTime.Now:HH:mm:ss.ffff}] {message}");
    }
}