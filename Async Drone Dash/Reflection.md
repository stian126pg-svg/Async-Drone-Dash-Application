# Async Drone Dash – Reflection


## Part A – Thread + Join

### What happened when Join was removed?

When `Join()` was removed, the main thread did not wait for the drone
threads to finish.

This caused `"All drones finished!"` to sometimes be printed before the
drones had completed their routes... In fact, it would print before the drones even took off.

With `Join()`, the main thread waits until both drone threads have completed
before continuing.

The output from Falcon-1 and Raven-2 was also "interleaved" and not completely
deterministic. Both threads wrote to the shared Console, so the exact order
of messages varied between runs.

This demonstrated that manually working with multiple threads requires
careful coordination.

---

## Part B – async/await

The drone flight was rewritten as an asynchronous method using
`await Task.Delay()` instead of `Thread.Sleep()`.

Multiple drone operations could then be started as Tasks and coordinated
using:

`await Task.WhenAll(...)`

The checkpoint output still overlapped because both drone operations were
progressing during the same period.

Falcon-1 generally completed first because it had a shorter delay, but
`Task.WhenAll()` did not allow the orchestration to continue normally until
all drone Tasks had completed.

### Exception handling in Thread vs Task

I tried to throw a deliberate exception inside one of the raw Threads in Part A.

Unlike a Task, the Thread did not store the exception so that it could later
be observed through Join(). The exception remained unhandled on that thread
and caused the application to terminate.

In the async version, exceptions are captured by the Task and propagate
through await, which allows the orchestration code to handle them with a
normal try/catch.

This made Task-based error handling easier to coordinate than raw Thread
exceptions.

### Error propagation

A simulated motor failure was added to Raven-2 at checkpoint 3.

When Raven-2 threw an `InvalidOperationException`, its Task became faulted.
Falcon-1 was not automatically cancelled and continued until its own route
was complete.

The exception then propagated through `await Task.WhenAll()` and could be
handled using a normal `try/catch`.

During an earlier experiment using `.Wait()`, the exception was wrapped in
an `AggregateException`. Using `await` exposed the original exception more
naturally and made the error handling a lot easier to understand.

### Thread + Join compared with async/await

The Thread version required manually creating threads, starting them and
calling `Join()` to see them to their completion.

The async version represented the operations using Tasks and coordinated
them using `await Task.WhenAll()`.

This required less orchestration code and made the intent of the
program easier and smoother to read.

For operations that spend time waiting, such as delays or HTTP requests,
async/await also avoids occupying a thread simply to wait for the operation
to finish.

Overall? The async version was easier to read, compose and maintain.

---

## Part C – Control Tower API

I decided to implement the optional local Control Tower using `HttpListener`.

The server exposes two endpoints:

- `/weather`
- `/route?drone=Name`

The weather endpoint returns `clear`, `wind` or `storm`.

The route endpoint returns the number of checkpoints assigned to a known
drone.

Artificial network delay was added to the server so that asynchronous HTTP
behavior could be observed more clearly.

### HttpClient

The client communicates with the Control Tower using `HttpClient`.

The HTTP calls are asynchronous and the JSON responses are deserialized
into C# objects.

The weather and route requests are independent, so they were started
together and coordinated with `Task.WhenAll()`.

When they were "awaited" sequentially, the second request did not begin until
the first had completed. When they were started together, their waiting time
could overlap and the total time was approximately determined by the slower
request instead of the sum of both delays.

### External data affecting the simulation

The HTTP responses were connected to the drone simulation rather than only
being displayed.

The route response determines `MaxCheckpoints`.

Weather changes the drone delay:

- clear: no additional delay
- wind: +250 ms
- storm: +750 ms

For example, when Falcon-1 received storm weather, its normal 500 ms delay
became 1250 ms. This difference was visible in the timestamps between
checkpoints.

This demonstrated how data retrieved asynchronously from another service
can affect application behavior.

### HTTP errors and timeouts

The Control Tower returns HTTP 404 when an unknown drone requests a route
and HTTP 400 when required route information is missing.

The client checks for failed HTTP responses and returns a fallback value
instead of allowing the application to crash.

`HttpClient` also has a five-second timeout.

During testing, the server was deliberately changed to respond after 7
seconds. Both HTTP requests timed out, but the exceptions were handled and
the drone kept to its existing route and delay values.

This allowed the simulation to continue even when the Control Tower was
unavailable or too slow.

### Invalid drone data

Validation was added before an asynchronous drone flight begins.

An empty drone name is rejected with an `ArgumentException`.

Negative checkpoint or delay values are rejected with
`ArgumentOutOfRangeException`.

This prevents "invalid state" from reaching the actual flight logic.

---

## Bonus – CancellationToken

Cancellation support was added using `CancellationTokenSource` and
`CancellationToken`.

The same cancellation token can be supplied to multiple drone Tasks.

During the emergency-abort demonstration, Falcon-1 and Raven-2 fly
concurrently while another Task listens for the user to press `C`.

Pressing `C` calls `Cancel()` on the shared `CancellationTokenSource`.

The token is passed to `Task.Delay()` inside the drone flight. When
cancellation is requested, the asynchronous delay responds by throwing an
`OperationCanceledException`.

The orchestration catches this exception and reports that all active drone
flights were cancelled.

Neither drone will report that they landed after the cancellation.

This demonstrated cooperative cancellation: the Tasks are not forcibly
terminated. Instead, cancellation is requested and the asynchronous
operations cooperate with that request.

---

## Two problems caused by blocking asynchronous code

### 1. Blocking a UI or request thread

Using `.Wait()` or `.Result` can block the thread that is responsible for
processing other work.

In a UI application this can make the interface freeze. In a server
application it can reduce the number of requests the application can handle
efficiently.

### 2. Deadlocks and unnecessary resource usage

Blocking while waiting for asynchronous work can contribute to deadlocks in
environments with a synchronization context.

Even when a deadlock does not occur, blocking wastes a thread that could
otherwise perform useful work.

For asynchronous workflows it is therefore generally better to propagate
async upward and use `await`.

---

## Final thoughts

The biggest difference I observed was that Thread-based code required me to
think much more directly about the threads themselves.

With async/await, I could focus more on the operations I wanted to perform
and how they should be coordinated.

The HTTP section made this especially clear because waiting for external
data is a natural use case for asynchronous programming.

The project also demonstrated that asynchronous programming involves more
than simply adding `async` and `await`. Error propagation, timeouts,
resource cleanup, validation and cancellation all need to be considered
when building a reliable asynchronous application.