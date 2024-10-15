
How to start the applications:

Run the script from \src\sql in an SSMS, it will create the Simulation database on a SqlServer.
In src\Simulation.Api\appsettings.Development.json, change the database connection according to step 1.
Run in order: SimulationServer.Api, Simulation.Hub.Broadcast, ActorSensor.DataGenerator, ActorSensor.ReceiverDisplay.
