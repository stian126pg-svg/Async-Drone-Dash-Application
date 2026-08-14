using AsyncDroneDash.ControlTower;
using AsyncDroneDash.Models;
using AsyncDroneDash.Services;

bool running = true;

while (running)
{
    Console.Clear();

    Console.WriteLine("================================");
    Console.WriteLine("       ASYNC DRONE DASH");
    Console.WriteLine("================================");
    Console.WriteLine();
    Console.WriteLine("1. Part A - Thread + Join");
    Console.WriteLine("2. Part B - Async success");
    Console.WriteLine("3. Part B - Async motor failure");
    Console.WriteLine("4. Part C - Control Tower API");
    Console.WriteLine("5. Bonus  - Emergency Abort");
    Console.WriteLine("0. Exit");
    Console.WriteLine();
    Console.Write("Choose an option: ");

    string? choice = Console.ReadLine();

    Console.Clear();

    switch (choice)
    {
        case "1":
            RunPartA();
            break;

        case "2":
            await RunPartBSuccessAsync();
            break; 

        case "3":
            await RunPartBFailureAsync();
            break;

        case "4":
            await RunPartCAsync();
            break;

        case "5":
            await RunEmergencyAbortAsync();
            break;

        case "0":
            running = false;
            break;

        default:
            Console.WriteLine("Invalid option.");
            Pause();
            break;
    }
}


static void RunPartA()
{
    Console.WriteLine("================================");
    Console.WriteLine("    PART A - THREAD + JOIN");
    Console.WriteLine("================================");
    Console.WriteLine();

    DroneModel falcon = new DroneModel(
        "Falcon-1",
        5,
        500);

    DroneModel raven = new DroneModel(
        "Raven-2",
        5,
        700);

    ThreadRaceService service =
        new ThreadRaceService();

    Thread falconThread = new Thread(() =>
    {
        service.FlyDrone(falcon);
    });

    Thread ravenThread = new Thread(() =>
    {
        service.FlyDrone(raven);
    });

    falconThread.Start();
    ravenThread.Start();

    falconThread.Join();
    ravenThread.Join();

    Console.WriteLine();
    Console.WriteLine("All drones finished!");

    Pause();
}


static async Task RunPartBSuccessAsync()
{
    Console.WriteLine("================================");
    Console.WriteLine("    PART B - ASYNC SUCCESS");
    Console.WriteLine("================================");
    Console.WriteLine();

    DroneModel falcon = new DroneModel(
        "Falcon-1",
        5,
        500);

    DroneModel raven = new DroneModel(
        "Raven-2",
        5,
        700);

    AsyncDroneService service =
        new AsyncDroneService();

    Task falconTask =
        service.FlyDroneAsync(falcon);

    Task ravenTask =
        service.FlyDroneAsync(raven);

    try
    {
        await Task.WhenAll(
            falconTask,
            ravenTask);

        Console.WriteLine();
        Console.WriteLine(
            "All drones finished successfully!");
    }
    catch (Exception exception)
    {
        Console.WriteLine();
        Console.WriteLine(
            $"Unexpected error: {exception.Message}");
    }

    Pause();
}


static async Task RunPartBFailureAsync()
{
    Console.WriteLine("================================");
    Console.WriteLine("   PART B - MOTOR FAILURE");
    Console.WriteLine("================================");
    Console.WriteLine();

    DroneModel falcon = new DroneModel(
        "Falcon-1",
        5,
        500);

    DroneModel raven = new DroneModel(
        "Raven-2",
        5,
        700);

    AsyncDroneService service =
        new AsyncDroneService();

    Task falconTask =
        service.FlyDroneAsync(falcon);

    Task ravenTask =
        service.FlyDroneAsync(
            raven,
            failAtCheckpoint: 3);

    try
    {
        await Task.WhenAll(
            falconTask,
            ravenTask);

        Console.WriteLine();
        Console.WriteLine(
            "All drones finished successfully!");
    }
    catch (InvalidOperationException exception)
    {
        Console.WriteLine();
        Console.WriteLine(
            "A drone failed!");

        Console.WriteLine(
            $"Error: {exception.Message}");
    }

    Pause();
}


