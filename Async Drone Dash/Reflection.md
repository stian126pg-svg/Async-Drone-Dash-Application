# Reflection – Async Drone Dash



## Part A – Thread + Join

### Initial observations

The simulator uses a DroneModel to represent each drone. The model contains the drone's name, maximum number of checkpoints, and delay between checkpoints.

A separate DroneLogger class is used to keep console output consistent and to include timestamps. This will make it easier to observe how multiple drones overlap while running concurrently.

### Experiments

The logger was tested with a one-second Thread.Sleep between messages.
The timestamp showed approximately one second between the messages.

### What happened when Join was removed?

When `Thread.Join()` was removed, the main thread immediately printed
"All drones finished!" before the drone had even started its route.

The drone continued running afterward and completed all of its
checkpoints.

This demonstrated that `Thread.Start()` begins the thread independently
of the main thread. Without `Join()`, the main thread does not wait for
the drone thread to finish.

The result can therefore be in an unexpected order because the threads
are executing independently.

### Multiple drones

Two drones were started on separate threads. Falcon-1 had a delay of
500 ms per checkpoint, while Raven-2 had a delay of 700 ms.

The drones started almost simultaneously, and their console output was
interleaved. Falcon-1 generally progressed faster because it had the
shorter delay and ultimately landed first.

However, the order of individual log messages was not completely
predictable. Raven-2 printed its first checkpoint at essentially the
same time as Falcon-1, and in some cases Raven-2's message appeared
first.

This demonstrated that the threads execute independently and that
thread scheduling affects the exact ordering of operations.

Both Join() calls ensured that the main thread waited for both drones
before printing "All drones finished!".

### Removing Join() with multiple drones

After removing both Join() calls, the main thread printed
"All drones finished!" immediately, before either drone had completed
its route.

The drones continued flying afterward.

The experiment was run multiple times. The general progression was
similar because Falcon-1 had a shorter delay than Raven-2, but the exact
ordering of messages varied slightly between runs.

For example, the order in which the launch messages appeared was not
always the same as the timestamps themselves. Some messages also had
nearly identical timestamps.

This showed that console output from multiple threads can become
interleaved and that the exact execution order should not be relied
upon.

Without Join(), the main thread has no synchronization point requiring
it to wait for the drone threads. Therefore, "All drones finished!"
does not actually mean that the drones have finished; it only means
that the main thread reached that statement.



## Part B – Task + TaskCompletionSource

### Initial observations

Part B replaces the direct use of Thread.Join() with a Task that
represents the completion of the drone's work.

TaskCompletionSource is used to control when the Task completes.
SetResult() is called when the drone successfully finishes its route,
while SetException() can be used when the drone encounters an error.

The initial implementation still uses a Thread to perform the actual
work, but the TaskCompletionSource provides a Task-based way for other
code to observe when the operation has completed.

### Task.WhenAll with multiple drones

Two drone Tasks were started and combined with Task.WhenAll().

Falcon-1 finished before Raven-2 because it had a shorter delay, but
the combined Task did not complete until both drone Tasks were finished.

This showed that Task.WhenAll() can be used to coordinate several
independent operations without manually joining each thread.

Compared with Thread.Join(), the code is beginning to focus more on
waiting for work to complete rather than directly managing the threads
performing that work.

### Failure propagation with TaskCompletionSource

A simulated motor failure was added to Raven-2 at checkpoint 3.

When the exception was thrown, TaskDroneService caught it and passed it
to TaskCompletionSource.SetException(). This caused Raven-2's Task to
become faulted.

Falcon-1 was not automatically stopped by Raven-2's failure and
continued until it completed its route.

Task.WhenAll() did not finish until the remaining Falcon-1 Task was also
complete. The combined Task then completed in a faulted state.

Because Program.cs used Task.Wait() without a try/catch, the failure was
reported as an AggregateException and terminated the application. The
original InvalidOperationException was visible as the inner exception.

