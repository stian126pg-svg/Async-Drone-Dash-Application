using System.Net.Http.Json;
using AsyncDroneDash.Utilities;

namespace AsyncDroneDash.Services;

public class ControlTowerClient
{
    private readonly HttpClient _httpClient;

    public ControlTowerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetWeatherAsync()
    {
        DroneLogger.Log(
            "Requesting weather from Control Tower...");

        WeatherResponse? response =
            await _httpClient.GetFromJsonAsync<WeatherResponse>(
                "weather");

        DroneLogger.Log(
            $"Control Tower reported weather: {response?.Weather}");

        return response?.Weather;
    }

    public async Task<int?> GetRouteAsync(string droneName)
    {
        DroneLogger.Log(
            $"Requesting route for {droneName} from Control Tower...");

        RouteResponse? response =
            await _httpClient.GetFromJsonAsync<RouteResponse>(
                $"route?drone={Uri.EscapeDataString(droneName)}");

        DroneLogger.Log(
            $"Control Tower assigned {response?.MaxCheckpoints} checkpoints to {droneName}.");

        return response?.MaxCheckpoints;
    }

    private class WeatherResponse
    {
        public string? Weather { get; set; }
    }

    private class RouteResponse
    {
        public string? Drone { get; set; }
        public int MaxCheckpoints { get; set; }
    }
}