static async Task RunPartCAsync()
{
    Console.WriteLine("================================");
    Console.WriteLine("   PART C - CONTROL TOWER API");
    Console.WriteLine("================================");
    Console.WriteLine();

    ControlTowerServer server =
        new ControlTowerServer("http://localhost:5000/");

    Task serverTask = server.StartAsync();

    try
    {
        await Task.Delay(500);

        HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000/"),
            Timeout = TimeSpan.FromSeconds(5)
        };

        ControlTowerClient towerClient =
            new ControlTowerClient(httpClient);

        AsyncDroneService droneService =
            new AsyncDroneService();

        DroneModel falcon = new DroneModel(
            "Falcon-1",
            5,
            500);

        Task<string?> weatherTask =
            towerClient.GetWeatherAsync();

        Task<int?> routeTask =
            towerClient.GetRouteAsync(falcon.Name);

        await Task.WhenAll(
            weatherTask,
            routeTask);

        string? weather =
            await weatherTask;

        int? checkpoints =
            await routeTask;

        if (checkpoints.HasValue)
        {
            falcon.MaxCheckpoints =
                checkpoints.Value;
        }

        falcon.DelayMs = weather switch
        {
            "clear" => falcon.DelayMs,
            "wind" => falcon.DelayMs + 250,
            "storm" => falcon.DelayMs + 750,
            _ => falcon.DelayMs
        };

        Console.WriteLine();
        Console.WriteLine(
            $"Weather: {weather ?? "unavailable"}");

        Console.WriteLine(
            $"Route checkpoints: {falcon.MaxCheckpoints}");

        Console.WriteLine(
            $"Adjusted delay: {falcon.DelayMs} ms");

        Console.WriteLine();

        try
        {
            await droneService.FlyDroneAsync(falcon);

            Console.WriteLine();
            Console.WriteLine(
                "Drone delivery completed!");
        }
        catch (Exception exception)
        {
            Console.WriteLine();
            Console.WriteLine(
                $"Drone delivery failed: {exception.Message}");
        }
    }
    finally
    {
        server.Stop();
    }

    Pause();
}


static async Task RunEmergencyAbortAsync()
{
    Console.WriteLine("================================");
    Console.WriteLine("    BONUS - EMERGENCY ABORT");
    Console.WriteLine("================================");
    Console.WriteLine();

    DroneModel falcon = new DroneModel(
        "Falcon-1",
        10,
        750);

    DroneModel raven = new DroneModel(
        "Raven-2",
        10,
        900);

    AsyncDroneService service =
        new AsyncDroneService();

    using CancellationTokenSource cancellationSource =
        new CancellationTokenSource();

    Console.WriteLine("Press C to cancel all drone flights.");
    Console.WriteLine();

    Task cancellationTask = Task.Run(() =>
    {
        while (!cancellationSource.IsCancellationRequested)
        {
            ConsoleKeyInfo key =
                Console.ReadKey(true);

            if (key.Key == ConsoleKey.C)
            {
                cancellationSource.Cancel();
                break;
            }
        }
    });

    Task falconTask =
        service.FlyDroneAsync(
            falcon,
            cancellationToken: cancellationSource.Token);

    Task ravenTask =
        service.FlyDroneAsync(
            raven,
            cancellationToken: cancellationSource.Token);

    try
    {
        await Task.WhenAll(
            falconTask,
            ravenTask);

        Console.WriteLine();
        Console.WriteLine(
            "All drone deliveries completed!");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine();
        Console.WriteLine(
            "EMERGENCY ABORT ACTIVATED!");

        Console.WriteLine(
            "All active drone flights cancelled.");
    }

    Pause();
}


static void Pause()
{
    Console.WriteLine();
    Console.WriteLine("Press Enter to return to the menu...");
    Console.ReadLine();
}