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