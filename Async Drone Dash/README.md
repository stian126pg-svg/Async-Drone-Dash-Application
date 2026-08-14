# Async Drone Dash

Async Drone Dash is a small C# console application created to explore and compare different approaches to concurrency and asynchronous programming in .NET.

The application simulates "delivery drones" travelling through checkpoints while demonstrating:

- `Thread` and `Join`
- `Task`
- `async` / `await`
- `Task.WhenAll`
- asynchronous exception handling
- `HttpClient`
- a locally hosted Control Tower API using `HttpListener`
- HTTP error and timeout handling
- `CancellationToken`

The project also comes with an interactive menu so each part can be tested separately!

---

## Requirements

- .NET 10 SDK
- A terminal or IDE capable of running .NET console applications

Check your installed .NET version with:

```powershell
dotnet --version
```

---

## Running the application

Navigate to the directory containing the `.csproj` file.

For example:

```powershell
cd "Async Drone Dash"
```

Then run:

```powershell
dotnet run
```

You can also verify that the project builds successfully with:

```powershell
dotnet build
```

---

## Main Menu

When the application starts, the following menu is displayed:

```text
================================
       ASYNC DRONE DASH
================================

1. Part A - Thread + Join
2. Part B - Async success
3. Part B - Async motor failure
4. Part C - Control Tower API
5. Bonus  - Emergency Abort
0. Exit
```

Each option demonstrates a different part of the assignment.

---

# Part A – Thread + Join

Part A demonstrates manual thread management.

Two drones, Falcon-1 and Raven-2, are started on separate `Thread` instances.

Each drone:

1. launches
2. travels through its checkpoints
3. waits between checkpoints
4. lands

The main thread uses:

```csharp
thread.Join();
```

to wait for both drone threads before printing:

```text
All drones finished!
```

During development, `Join()` was temporarily removed to observe what happens when the main thread does not wait for the worker threads.

Without `Join()`, the final message could be printed before the drones had completed their routes.

The output from multiple threads may also appear in different orders because both threads write to the shared console.

---

# Part B – Async Orchestration

Part B replaces manual thread handling with modern asynchronous programming.

Drone flights are implemented using:

```csharp
async
await
Task.Delay()
```

Multiple drones are started and coordinated with:

```csharp
await Task.WhenAll(...);
```

This allows both drone operations to progress concurrently without manually creating and joining threads.

## Async success

Menu option 2 runs Falcon-1 and Raven-2 normally.

The application waits asynchronously until both drone Tasks have completed.

## Motor failure

Menu option 3 demonstrates asynchronous exception propagation.

Raven-2 is configured to suffer a simulated motor failure at a checkpoint.

The exception propagates through its `Task` and is handled by the orchestration code using `try/catch`.

This demonstrates how errors from asynchronous operations can be handled without manually coordinating exceptions between threads.

---

# Part C – Control Tower API

Part C implements the optional Control Tower HTTP service.

The application hosts a local HTTP server using:

```csharp
HttpListener
```

The drone client communicates with it asynchronously using:

```csharp
HttpClient
```

The Control Tower runs locally at:

```text
http://localhost:5000/
```

It is automatically started when Part C begins and stopped when Part C finishes.

No separate server process needs to be started manually.

## Endpoints

### Weather

```text
/weather
```

Returns one of:

```text
clear
wind
storm
```

Weather affects the delay between drone checkpoints.

| Weather | Delay adjustment |
| --- | ---: |
| clear | +0 ms |
| wind | +250 ms |
| storm | +750 ms |

For example, Falcon-1 normally has a delay of 500 ms.

During a storm:

```text
500 ms + 750 ms = 1250 ms
```

### Route

```text
/route?drone=Falcon-1
```

Returns route information for a known drone.

The returned checkpoint count is used by the simulation as the drone's `MaxCheckpoints`.

Unknown drones are rejected with an HTTP error instead of receiving an invented route.

---

## Concurrent HTTP requests

Weather and route information are independent requests.

They are therefore started together and coordinated using:

```csharp
await Task.WhenAll(...);
```

This allows their waiting time to overlap instead of performing both HTTP requests sequentially.

Artificial network delay is included in the Control Tower to make this behavior easier to observe.

---

## HTTP errors and timeouts

The HTTP client includes error handling for unsuccessful requests and network problems.

Examples tested during development include:

- unknown drone
- missing drone name
- HTTP 400 Bad Request
- HTTP 404 Not Found
- request timeout

`HttpClient` uses a five-second timeout.

During testing, the Control Tower was deliberately configured to take longer than the timeout. The client handled the timeout without crashing and the drone continued using its existing values.

---

# Validation

Drone data is validated before a flight begins.

The application rejects:

- empty drone names
- negative checkpoint values
- negative delay values

Invalid data produces clear exceptions rather than allowing invalid state to continue into the flight simulation.

---

# Bonus – Emergency Abort

The project includes cancellation support using:

```csharp
CancellationTokenSource
CancellationToken
```

Select:

```text
5. Bonus - Emergency Abort
```

to start multiple drone flights.

While the drones are flying, press:

```text
C
```

to request cancellation.

Both drones receive the same `CancellationToken`, allowing one cancellation request to stop all active flights.

The token is passed into:

```csharp
Task.Delay()
```

so the asynchronous wait can respond immediately to cancellation.

The application catches the resulting `OperationCanceledException` and reports:

```text
EMERGENCY ABORT ACTIVATED!
All active drone flights cancelled.
```

This demonstrates cooperative cancellation rather than forcibly terminating running Tasks.

---

# Project Structure

The project separates models, services, utilities and the Control Tower implementation.

```text
Async Drone Dash/
│
├── ControlTower/
│   └── ControlTowerServer.cs
│
├── Models/
│   └── DroneModel.cs
│
├── Services/
│   ├── AsyncDroneService.cs
│   ├── ControlTowerClient.cs
|   ├── TaskDroneService.cs 
│   └── ThreadRaceService.cs
│
├── Utilities/
│   └── DroneLogger.cs
│
├── Program.cs
├── reflection.md
└── README.md
```

The exact project structure may be extended or altered, if the application sees further development.

---

# Key Concepts Demonstrated

The project demonstrates several important asynchronous programming concepts:

- manual thread creation
- waiting for threads using `Join`
- Tasks
- `async` / `await`
- non-blocking delays
- `Task.WhenAll`
- exception propagation through Tasks
- asynchronous HTTP communication
- JSON serialization/deserialization
- HTTP status-code handling
- timeouts
- resource cleanup
- input validation
- cooperative cancellation

---

# Reflection

Additional observations from development and comparisons between `Thread`/`Join` and `async`/`await` can be found in:

```text
reflection.md
```

This includes observations from intentionally removing `Join`, triggering motor failures, forcing HTTP timeouts, requesting unknown drones and cancelling active flights.