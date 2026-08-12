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

        try
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync(
                    $"route?drone={Uri.EscapeDataString(droneName)}");

            if (!response.IsSuccessStatusCode)
            {
                DroneLogger.Log(
                    $"Control Tower could not find route for {droneName}. " +
                    $"Status: {(int)response.StatusCode} {response.StatusCode}");

                return null;
            }

            RouteResponse? route =
                await response.Content.ReadFromJsonAsync<RouteResponse>();

            DroneLogger.Log(
                $"Control Tower assigned {route?.MaxCheckpoints} checkpoints to {droneName}.");

            return route?.MaxCheckpoints;
        }
        
        catch (HttpRequestException exception)
        {
            DroneLogger.Log(
                $"Route request failed: {exception.Message}");

            return null;
        }
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