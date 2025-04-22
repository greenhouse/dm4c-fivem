using CitizenFX.Core.UI;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace Planerain.Client
{
    public class ClientMain : BaseScript
    {
        /* -------------------------------------------------------- */
        /* GLOBALS
        /* -------------------------------------------------------- */
        private bool isRainingPlanes = false;
        private readonly List<Vehicle> activePlanes = new List<Vehicle>();
        private readonly List<int> activePlaneHandles = new List<int>();
        private readonly Random random = new Random();
        private readonly string[] planeModels = { "shamal", "luxor", "velum", "lazer" };
        private const float SPAWN_RADIUS_MIN = 10f;
        private const float SPAWN_RADIUS_MAX = 50f;
        private const float SPAWN_HEIGHT = 100f;
        private const float DESPAWN_DISTANCE = 1500f;
        private const int MAX_PLANES = 128; // i think game core will limit this to 128 planes max

        /* -------------------------------------------------------- */
        /* Constructor
        /* -------------------------------------------------------- */
        public ClientMain()
        {
            Debug.WriteLine("Hi from plainrain.Client!");

            // NOTE: these registers depend on globals above
            RegisterEventHanlders();
            RegisterTickHandlers();
            RegisterCommands();

            SetDayTime();
        }


        /* -------------------------------------------------------- */
        /* PRIVATE - init support
        /* -------------------------------------------------------- */
        private void RegisterEventHanlders() {
            // Listen for server-triggered events
            // EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["plainrain:startRain"] += new Action(OnStartRain);
            EventHandlers["plainrain:stopRain"] += new Action(OnStopRain);
            EventHandlers["playerSpawned"] += new Action(OnPlayerSpawned);
        }
        private void RegisterTickHandlers() {
            // Tick += UpdateHud;
            Tick += SpawnAndManagePlanes;
            Tick += CleanUpPlanes;
        }

        private void RegisterCommands() {

            // Register command handlers
            API.RegisterCommand("/startrain", new Action<int, dynamic>((source, args) =>
            {
                StartRain();
            }), false);
            API.RegisterCommand("/stoprain", new Action<int, dynamic>((source, args) =>
            {
                StopRain();
            }), false);
            API.RegisterCommand("/cleanrain", new Action<int, dynamic>((source, args) =>
            {
                OnCleanUpPlanes();
            }), false);
            
            // get test setup
            API.RegisterCommand("/givetest", new Action<int, dynamic>((source, args) =>
            {
                GiveTestSetup();
            }), false);

            // respwan by sky dive
            API.RegisterCommand("/skydive", new Action<int, dynamic>((source, args) =>
            {
                string is_enable = args.Count > 0 ? args[0].ToString().ToLower() : "false";
                if (is_enable == "enable" || is_enable == "true" || is_enable == "yes" || is_enable == "on" || is_enable == "1")
                {
                    // Disable standard / Enable skydive spawn coords resource (skater = standard, hipster = skydive)
                    TriggerServerEvent("stopResource", "fivem-map-skater"); // Ask server to stop resource
                    TriggerServerEvent("startResource", "fivem-map-hipster"); // Ask server to start resource
                    hlog("YOU enabled skydive respawns!", true, true); // debug, screen
                }
                else
                {
                    // Disable skydive / Enable standard spawn coords resource (skater = standard, hipster = skydive)
                    TriggerServerEvent("stopResource", "fivem-map-hipster"); // Ask server to stop resource
                    TriggerServerEvent("startResource", "fivem-map-skater"); // Ask server to start resource
                    hlog("YOU disabled skydive respawns!", true, true); // debug, screen
                }

                // kill player to force respawn
                SetEntityHealth(PlayerPedId(), 0);
                hlog("YOU were killed to force respawn!", true, true);
            }), false);

            // teleport player to new coords
            API.RegisterCommand("/jump", new Action<int, dynamic>((source, args) =>
            {
                List<float> coords = new List<float>();
                if (args.Count >= 3 && args.Count <= 4) {
                    if (float.TryParse(args[0].ToString(), out float xCoord)) coords.Add(xCoord);
                    else hlog($"/jump failed: Invalid jump coord: {args[0]}.", true, false); // debug, screen

                    if (float.TryParse(args[1].ToString(), out float yCoord)) coords.Add(yCoord);
                    else hlog($"/jump failed: Invalid jump coord: {args[1]}.", true, false); // debug, screen

                    if (float.TryParse(args[2].ToString(), out float zCoord)) coords.Add(zCoord);
                    else hlog($"/jump failed: Invalid jump coord: {args[2]}.", true, false); // debug, screen
                } else {
                    hlog($"/jump failed: Invalid arg count", true, true); // debug, screen
                }      
                
                // check for range arg
                float coordRange = 0;
                if (args.Count == 4) {
                    if (float.TryParse(args[3].ToString(), out float range)) coordRange = range;
                    else hlog($"/jump warn: found Invalid range arg: {args[3]}. defaulting to range {coordRange}", true, false); // debug, screen
                }

                // execute coord jump
                Vector3 coordsV = new Vector3(coords[0], coords[1], coords[2]);
                OnJumpCommand(coordsV, coordRange);
            }), false);

            // manually give car
            API.RegisterCommand("/givecar", new Action<int, dynamic>((source, args) =>
            {
                uint vehicleHash = (uint)API.GetHashKey("POLICE");
                GiveVehicle(vehicleHash);
                hlog($"YOU manually requested a car type: {vehicleHash}", true, true); // debug, screen
            }), false);

            // manually give bike
            API.RegisterCommand("/givebike", new Action<int, dynamic>((source, args) =>
            {
                // nightblade
                uint vehicleHash = (uint)API.GetHashKey("NIGHTBLADE");
                GiveVehicle(vehicleHash);
                hlog($"YOU manually requested a bike type: {vehicleHash}", true, true); // debug, screen
            }), false);

            // Register the /quit command
            API.RegisterCommand("/quit", new Action<int, dynamic>((source, args) =>
            {
                QuitServer(); // Trigger server event to disconnect
            }), false);

            // get curreny coords
            API.RegisterCommand("/coords", new Action<int, dynamic>((source, args) =>
            {
                Vector3 coords = API.GetEntityCoords(API.PlayerPedId(), false);
                hlog($"YOU are at coords: {coords}", true, true); // debug, screen
            }), false);
        }


        /* -------------------------------------------------------- */
        /* PRIVATE - event hanlders                            
        /* -------------------------------------------------------- */
        private void OnStartRain()
        {
            StartRain();
        }

        private void OnStopRain()
        {
            StopRain();
        }

        private void OnPlayerSpawned()
        {   
            hlog("YOU respawned", true, true); // debug, screen
            // GiveTestSetup(); // ** WARNING ** - testing only (comment out for production)
        }
        private void OnJumpCommand(Vector3 coords, float range=0)
        {
            // Teleport player to new coordinates
            float X = coords.X+range;
            float Y = coords.Y+range;
            float Z = coords.Z+range;
            API.SetEntityCoords(API.PlayerPedId(), X, Y, Z, false, false, false, false);
            hlog($"YOU jumped to coords: {X}, {Y}, {Z}", true, true); // debug, screen
        }
        private void OnClientResourceStart(string resourceName)
        {
            // Load NUI HUD
            // API.SetNuiFocus(false, false);
            // API.SendNuiMessage(@"{""type"": ""showHud"", ""visible"": true}");
        }


        /* -------------------------------------------------------- */
        /* PRIVATE - task                            
        /* -------------------------------------------------------- */
        [Tick]
        public Task OnTick()
        {
            // DrawRect(0.5f, 0.5f, 0.5f, 0.5f, 255, 255, 255, 150);

            return Task.FromResult(0);
        }
        private void OnCleanUpPlanes()
        {
            // Clean up planes that are too far away or have been destroyed
            for (int i = activePlanes.Count - 1; i >= 0; i--)
            {
                Vehicle plane = activePlanes[i];

                int handle = plane.Handle;
                API.DeleteVehicle(ref handle);
                activePlanes.RemoveAt(i);
            }
        }
        private async Task CleanUpPlanes()
        {
            // Vector3 playerPos = Game.PlayerPed.Position;

            // for (int i = activePlanes.Count - 1; i >= 0; i--)
            // {
            //     Vehicle plane = activePlanes[i];

            //     if (!DoesEntityExist(plane.Handle))
            //     {
            //         activePlanes.RemoveAt(i);
            //         continue;
            //     }

            //     float distance = Vector3.Distance(playerPos, plane.Position);
            //     // if (distance > DESPAWN_DISTANCE || plane.Position.Z < playerPos.Z + 10f)
            //     if (distance > DESPAWN_DISTANCE)
            //     {
            //         int handle = plane.Handle;
            //         API.DeleteVehicle(ref handle);
            //         activePlanes.RemoveAt(i);
            //         continue;
            //     }

            //     // Ensure plane continues falling straight down
            //     SetVehicleForwardSpeed(plane.Handle, 0f); // No horizontal movement
            //     SetEntityRotation(plane.Handle, 90f, 0f, 0f, 2, true); // Maintain 90-degree pitch
            // }

            await Task.FromResult(0);
        }
        private async Task SpawnAndManagePlanes()
        {
            // if (!isRainingPlanes)
            //     return;

            if (activePlanes.Count < MAX_PLANES)
            {
                hlog($"YOU are spawning planes: {activePlanes.Count}", true, false); // debug, screen
                Vector3 playerPos = Game.PlayerPed.Position;

                // Generate random radius within min and max
                // float radius = SPAWN_RADIUS_MIN + (float)random.NextDouble() * (SPAWN_RADIUS_MAX - SPAWN_RADIUS_MIN);

                float spawn_min_rad = 20f;
                float spawn_max_rad = 100f;
                float spawn_height = 100f;
                float radius = spawn_min_rad + (float)random.NextDouble() * (spawn_max_rad - spawn_min_rad);
                float angle = (float)(random.NextDouble() * 2 * Math.PI);
                float x = playerPos.X + radius * (float)Math.Cos(angle);
                float y = playerPos.Y + radius * (float)Math.Sin(angle);
                // float z = playerPos.Z + SPAWN_HEIGHT;
                float z = playerPos.Z + spawn_height;

                string planeModel = planeModels[random.Next(planeModels.Length)];
                uint modelHash = (uint)GetHashKey(planeModel);

                RequestModel(modelHash);
                int attempts = 0;
                while (!HasModelLoaded(modelHash) && attempts < 100)
                {
                    await Delay(10);
                    attempts++;
                }

                if (HasModelLoaded(modelHash))
                {
                    // Spawn plane with arbitrary heading (falls straight down)
                    int vehicleHandle = CreateVehicle(modelHash, x, y, z, 0f, true, false);
                    if (DoesEntityExist(vehicleHandle))
                    {
                        Vehicle plane = new Vehicle(vehicleHandle);
                        activePlanes.Add(plane);

                        // Set plane properties
                        SetVehicleEngineOn(plane.Handle, false, true, true); // Engine off for free fall
                        SetVehicleForwardSpeed(plane.Handle, 0f); // No initial speed

                        // increase downward velocity (increase gravitational force)
                        float velocity_down = -20f;
                        API.SetEntityVelocity(plane.Handle, 0f, 0f, velocity_down); 

                        // Set 90-degree downward pitch for vertical fall
                        SetEntityRotation(plane.Handle, -90f, 0f, 0f, 2, true);

                        // Create a pilot but freeze in place
                        int pilot = CreatePedInsideVehicle(plane.Handle, 26, (uint)GetHashKey("s_m_y_pilot_01"), -1, true, false);
                        SetBlockingOfNonTemporaryEvents(pilot, true);
                        SetPedConfigFlag(pilot, 184, true); // Prevent pilot control
                        FreezeEntityPosition(pilot, true); // Freeze pilot to avoid interference
                    }

                    // SetModelAsNoLongerNeeded(modelHash);
                }
            }

            await Delay(500);
        }
        // private async Task SpawnAndManagePlanes3()
        // {
        //     // if (!isRainingPlanes)
        //     //     return;

        //     if (activePlanes.Count < MAX_PLANES)
        //     {
        //         Vector3 playerPos = Game.PlayerPed.Position;

        //         // Spawn directly overhead
        //         float x = playerPos.X;
        //         float y = playerPos.Y;
        //         // float z = playerPos.Z + SPAWN_HEIGHT;
        //         float z = playerPos.Z + 100f;

        //         string planeModel = planeModels[random.Next(planeModels.Length)];
        //         uint modelHash = (uint)GetHashKey(planeModel);

        //         RequestModel(modelHash);
        //         int attempts = 0;
        //         while (!HasModelLoaded(modelHash) && attempts < 100)
        //         {
        //             await Delay(10);
        //             attempts++;
        //         }

        //         if (HasModelLoaded(modelHash))
        //         {
        //             // Get ped's velocity to determine running direction
        //             Vector3 velocity = GetEntityVelocity(PlayerPedId());
        //             float heading = 0f;
        //             if (velocity.X != 0f || velocity.Y != 0f) // Only calculate if moving
        //             {
        //                 // heading = (float)(Math.Atan2(velocity.Y, velocity.X) * 180 / Math.PI);
        //                 heading = (float)(Math.Atan2(velocity.Y, velocity.X) * 90 / Math.PI);
        //             }

        //             // Spawn plane with arbitrary heading (since diving straight down)
        //             // int vehicleHandle = CreateVehicle(modelHash, x, y, z, 0f, true, false);
        //             int vehicleHandle = CreateVehicle(modelHash, x, y, z, heading, true, false);
        //             if (DoesEntityExist(vehicleHandle))
        //             {
        //                 Vehicle plane = new Vehicle(vehicleHandle);
        //                 activePlanes.Add(plane);

        //                 // Set plane properties
        //                 SetVehicleEngineOn(plane.Handle, false, true, false);
        //                 // SetVehicleForwardSpeed(plane.Handle, 60f);
        //                 SetVehicleForwardSpeed(plane.Handle, 20f);

        //                 // Set 90-degree downward pitch
        //                 // SetEntityRotation(plane.Handle, 90f, 0f, 0f, 2, true);
        //                 SetEntityRotation(plane.Handle, 180f, 0f, 90f, 2, true);

        //                 // Create a pilot but disable control
        //                 int pilot = CreatePedInsideVehicle(plane.Handle, 26, (uint)GetHashKey("s_m_y_pilot_01"), -1, true, false);
        //                 SetBlockingOfNonTemporaryEvents(pilot, true);
        //                 SetPedConfigFlag(pilot, 184, true);
        //             }

        //             // SetModelAsNoLongerNeeded(modelHash);
        //         }
        //     }

        //     await Delay(1000);
        // }


        // private async Task SpawnAndManagePlane2()
        // {
        //     // if (!isRainingPlanes)
        //     //     return;

        //     if (activePlanes.Count < MAX_PLANES)
        //     {
        //         Vector3 playerPos = Game.PlayerPed.Position;

        //         // Get player's heading and calculate spawn position behind them
        //         float playerHeading = GetEntityHeading(PlayerPedId()); // Player's facing direction in degrees
        //         float spawnAngle = (playerHeading + 180f) * (float)(Math.PI / 180); // Opposite direction in radians
        //         float radius = 200f;
        //         float x = playerPos.X + radius * (float)Math.Cos(spawnAngle); // Spawn behind player
        //         float y = playerPos.Y + radius * (float)Math.Sin(spawnAngle);
        //         // float z = playerPos.Z + SPAWN_HEIGHT;
        //         float z = playerPos.Z + 200f;

        //         string planeModel = planeModels[random.Next(planeModels.Length)];
        //         uint modelHash = (uint)GetHashKey(planeModel);

        //         RequestModel(modelHash);
        //         int attempts = 0;
        //         while (!HasModelLoaded(modelHash) && attempts < 100)
        //         {
        //             await Delay(10);
        //             attempts++;
        //         }

        //         if (HasModelLoaded(modelHash))
        //         {
        //             // Calculate heading toward player
        //             Vector3 directionToPlayer = playerPos - new Vector3(x, y, z);
        //             // float heading = (float)(Math.Atan2(directionToPlayer.Y, directionToPlayer.X) * 180 / Math.PI);
        //             float heading = (float)(Math.Atan2(directionToPlayer.Y, directionToPlayer.X) * -195 / Math.PI);

        //             // Spawn plane
        //             int vehicleHandle = CreateVehicle(modelHash, x, y, z, heading, true, false);
        //             if (DoesEntityExist(vehicleHandle))
        //             {
        //                 Vehicle plane = new Vehicle(vehicleHandle);
        //                 activePlanes.Add(plane);

        //                 // Set plane properties
        //                 SetVehicleEngineOn(plane.Handle, false, true, false);
        //                 SetVehicleForwardSpeed(plane.Handle, 60f);

        //                 // Set initial downward pitch toward player
        //                 // SetEntityRotation(plane.Handle, 45f, 0f, heading, 2, true);
        //                 SetEntityRotation(plane.Handle, 22.5f, 0f, heading, 2, true);

        //                 // Create a pilot but disable control
        //                 int pilot = CreatePedInsideVehicle(plane.Handle, 26, (uint)GetHashKey("s_m_y_pilot_01"), -1, true, false);
        //                 SetBlockingOfNonTemporaryEvents(pilot, true);
        //                 SetPedConfigFlag(pilot, 184, true);
        //             }

        //             // SetModelAsNoLongerNeeded(modelHash);
        //         }
        //     }

        //     await Delay(500);
        // }
        // private async Task SpawnAndManagePlanes1()
        // {
        //     // if (!isRainingPlanes)
        //     //     return;

        //     if (activePlanes.Count < MAX_PLANES)
        //     {
        //         Vector3 playerPos = Game.PlayerPed.Position;

        //         // Spawn within a fixed radius (e.g., 800 units) around player
        //         // float radius = 200f;
        //         // float angle = (float)(random.NextDouble() * 2 * Math.PI);
        //         // // float x = playerPos.X + radius * (float)Math.Cos(angle);
        //         // // float y = playerPos.Y + radius * (float)Math.Sin(angle);
        //         // float x = playerPos.X - radius;
        //         // float y = playerPos.Y - radius;
        //         // // float z = playerPos.Z + SPAWN_HEIGHT;
        //         // float z = playerPos.Z + 50f;

        //         // Get player's heading and calculate spawn position behind them
        //         float playerHeading = GetEntityHeading(PlayerPedId()); // Player's facing direction in degrees
        //         float spawnAngle = (playerHeading + 180f) * (float)(Math.PI / 180); // Opposite direction in radians
        //         float radius = 800f;
        //         float x = playerPos.X + radius * (float)Math.Cos(spawnAngle); // Spawn behind player
        //         float y = playerPos.Y + radius * (float)Math.Sin(spawnAngle);
        //         float z = playerPos.Z + SPAWN_HEIGHT;

        //         string planeModel = planeModels[random.Next(planeModels.Length)];
        //         uint modelHash = (uint)GetHashKey(planeModel);

        //         RequestModel(modelHash);
        //         int attempts = 0;
        //         while (!HasModelLoaded(modelHash) && attempts < 100)
        //         {
        //             await Delay(10);
        //             attempts++;
        //         }

        //         if (HasModelLoaded(modelHash))
        //         {
        //             // Calculate heading toward player
        //             Vector3 directionToPlayer = playerPos - new Vector3(x, y, z);
        //             // float heading = (float)(Math.Atan2(directionToPlayer.Y, directionToPlayer.X) * 180 / Math.PI);
        //             // float heading = (float)(Math.Atan2(directionToPlayer.Y, directionToPlayer.X));

        //             int playerPed = API.PlayerPedId();
        //             float heading = API.GetEntityHeading(playerPed);
        //             // Spawn plane
        //             int vehicleHandle = CreateVehicle(modelHash, x, y, z, heading, true, false);
        //             if (DoesEntityExist(vehicleHandle))
        //             {
        //                 Vehicle plane = new Vehicle(vehicleHandle);
        //                 activePlanes.Add(plane);

        //                 // Set plane properties
        //                 SetVehicleEngineOn(plane.Handle, false, true, false);
        //                 SetVehicleForwardSpeed(plane.Handle, 1000f);

        //                 // Set initial downward pitch toward player
        //                 SetEntityRotation(plane.Handle, 45f, 0f, heading, 2, true);

        //                 // Create a pilot but disable control
        //                 int pilot = CreatePedInsideVehicle(plane.Handle, 26, (uint)GetHashKey("s_m_y_pilot_01"), -1, true, false);
        //                 SetBlockingOfNonTemporaryEvents(pilot, true);
        //                 SetPedConfigFlag(pilot, 184, true);
        //             }

        //             // SetModelAsNoLongerNeeded(modelHash);
        //         }
        //     }

        //     await Delay(500);
        // }
        // private async Task SpawnAndManagePlanes0()
        // {
        //     // Only spawn if under max plane limit
        //     if (activePlanes.Count < MAX_PLANES)
        //     {
        //         hlog($"YOU are spawning planes: {activePlanes.Count}", true, false); // debug, screen
        //         // Get player position
        //         Vector3 playerPos = Game.PlayerPed.Position;

        //         // Generate random spawn position
        //         float angle = (float)(random.NextDouble() * 2 * Math.PI);
        //         float radius = SPAWN_RADIUS_MIN + (float)random.NextDouble() * (SPAWN_RADIUS_MAX - SPAWN_RADIUS_MIN);
        //         float x = playerPos.X + radius * (float)Math.Cos(angle);
        //         float y = playerPos.Y + radius * (float)Math.Sin(angle);
        //         float z = playerPos.Z + SPAWN_HEIGHT;

        //         // Calculate heading toward player for initial orientation
        //         Vector3 directionToPlayer = playerPos - new Vector3(x, y, z);
        //         float heading = (float)(Math.Atan2(directionToPlayer.Y, directionToPlayer.X) * 180 / Math.PI);

        //         // float x = playerPos.X;
        //         // float y = playerPos.Y;
        //         // float z = playerPos.Z + 100;
        //         Vector3 spawnPos = new Vector3(x, y, z);
        //         hlog($"YOU are spawning planes at random coords: {spawnPos}", true, false); // debug, screen

        //         // Select random plane model
        //         string planeModel = planeModels[random.Next(planeModels.Length)];
        //         uint modelHash = (uint)GetHashKey(planeModel);

        //         // Request model
        //         RequestModel(modelHash);
        //         int attempts = 0;
        //         while (!HasModelLoaded(modelHash) && attempts < 100)
        //         {
        //             await Delay(10);
        //             attempts++;
        //             hlog($"YOU are spawning planes attempt: {attempts}", true, false); // debug, screen
        //         }

        //         if (HasModelLoaded(modelHash))
        //         {
        //             hlog("YOU are spawning planes: model loaded", true, false); // debug, screen
        //             // Spawn plane
        //             int vehicleHandle = CreateVehicle(modelHash, x, y, z, 0f, true, false);
        //             if (DoesEntityExist(vehicleHandle))
        //             {
        //                 hlog("YOU are spawning planes: DoesEntityExist", true, false); // debug, screen
        //                 // Vehicle plane = new Vehicle(vehicleHandle);
        //                 // activePlanes.Add(plane);

        //                 // // Set plane properties
        //                 // SetVehicleEngineOn(plane.Handle, true, true, false);
        //                 // SetHeliBladesFullSpeed(plane.Handle); // For planes, ensures they move
        //                 // SetVehicleForwardSpeed(plane.Handle, 50f); // Reasonable speed

        //                 // // Create a pilot
        //                 // int pilot = CreatePedInsideVehicle(plane.Handle, 26, (uint)GetHashKey("s_m_y_pilot_01"), -1, true, false);
        //                 // SetBlockingOfNonTemporaryEvents(pilot, true);


        //                 // ... crashing
        //                 Vehicle plane = new Vehicle(vehicleHandle);
        //                 activePlanes.Add(plane);

        //                 SetVehicleEngineOn(plane.Handle, false, true, false);
        //                 SetVehicleForwardSpeed(plane.Handle, 100f);
        //                 SetEntityRotation(plane.Handle, 45f, 0f, heading, 2, true);

        //                 // Create a pilot but disable control to ensure crash
        //                 int pilot = CreatePedInsideVehicle(plane.Handle, 26, (uint)GetHashKey("s_m_y_pilot_01"), -1, true, false);
        //                 SetBlockingOfNonTemporaryEvents(pilot, true);
        //                 SetPedConfigFlag(pilot, 184, true); // Prevent pilot from recovering control
        //             }

        //             // SetModelAsNoLongerNeeded(modelHash);
        //         }
        //     }

        //     await Delay(500);
        //     // await Task.FromResult(1000);
        // }
        // private async Task CleanUpPlanes()
        // {
        //     Vector3 playerPos = Game.PlayerPed.Position;

        //     for (int i = activePlanes.Count - 1; i >= 0; i--)
        //     {
        //         Vehicle plane = activePlanes[i];

        //         if (!DoesEntityExist(plane.Handle))
        //         {
        //             activePlanes.RemoveAt(i);
        //             continue;
        //         }

        //         // Check distance from player
        //         float distance = Vector3.Distance(playerPos, plane.Position);
        //         if (distance > DESPAWN_DISTANCE)
        //         {
        //             DeleteVehicle(ref plane.Handle);
        //             activePlanes.RemoveAt(i);
        //             continue;
        //         }

        //         // Update plane to track player
        //         Vector3 direction = playerPos - plane.Position;
        //         float heading = (float)(Math.Atan2(direction.Y, direction.X) * 180 / Math.PI);
        //         SetVehicleForwardSpeed(plane.Handle, 50f);
        //         TaskVehicleDriveToCoord(GetPedInVehicleSeat(plane.Handle, -1), plane.Handle, playerPos.X, playerPos.Y, playerPos.Z, 50f, 1f, GetEntityModel(plane.Handle), 787263, 10f, 10f);
        //     }

        //     await Task.FromResult(0);
        // }

        /* -------------------------------------------------------- */
        /* PRIVATE - algorthimic support                            
        /* -------------------------------------------------------- */
        private void StartRain()
        {
            if (!isRainingPlanes)
            {
                isRainingPlanes = true;
                Debug.WriteLine("Started raining planes!");
            }

            TriggerServerEvent("startrain");
        }

        private void StopRain()
        {
            if (isRainingPlanes)
            {
                isRainingPlanes = false;
                // Despawn all active planes
                foreach (var plane in activePlanes)
                {
                    if (DoesEntityExist(plane.Handle))
                    {
                        // DeleteVehicle(ref plane.Handle);
                    }
                }
                activePlanes.Clear();
                Debug.WriteLine("Stopped raining planes!");
            }

            TriggerServerEvent("stoprain");
        }
        private void SetDayTime()
        {
            NetworkOverrideClockTime(12, 0, 0); // Set time to 12:00:00 (noon)
            SetWeatherTypeNow("CLEAR"); // Set weather to clear
            hlog("Time set to daytime (12:00)", true, true);
        }
        private void GiveTestSetup(){
            hlog($"YOU initialized test setup", true, true); // debug, screen
        }
        private async void GiveVehicle(uint vehicleHash) {
            // Spawn the waverunner near the player
            int playerPed = API.PlayerPedId();
            Vector3 playerPos = API.GetEntityCoords(playerPed, true);

            // string model = "PCJ"; // Try "BMX", "SEASHARK", etc.

            // uint modelHash = (uint)GetHashKey(vehicleHash);
            RequestModel(vehicleHash);
            int timeout = 0;
            while (!HasModelLoaded(vehicleHash) && timeout < 50) // 5-second timeout
            {
                await Delay(100);
                timeout++;
            }
            if (!HasModelLoaded(vehicleHash))
            {
                Screen.ShowNotification($"~r~Failed to load {vehicleHash}");
                return;
            }
            int vehicle = API.CreateVehicle(vehicleHash, playerPos.X, playerPos.Y, playerPos.Z + 2.0f, API.GetEntityHeading(playerPed), true, false);

            // Place the player in the vehicle
            API.SetPedIntoVehicle(playerPed, vehicle, -1); // -1 is the driver seat

            // Enable weapon usage in the vehicle -> NOTE_041625: not working, can't seem to ride with weapon & shoot
            API.SetPedConfigFlag(playerPed, 184, true); // Allow weapons in vehicle
            API.SetPedCanSwitchWeapon(playerPed, true); // Allow weapon switching
            API.SetPlayerCanDoDriveBy(API.PlayerId(), true); // Enable drive-by shooting
            API.SetCurrentPedWeapon(playerPed, (uint)API.GetHashKey("WEAPON_PISTOL"), true); // Equip a default weapon
        }
        private void QuitServer() {
            // Notify the player
            hlog("YOU are Disconnecting from the server...", true, true); // debug, screen

            // Trigger server event to disconnect
            TriggerServerEvent("playerQuit");
        }
        private void hlog(string message, bool debug, bool screen)
        {
            if (debug) Debug.WriteLine(message);
            if (screen) Screen.ShowNotification(message);
        }

        // /* -------------------------------------------------------- */
        // /* HUD - HTML - support
        // /* -------------------------------------------------------- */
        // // Register test command
        // API.RegisterCommand("/togglehud", new Action<int, dynamic>((source, args) =>
        // {
        //     HUD_VISIBLE = !HUD_VISIBLE; // Toggle state
        //     API.SendNuiMessage($@"{{""type"": ""showHud"", ""visible"": {HUD_VISIBLE.ToString().ToLower()}}}");
        //     hlog($"HUD {(HUD_VISIBLE ? "enabled" : "disabled")}", false, true); // debug, screen
        // }), false);
        // private async Task UpdateHud()
        // {
        //     // get current player w/ weaponHash selected
        //     int playerPed = API.PlayerPedId();
        //     int weaponHashSel = API.GetSelectedPedWeapon(playerPed);
            
        //     if (API.IsPedShooting(playerPed)) {
        //         hlog($"API.IsPedShooting invoked w/ LIVE_AMMO: {LIVE_AMMO}, LAST_WEAPON_AMMO_CNT: {LAST_WEAPON_AMMO_CNT}", true, false); // debug, screen

        //         // set 0 ammo if negative
        //         if (LIVE_AMMO <= 0) {
        //             hlog($"No Loaded ammo. Use|Fill Reserve!", true, true); // bool: debug, screen
        //             LIVE_AMMO = 0;
        //             LAST_WEAPON_AMMO_CNT = 0;

        //             // SetPedAmmo w/ LIVE_AMMO & WEAPONHASH_BULLET_VALUE calc
        //             SetPedAmmoWithBulletValue();
        //         } else {
        //             // calc weaponHash ammo discharged during this task (and update global for next task)
        //             int weaponAmmoCurr = API.GetAmmoInPedWeapon(playerPed, (uint)weaponHashSel);
        //             int weapAmmoDischarge = LAST_WEAPON_AMMO_CNT - weaponAmmoCurr; // NOTE: calc total discharge amnt incase frames/tasks are missed
        //             hlog($"weaponAmmoCurr: {weaponAmmoCurr}, weapAmmoDischarge: {weapAmmoDischarge}", false, false); // debug, screen
                    
        //             // calc loaded ammo for HUD update
        //             int bulletVal = WEAPONHASH_BULLET_VALUE[weaponHashSel]; // get $BULLET token value per weapon type
        //             // LIVE_AMMO = LIVE_AMMO - bulletVal; // calc new total LIVE_AMMO
        //             LIVE_AMMO = LIVE_AMMO - (bulletVal * weapAmmoDischarge); // calc new total LIVE_AMMO
        //             LAST_WEAPON_AMMO_CNT = weaponAmmoCurr; // save and update last ammo count for next task calc
        //         }

        //         // update HUD
        //         UpdateNui(LIVE_AMMO); 
        //     }
        //     await Task.FromResult(0);
        // }
        // private void UpdateNui()
        // {            
        //     API.SendNuiMessage($@"{{
        //         ""type"": ""updateAmmo"",
        //         ""live"": {-1},
        //         ""reserve"": {-1}
        //     }}");
        // }
    }
}