### Handling Task failures

The call to Task.WhenAll() was wrapped in a try/catch.

Because the program currently uses Task.Wait(), failures are exposed as
an AggregateException. The InnerExceptions collection can be inspected
to retrieve the original error from the failed drone.

This allowed the application to report Raven-2's simulated motor failure
without terminating with an unhandled exception.

This also showed that Tasks provide more information about the outcome
of an operation than directly managing threads. A Task can represent
successful completion, failure, or cancellation.

### Multiple failures

A second simulated motor failure was temporarily added so both drones
would fail.

Task.WhenAll() completed as faulted and AggregateException contained
both original exceptions. This demonstrated why AggregateException can
be useful when several concurrent Tasks fail.

The order of the reported exceptions should not be assumed to represent
the exact chronological order in which the failures occurred.

### Multiple drones with async/await

Falcon-1 and Raven-2 were started as separate asynchronous operations
and coordinated using await Task.WhenAll().

Their checkpoint output overlapped, showing that both drone operations
were making progress during the same period.

Falcon-1 still completed first because it had the shorter delay, but
the program did not continue past Task.WhenAll() until Raven-2 had also
finished.

Compared with Thread + Join, the async version required much less
manual coordination code and was easier to read because the program
focused on the operations being performed rather than directly managing
threads.

### Error propagation with async/await

A simulated motor failure was added to Raven-2 at checkpoint 3.

When Raven-2 threw an InvalidOperationException, its Task became faulted.
Falcon-1 was not automatically cancelled and continued until it completed
its route.

await Task.WhenAll() did not continue normally because one of the Tasks
had failed. After the remaining Task completed, the original
InvalidOperationException was propagated back to the orchestration code.

This differed from using Task.Wait(), where the failure was wrapped in
an AggregateException. With await, the original exception was much easier
to handle directly.

### Comparison with Thread + Join

The async/await version required less manual coordination than the
Thread + Join version.

With Thread, the program had to explicitly create threads, start them,
and call Join() to wait for completion.

With async/await, each drone operation returned a Task, and
Task.WhenAll() was used to coordinate them. This made the orchestration
code shorter and easier to follow.

Error handling was also easier with async/await because exceptions
propagated through the Task and could be handled with a normal try/catch
around await Task.WhenAll().

Overall, the async version focused more on the work being performed and
less on directly managing threads, which should make the code easier to
maintain.



## Part C – Control Tower API

### First asynchronous HTTP request

A local Control Tower API was created using HttpListener. The first
endpoint tested with HttpClient was `/weather`.

The client used GetFromJsonAsync() to asynchronously request the weather
and deserialize the JSON response into a C# object.

The server included a random artificial delay between 200 and 1000 ms.
Timestamps in the client output made this delay visible. While waiting
for the HTTP response, the application used await rather than
synchronously blocking a thread.

A five-second HttpClient timeout was also configured so the client would
not wait indefinitely if the Control Tower failed to respond.

### Sequential vs concurrent HTTP calls

The weather and route requests were first awaited sequentially, meaning
the second request did not begin until the first had completed.

They were then started together and coordinated with Task.WhenAll().
Because the requests were independent, they could overlap while waiting
for the server.

This reduced the total waiting time to approximately the duration of the
slowest request rather than the combined duration of both requests.

### Using Control Tower data in the simulation

The Control Tower responses were connected to the drone simulation rather
than only being displayed.

The `/route` response determines the drone's number of checkpoints, while
the `/weather` response adjusts its delay. Clear weather keeps the original
delay, wind adds 250 ms, and storm adds 750 ms.

During testing, the Control Tower returned "wind" for Falcon-1. Its original
500 ms delay was therefore increased to 750 ms, which could also be observed
in the timestamps between checkpoints.

This demonstrated how asynchronously retrieved HTTP data can affect the
behavior of the application after the request completes.