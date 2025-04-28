using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System.Collections.Generic; // required for List<T> support
using static CitizenFX.Core.Native.API;

namespace house.Client
{
    public class ClientMain : BaseScript
    {
        /* -------------------------------------------------------- */
        /* Constructor
        /* -------------------------------------------------------- */
        public ClientMain()
        {
            Debug.WriteLine("Hi from house.Client!");

            SetDayTime();
            RegisterEventHanlders();
            RegisterTickHandlers();
            RegisterDefaultCommands();       
        }

        /* -------------------------------------------------------- */
        /* PRIVATE - init support
        /* -------------------------------------------------------- */
        private void hlog(string message, bool debug, bool screen)
        {
            if (debug) Debug.WriteLine(message);
            if (screen) Screen.ShowNotification(message);
        }
        private void SetDayTime()
        {
            NetworkOverrideClockTime(12, 0, 0); // Set time to 12:00:00 (noon)
            SetWeatherTypeNow("CLEAR"); // Set weather to clear
            hlog("Time set to daytime (12:00)", true, true);
        }
        private void RegisterEventHanlders() {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["playerSpawned"] += new Action(OnPlayerSpawned);
            EventHandlers["gameEventTriggered"] += new Action<string, int[]>(OnGameEventTriggered); // Add this
        }
        private void RegisterTickHandlers() {
            // Tick += UpdateHud;
            // Tick += SpawnAndManagePlanes;
            // Tick += CleanUpPlanes;
        }
        private void RegisterDefaultCommands()
        {
            // get test setup
            // API.RegisterCommand("/givetest", new Action<int, dynamic>((source, args) =>
            // {
            //     // GiveTestSetup();
            // }), false);

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
                CoordJump(coordsV, coordRange);
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

            // manually give boat
            API.RegisterCommand("/giveboat", new Action<int, dynamic>((source, args) =>
            {
                uint vehicleHash = (uint)API.GetHashKey("SEASHARK");
                if (args.Count > 0)
                {
                    string type = args[0].ToString();
                    vehicleHash = type == "2" ? (uint)API.GetHashKey("SEASHARK2") : (uint)API.GetHashKey("SEASHARK");
                    vehicleHash = type == "3" ? (uint)API.GetHashKey("SEASHARK3") : (uint)API.GetHashKey("SEASHARK");
                    vehicleHash = type == "4" ? (uint)API.GetHashKey("SPEEDER") : (uint)API.GetHashKey("SEASHARK");
                }
                GiveVehicle(vehicleHash);
                hlog($"YOU manually requested a boat type: {vehicleHash}", true, true); // debug, screen
            }), false);

            // manually sync pickups from server side
            API.RegisterCommand("/stopresource", new Action<int, dynamic>((source, args) =>
            {
                if (args.Count > 0)
                {
                    string resourceName = args[0].ToString();
                    TriggerServerEvent("stopResource", resourceName); // Ask server to stop resource
                    hlog($"YOU manually requested resource stop: {resourceName}", true, true); // debug, screen
                }
                else
                {
                    hlog("YOU manually requested resource stop: no args", true, false); // debug, screen
                }
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
                TriggerServerEvent("playerCoords", coords); // Send coords to server    
            }), false);
        }

        /* -------------------------------------------------------- */
        /* PRIVATE - task                            
        /* -------------------------------------------------------- */
        // [Tick]
        // public Task OnTick()
        // {
        //     // DrawRect(0.5f, 0.5f, 0.5f, 0.5f, 255, 255, 255, 150);
        //     return Task.FromResult(0);
        // }

        /* -------------------------------------------------------- */
        /* PRIVATE - event hanlders                            
        /* -------------------------------------------------------- */
        private void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            // Start HUD
            API.SendNuiMessage(@"{""type"":""init"",""killCount"":0,""level"":1}");
        }
        private void OnPlayerSpawned()
        {   
            hlog("YOU respawned", true, true); // debug, screen
            // requestPickupSync(); // need to get BULLET_PICKUPS from server (for "draw" and blips)
            // giveDefaultWeapons();
            // GiveTestSetup(); // ** WARNING ** - testing only (comment out for production)
        }
        // Handle game events (ie. to detect kills)
        //  note: The CEventNetworkEntityDamage approach assumes the police NPCs are spawned as PedHash.Cop01SMY. 
        //      If you change the ped model, update any model-specific checks.
        private void OnGameEventTriggered(string eventName, int[] args)
        {
            // if (eventName != "CEventNetworkEntityDamage") return;

            // int victim = args[0]; // Entity that was damaged
            // int attacker = args[1]; // Entity that caused the damage
            // bool isDead = args[4] == 1; // Whether the victim is dead

            // // Check if the attacker is the player and the victim is a ped that died
            // if (attacker == Game.PlayerPed.Handle && isDead && API.IsEntityAPed(victim))
            // {
            //     OnPlayerKilled(victim, "killed"); // Call the kill handler
            // }
        }

        /* -------------------------------------------------------- */
        /* PRIVATE - support functions
        /* -------------------------------------------------------- */
        private void CoordJump(Vector3 coords, float range=0)
        {
            // Teleport player to new coordinates
            float X = coords.X+range;
            float Y = coords.Y+range;
            float Z = coords.Z+range;
            API.SetEntityCoords(API.PlayerPedId(), X, Y, Z, false, false, false, false);
            hlog($"YOU jumped to coords: {X}, {Y}, {Z}", true, true); // debug, screen
        }
        private void QuitServer() {
            // Notify the player
            hlog("YOU are Disconnecting from the server...", true, true); // debug, screen

            // Trigger server event to disconnect
            TriggerServerEvent("playerQuit");
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
    }
}