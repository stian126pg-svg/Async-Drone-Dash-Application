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

    private class WeatherResponse
    {
        public string? Weather { get; set; }
    }
}