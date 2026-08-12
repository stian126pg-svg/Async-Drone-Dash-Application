using System.Net;
using System.Text;
using System.Text.Json;

namespace AsyncDroneDash.ControlTower;

public class ControlTowerServer
{
    private readonly HttpListener _listener;

    public ControlTowerServer(string prefix)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
    }

    public async Task StartAsync()
    {
        _listener.Start();

        Console.WriteLine("Control Tower online.");

        while (_listener.IsListening)
        {
            HttpListenerContext context =
                await _listener.GetContextAsync();

            _ = HandleRequestAsync(context);
        }
    }

    private async Task HandleRequestAsync(
        HttpListenerContext context)
    {
        try
        {
            string? rawUrl = context.Request.RawUrl;

            if (rawUrl == null)
            {
                await SendResponseAsync(
                    context,
                    400,
                    new { error = "Invalid request." });

                return;
            }

            if (rawUrl.StartsWith("/route"))
            {
                await HandleRouteAsync(context);
            }
            else if (rawUrl.StartsWith("/weather"))
            {
                await HandleWeatherAsync(context);
            }
            else
            {
                await SendResponseAsync(
                    context,
                    404,
                    new { error = "Endpoint not found." });
            }
        }
        catch (Exception exception)
        {
            await SendResponseAsync(
                context,
                500,
                new { error = exception.Message });
        }
    }

    private async Task HandleRouteAsync(
        HttpListenerContext context)
    {
        string? droneName =
            context.Request.QueryString["drone"];

        if (string.IsNullOrWhiteSpace(droneName))
        {
            await SendResponseAsync(
                context,
                400,
                new { error = "Drone name is required." });

            return;
        }

        int? checkpoints = droneName switch
        {
            "Falcon-1" => 5,
            "Raven-2" => 6,
            _ => null
        };

        if (checkpoints == null)
        {
            await SendResponseAsync(
                context,
                404,
                new { error = $"Unknown drone: {droneName}" });

            return;
        }

        await SimulateNetworkDelayAsync();

        await SendResponseAsync(
            context,
            200,
            new
            {
                drone = droneName,
                maxCheckpoints = checkpoints
            });
    }

    private async Task HandleWeatherAsync(
        HttpListenerContext context)
    {
        string[] weatherOptions =
        {
            "clear",
            "wind",
            "storm"
        };

        string weather =
            weatherOptions[Random.Shared.Next(weatherOptions.Length)];

        await SimulateNetworkDelayAsync();

        await SendResponseAsync(
            context,
            200,
            new
            {
                weather
            });
    }

    private static async Task SimulateNetworkDelayAsync()
    {
        int delay = 7000;

        await Task.Delay(delay);
    }

    private static async Task SendResponseAsync(
        HttpListenerContext context,
        int statusCode,
        object data)
    {
        string json =
            JsonSerializer.Serialize(data);

        byte[] buffer =
            Encoding.UTF8.GetBytes(json);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType =
            "application/json";

        context.Response.ContentLength64 =
            buffer.Length;

        await context.Response.OutputStream.WriteAsync(buffer);

        context.Response.Close();
    }
}