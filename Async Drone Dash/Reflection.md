# Reflection – Async Drone Dash

## Part A – Thread + Join

### Initial observations

The simulator uses a DroneModel to represent each drone. The model contains the drone's name, maximum number of checkpoints, and delay between checkpoints.

A separate DroneLogger class is used to keep console output consistent and to include timestamps. This will make it easier to observe how multiple drones overlap while running concurrently.

### Experiments

The logger was tested with a one-second Thread.Sleep between messages.
The timestamp showed approximately one second between the messages.

### What happened when Join was removed?

_To be filled in after the experiment._

### Thoughts

_To be filled in as we learn._