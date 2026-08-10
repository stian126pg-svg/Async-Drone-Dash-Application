namespace AsyncDroneDash.Models;

public class DroneModel
{
    public string Name { get; set; }
    public int MaxCheckpoints { get; set; }
    public int DelayMs { get; set; }
    
    public DroneModel(string name, int maxCheckpoints, int delayMs)
    {
        Name = name;
        MaxCheckpoints = maxCheckpoints;
        DelayMs = delayMs;
    }
}