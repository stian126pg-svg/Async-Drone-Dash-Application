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

### Thoughts

_To be filled in as we learn._