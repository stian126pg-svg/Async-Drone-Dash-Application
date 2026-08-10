using AsyncDroneDash.Models;
using AsyncDroneDash.Services;

DroneModel drone = new DroneModel(
    "Falcon-1",
    5,
    500);

ThreadRaceService service = new ThreadRaceService();

service.FlyDrone(drone);