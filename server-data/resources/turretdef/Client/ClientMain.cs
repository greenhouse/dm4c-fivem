using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;
using System.Collections.Generic; // required for List<T> support

namespace turretdef.Client
{
    public class ClientMain : BaseScript
    {
        private static readonly Vector3 PoliceSpawnPoint = new Vector3(100.0f, 100.0f, 30.0f); // Example GPS location
        private int currentLevel = 1;
        private float spawnInterval = 5.0f; // Seconds
        private int killCount = 0;
        private bool isSpawning = false;

        /* -------------------------------------------------------- */
        /* Constructor
        /* -------------------------------------------------------- */
        public ClientMain()
        {
            Debug.WriteLine("Hi from turretdef.Client!");

            SetDayTime();
            RegisterEventHanlders();
            RegisterTickHandlers();
            RegisterDefaultCommands();
            RegisterCommands();            
        }

        /* -------------------------------------------------------- */
        /* PRIVATE - init support
        /* -------------------------------------------------------- */
        private void SetDayTime()
        {
            NetworkOverrideClockTime(12, 0, 0); // Set time to 12:00:00 (noon)
            SetWeatherTypeNow("CLEAR"); // Set weather to clear
            hlog("Time set to daytime (12:00)", true, true);
        }
        private void RegisterEventHanlders() {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["playerKilledByPlayer"] += new Action<int, string>(OnPlayerKilled);
            EventHandlers["setLevel"] += new Action<int, float>(SetLevel);
            EventHandlers["playerSpawned"] += new Action(OnPlayerSpawned);
            
            EventHandlers["gameEventTriggered"] += new Action<string, int[]>(OnGameEventTriggered); // Add this
        }
        private void RegisterTickHandlers() {
            // Tick += UpdateHud;
            // Tick += SpawnAndManagePlanes;
            // Tick += CleanUpPlanes;
        }
        private void RegisterCommands()
        {
            for (int i = 1; i <= 5; i++)
            {
                int level = i;
                float interval = i * 5.0f;                
                API.RegisterCommand($"level{level}", new Action<int, dynamic>((source, args) =>
                {
                    SetLevel(level, interval);
                    hlog($"Set defense level {level}", true, true); // debug, screen
                }), false);
            }
        }
        private void RegisterDefaultCommands()
        {
            // get test setup
            API.RegisterCommand("/givetest", new Action<int, dynamic>((source, args) =>
            {
                // GiveTestSetup();
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
        /* PRIVATE - task                            
        /* -------------------------------------------------------- */
        private async Task SpawnPolice()
        {
            var model = new Model(PedHash.Cop01SMY);
            await model.Request(1000);
            if (!model.IsLoaded) return;

            var ped = await World.CreatePed(model, PoliceSpawnPoint);
            ped.Weapons.Give(WeaponHash.Pistol, 9999, true, true);
            API.SetPedInfiniteAmmo(ped.Handle, true, (uint)WeaponHash.Pistol);
            API.TaskCombatPed(ped.Handle, Game.PlayerPed.Handle, 0, 16);
            model.MarkAsNoLongerNeeded();
        }

        /* -------------------------------------------------------- */
        /* PRIVATE - event hanlders                            
        /* -------------------------------------------------------- */
        private void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            // Initialize player
            InitializePlayer();

            // Start HUD
            API.SendNuiMessage(@"{""type"":""init"",""killCount"":0,""level"":1}");

            // Start police spawning
            StartPoliceSpawning();
        }

        // Handle game events to detect kills
        //  note: The CEventNetworkEntityDamage approach assumes the police NPCs are spawned as PedHash.Cop01SMY. 
        //      If you change the ped model, update any model-specific checks.
        private void OnGameEventTriggered(string eventName, int[] args)
        {
            if (eventName != "CEventNetworkEntityDamage") return;

            int victim = args[0]; // Entity that was damaged
            int attacker = args[1]; // Entity that caused the damage
            bool isDead = args[4] == 1; // Whether the victim is dead

            // Check if the attacker is the player and the victim is a ped that died
            if (attacker == Game.PlayerPed.Handle && isDead && API.IsEntityAPed(victim))
            {
                OnPlayerKilled(victim, "killed"); // Call the kill handler
            }
        }

        // note: The deathData parameter in OnPlayerKilled is unused in the current logic but kept for compatibility with the original code.
        private void OnPlayerKilled(int victim, string deathData)
        {
            killCount++;
            UpdateWeaponBasedOnKills();
            API.SendNuiMessage($@"{{""type"":""update"",""killCount"":{killCount},""level"":{currentLevel}}}");
        }
        private void OnPlayerSpawned()
        {   
            hlog("YOU respawned", true, true); // debug, screen
            // requestPickupSync(); // need to get BULLET_PICKUPS from server (for "draw" and blips)
            // giveDefaultWeapons();
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

        /* -------------------------------------------------------- */
        /* PRIVATE - support functions
        /* -------------------------------------------------------- */
        private void UpdateWeaponBasedOnKills()
        {
            var playerPed = Game.PlayerPed;
            if (killCount >= 10 && !playerPed.Weapons.HasWeapon(WeaponHash.AssaultRifle))
            {
                playerPed.Weapons.Give(WeaponHash.AssaultRifle, 9999, true, true);
                API.SetPedInfiniteAmmo(API.GetPlayerPed(-1), true, (uint)WeaponHash.AssaultRifle);
                TriggerEvent("chat:addMessage", new { color = new[] { 255, 255, 0 }, args = new[] { "[Weapon]", "Assault Rifle unlocked!" } });
            }
            else if (killCount >= 20 && !playerPed.Weapons.HasWeapon(WeaponHash.SniperRifle))
            {
                playerPed.Weapons.Give(WeaponHash.SniperRifle, 9999, true, true);
                API.SetPedInfiniteAmmo(API.GetPlayerPed(-1), true, (uint)WeaponHash.SniperRifle);
                TriggerEvent("chat:addMessage", new { color = new[] { 255, 255, 0 }, args = new[] { "[Weapon]", "Sniper Rifle unlocked!" } });
            }
        }

        private void SetLevel(int level, float interval)
        {
            currentLevel = level;
            spawnInterval = interval;
            API.SetPlayerWantedLevel(API.PlayerId(), level, false);
            API.SetPlayerWantedLevelNow(API.PlayerId(), false);
            API.SendNuiMessage($@"{{""type"":""levelChange"",""level"":{level}}}");
            API.SendNuiMessage($@"{{""type"":""update"",""killCount"":{killCount},""level"":{currentLevel}}}");
        }
        private void hlog(string message, bool debug, bool screen)
        {
            if (debug) Debug.WriteLine(message);
            if (screen) Screen.ShowNotification(message);
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
        private void InitializePlayer()
        {
            int playerPed = API.PlayerPedId();
            // playerPed.Weapons.RemoveAll();
            // playerPed.Weapons.Give(WeaponHash.Pistol, 9999, true, true);
            // API.SetPlayerWeaponAmmo(API.GetPlayerPed(-1), (uint)WeaponHash.Pistol, 9999);
            API.GiveWeaponToPed(playerPed, (uint)WeaponHash.Pistol, 9999, false, true);
            API.SetPedInfiniteAmmo(playerPed, true, (uint)WeaponHash.Pistol);
            API.SetPlayerWantedLevel(playerPed, 1, false);
            API.SetPlayerWantedLevelNow(playerPed, false);
        }
        private async void StartPoliceSpawning()
        {
            isSpawning = true;
            while (isSpawning)
            {
                await SpawnPolice();
                await Delay((int)(spawnInterval * 1000));
            }
        }
    }
}