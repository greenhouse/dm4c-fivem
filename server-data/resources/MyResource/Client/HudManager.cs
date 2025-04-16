using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
// using System.Diagnostics;
// using Mono.CSharp;

namespace DeathmatchClient
{
    public class HudManager : BaseScript
    {
        private int F4_KEY_RESERVE_AMNT = 200; // default reserve ammo amount
        private int LIVE_AMMO = 0;
        private int RESERVE_AMMO = 0;
        private int LAST_WEAPON_AMMO_CNT = 0;
        private int LAST_WEAPONHASH_SELECT = 0; // Track last equipped weapon
        private bool LAST_DEAD_CHECKED = false; // Debounce death triggers
        private bool hudVisible = true; // Track HUD state

        // weapon type to bullet token value
        private readonly int WEAPONHASH_NONE = -1569615261;
        private readonly string[] WEAPON_NAME_LIST;
        private readonly Dictionary<int, int> WEAPONHASH_BULLET_VALUE;
        private readonly Dictionary<int, string> WEAPONHASH_TO_NAME;
        private readonly Dictionary<int, Tuple<int, Vector3, int, int, bool>> BULLET_PICKUPS = new Dictionary<int, Tuple<int, Vector3, int, int, bool>>(); // <pickupId <pickupHandle, deathCoords, bulletAmnt, bilp, isPickedUp>>

        public HudManager()
        {
            // init globals
            WEAPON_NAME_LIST = new string[]
            {
                "WEAPON_PISTOL",       // Index 0
                "WEAPON_ASSAULTRIFLE", // Index 1
                "WEAPON_PUMPSHOTGUN",  // Index 2
                "WEAPON_SNIPERRIFLE",  // Index 3
                "WEAPON_GRENADE",      // Index 4
                "WEAPON_RPG",          // Index 5
                "WEAPON_HOMINGLAUNCHER", // Index 6
                "GADGET_PARACHUTE", // Index 7
            };
            WEAPONHASH_BULLET_VALUE = new Dictionary<int, int>
            {
                { WEAPONHASH_NONE, 0 },      // None: 0 $BULLET = $0.00
                { API.GetHashKey(WEAPON_NAME_LIST[0]), 1 },      // Handgun: 1 $BULLET = $0.01
                { API.GetHashKey(WEAPON_NAME_LIST[1]), 2 }, // AR: 2 $BULLET = $0.02
                { API.GetHashKey(WEAPON_NAME_LIST[2]), 4 }, // Shotgun: 4 $BULLET = $0.04
                { API.GetHashKey(WEAPON_NAME_LIST[3]), 5 }, // Sniper: 5 $BULLET = $0.05
                { API.GetHashKey(WEAPON_NAME_LIST[4]), 7 },     // Grenade: 7 $BULLET = $0.07
                { API.GetHashKey(WEAPON_NAME_LIST[5]), 10 },         // Rocket Launcher: 10 $BULLET = $0.10
                { API.GetHashKey(WEAPON_NAME_LIST[6]), 20 }, // Homing Launcher: 20 $BULLET = $0.20
                { API.GetHashKey(WEAPON_NAME_LIST[7]), 0 } // Homing Launcher: 20 $BULLET = $0.00
            };
            WEAPONHASH_TO_NAME = new Dictionary<int, string>
            {
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[0])), WEAPON_NAME_LIST[0] },      // Handgun
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[1])), WEAPON_NAME_LIST[1] }, // AR
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[2])), WEAPON_NAME_LIST[2] },  // Shotgun
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[3])), WEAPON_NAME_LIST[3] }, // Sniper
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[4])), WEAPON_NAME_LIST[4] },     // Grenade
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[5])), WEAPON_NAME_LIST[5] },          // Rocket Launcher
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[6])), WEAPON_NAME_LIST[6] },          // Homing Launcher
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[7])), WEAPON_NAME_LIST[7] }          // Homing Launcher
            };

            // NOTE: these registers depend on globals above
            RegisterEventHanlders();
            RegisterTickHandlers();
            RegisterCommands();
        }

        /* -------------------------------------------------------- */
        /* PRIVATE - initialization support
        /* -------------------------------------------------------- */
        private void RegisterEventHanlders() {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["updateAmmoReserve"] += new Action<int>(OnUpdateAmmoReserve);
            EventHandlers["spawnBulletTokenPickup"] += new Action<String, int, int, Vector3>(OnSpawnBulletTokenPickup);
            EventHandlers["removeBulletTokenPickup"] += new Action<String, int>(OnRemoveBulletTokenPickup);
            
            EventHandlers["playerSpawned"] += new Action(OnPlayerSpawned);
        }
        private void RegisterTickHandlers() {
            Tick += UpdateHud;
            Tick += UpdateWeapon;
            Tick += CheckDeath;
            Tick += CheckForPickup;
            Tick += SyncPickups;
            Tick += CreatePickups;
            Tick += DrawPickups;
            Tick += OnKeyPress;
        }
        private void RegisterCommands() {
            // Register test command
            API.RegisterCommand("/togglehud", new Action<int, dynamic>((source, args) =>
            {
                hudVisible = !hudVisible; // Toggle state
                API.SendNuiMessage($@"{{""type"": ""showHud"", ""visible"": {hudVisible.ToString().ToLower()}}}");
                hlog($"HUD {(hudVisible ? "enabled" : "disabled")}", false, true); // debug, screen
            }), false);

            // New givehandgun command
            API.RegisterCommand("/givehandgun", new Action<int, dynamic>((source, args) =>
            {
                int ammo = 50; // Default ammo
                if (args.Count > 0)
                {
                    if (int.TryParse(args[0].ToString(), out int parsedAmmo))
                    {
                        ammo = parsedAmmo;
                    }
                    else
                    {
                        hlog("Invalid ammo amount. Using default (50).", false, true); // debug, screen
                    }
                }

                // Give pistol and loaded ammo
                uint weaponHash = (uint)API.GetHashKey("WEAPON_PISTOL");
                int playerPed = API.PlayerPedId();
                API.GiveWeaponToPed(playerPed, weaponHash, 0, false, true); // Equip pistol
                API.AddAmmoToPed(playerPed, weaponHash, ammo); // Add loaded ammo
                hlog($"Gave pistol with {ammo} loaded ammo.", false, true); // debug, screen

                // Trigger server event to update reserve ammo
                TriggerServerEvent("purchaseAmmo", ammo); // Reuse existing event
            }), false);

            // New giveguns command
            API.RegisterCommand("/giveguns", new Action<int, dynamic>((source, args) =>
            {
                // Give default weapons with 0 ammo
                giveDefaultWeapons();                
            }), false);

            // New givehandgun command
            API.RegisterCommand("/givereserve", new Action<int, dynamic>((source, args) =>
            {
                int ammo = 50; // Default ammo
                if (args.Count > 0)
                {
                    if (int.TryParse(args[0].ToString(), out int parsedAmmo))
                    {
                        ammo = parsedAmmo;
                    }
                    else
                    {
                        hlog($"Invalid ammo amount. Using default {ammo}.", false, true); // debug, screen
                    }
                }

                // Trigger server event to update reserve ammo
                TriggerServerEvent("purchaseAmmo", ammo); // Reuse existing event
            }), false);

            // transfer reserve ammo to live ammo
            API.RegisterCommand("/loadreserve", new Action<int, dynamic>((source, args) =>
            {
                if (RESERVE_AMMO == 0)
                {
                    hlog("No reserve ammo available. Use /givereserve first.", true, true); // debug, screen
                    return;
                }

                // set reserve ammo
                int ammo = RESERVE_AMMO;
                if (args.Count > 0) {
                    if (int.TryParse(args[0].ToString(), out int parsedAmmo)) {
                        if (parsedAmmo <= RESERVE_AMMO) {
                            ammo = parsedAmmo;
                        } else {
                            hlog($"Not enough reserve ammo. Using default max reserve {ammo}.", false, true); // debug, screen
                        }
                    } else {
                        hlog($"No|Invalid ammo amount. Using default {ammo}.", false, true); // debug, screen
                    }
                }

                // increment live ammo
                LIVE_AMMO += ammo;

                // Trigger server event to update reserve ammo
                TriggerServerEvent("loadReserveAmmo", ammo); // Reuse existing event
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

            // manually sync pickups from server side
            API.RegisterCommand("/syncpickups", new Action<int, dynamic>((source, args) =>
            {
                TriggerServerEvent("requestPickupSync"); // Ask server for active pickups
                hlog("YOU manually requested pickup sync", true, true); // debug, screen
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

            // manually sync pickups from server side
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

                // Spawn the waverunner near the player
                int playerPed = API.PlayerPedId();
                Vector3 playerPos = API.GetEntityCoords(playerPed, true);
                int vehicle = API.CreateVehicle(vehicleHash, playerPos.X, playerPos.Y, playerPos.Z + 2.0f, API.GetEntityHeading(playerPed), true, false);

                // Place the player in the vehicle
                API.SetPedIntoVehicle(playerPed, vehicle, -1); // -1 is the driver seat

                // Enable weapon usage in the vehicle -> NOTE_041625: not working, can't seem to ride with weapon & shoot
                API.SetPedConfigFlag(playerPed, 184, true); // Allow weapons in vehicle
                API.SetPedCanSwitchWeapon(playerPed, true); // Allow weapon switching
                API.SetPlayerCanDoDriveBy(API.PlayerId(), true); // Enable drive-by shooting
                API.SetCurrentPedWeapon(playerPed, (uint)API.GetHashKey("WEAPON_PISTOL"), true); // Equip a default weapon

                hlog($"YOU manually requested a boat type: {vehicleHash}", true, true); // debug, screen
            }), false);

            // Register the /quit command
            API.RegisterCommand("/quit", new Action<int, dynamic>((source, args) =>
            {
                QuitServer(); // Trigger server event to disconnect
            }), false);
            
        }

        /* -------------------------------------------------------- */
        /* PRIVATE - event hanlders                            
        /* -------------------------------------------------------- */
        private void OnPlayerSpawned()
        {   
            hlog("YOU respawned", true, true); // debug, screen
            requestPickupSync();
            giveDefaultWeapons();
        }
        private void OnJumpCommand(Vector3 coords, float range=0)
        {
            // Teleport player to new coordinates
            float X = coords.X+range;
            float Y = coords.Y+range;
            float Z = coords.Z+range;
            API.SetEntityCoords(API.PlayerPedId(), X, Y, Z, false, false, false, false);
            hlog($"YOU jumped to coords: {X}, {Y}, {Z}", true, true); // debug, screen

            requestPickupSync();
        }
        private void OnClientResourceStart(string resourceName)
        {
            // Load NUI HUD
            API.SetNuiFocus(false, false);
            API.SendNuiMessage(@"{""type"": ""showHud"", ""visible"": true}");
        }

        private void OnUpdateAmmoReserve(int reserve)
        {
            // set client side reserve for HUD
            RESERVE_AMMO = reserve;

            // SetPedAmmo w/ LIVE_AMMO & WEAPONHASH_BULLET_VALUE calc
            SetPedAmmoWithBulletValue();
            
            // update HUD
            hlog($"Updating HUD with LIVE_AMMO: {LIVE_AMMO} | RESERVE_AMMO: {RESERVE_AMMO}", true, false); // debug, screen
            UpdateNui(LIVE_AMMO);
        }
        private void OnSpawnBulletTokenPickup(String playerName, int pickupId, int bulletAmnt, Vector3 deathCoords)
        {
            // Add blip to minimap (regardless of player range)
            int blip = API.AddBlipForCoord(deathCoords.X, deathCoords.Y, deathCoords.Z);
            API.SetBlipSprite(blip, 1); // Circle sprite
            API.SetBlipColour(blip, 46); // Gold color
            API.SetBlipScale(blip, 0.8f);
            API.SetBlipAsShortRange(blip, true); // Only show when nearby (optional)
            // pickupBlips[pickupId] = blip;

            // Draw marker above pickup (fails if player not in range)
            Vector3 playerPos = Game.PlayerPed.Position;
            DrawBulletTokenPickupMarker(playerPos, deathCoords, bulletAmnt); 

            // generate pickup and store in BULLET_PICKUPS
            uint pickupHash = (uint)API.GetHashKey("PICKUP_MONEY_VARIABLE");
            int newPickup = API.CreatePickup(pickupHash, deathCoords.X, deathCoords.Y, deathCoords.Z, 0, bulletAmnt, false, 0);
            BULLET_PICKUPS[pickupId] = new Tuple<int, Vector3, int, int, bool>(newPickup, deathCoords, bulletAmnt, blip, false);
            hlog($"Player {playerName} Dropped {bulletAmnt} $BULLET tokens _ at: ({deathCoords})", true, false); // debug, screen
        }
        private void OnRemoveBulletTokenPickup(String playerName, int pickupId)
        {
            int blip = BULLET_PICKUPS[pickupId].Item4;
            API.RemoveBlip(ref blip);

            // BULLET_PICKUPS.Remove(pickupId); // note: may becausing exception
            BULLET_PICKUPS[pickupId] = Tuple.Create(BULLET_PICKUPS[pickupId].Item1, BULLET_PICKUPS[pickupId].Item2, BULLET_PICKUPS[pickupId].Item3, BULLET_PICKUPS[pickupId].Item4, true);
            hlog($"Player {playerName} Removed $BULLET token pickup _ pickupId: {pickupId}", true, true); // debug, screen
        }

        /* -------------------------------------------------------- */
        /* PRIVATE - frame/task loop support                            
        /* -------------------------------------------------------- */
        private async Task OnKeyPress()
        {
            // Keybind logic
            if (API.IsControlJustPressed(0, 288)) { // F1
                hlog("F1 -> TODO: show HUD for all 'F' key presses", true, true); // debug, screen
            }
            else if (API.IsControlJustPressed(0, 289)) { // F2 -> show HUD weapon ammo to $BULLET pricing
                hlog("F2 -> TODO: show HUD weapon ammo to $BULLET pricing", true, true); // debug, screen
            }
            else if (API.IsControlJustPressed(0, 290)) { // F3 -> get all weapons
                giveDefaultWeapons();
                hlog("F3: got weapons", true, true); // debug, screen
            }
            else if (API.IsControlJustPressed(0, 291)) { // F4 -> get some reserve $BULLET tokens
                TriggerServerEvent("purchaseAmmo", F4_KEY_RESERVE_AMNT); // Reuse existing event
                hlog($"F4: got {F4_KEY_RESERVE_AMNT} reserve $BULLET tokens", true, true); // debug, screen
            }
            else if (API.IsControlJustPressed(0, 292)) { // F5 -> load ammo from reserve
                int ammo = RESERVE_AMMO > 0 ? RESERVE_AMMO / 2 : 0; // default reserve ammo amount
                if (ammo == 0) hlog("not enouch reserve $BULLET to load, use F4 or /givereserve <amnt>", true, true); // debug, screen

                // increment live ammo & Trigger server event to update reserve ammo
                LIVE_AMMO += ammo;
                TriggerServerEvent("loadReserveAmmo", ammo); // Reuse existing event
                hlog("F5: loaded half your reserve $BULLET into ammo", true, true); // debug, screen
            }
            else if (API.IsControlJustPressed(0, 293)) { // F6 -> quit/leave server
                QuitServer();
                hlog("F6: quit server", true, true); // debug, screen
            }
            
            await Delay(10);
        }
        private async Task DrawPickups() {
            // Draw markers for all active pickups
            foreach (var kvp in BULLET_PICKUPS)
            {
                int pickupId = kvp.Key;
                // int pickupHandle = BULLET_PICKUPS[pickupId].Item1;
                Vector3 deathCoords = BULLET_PICKUPS[pickupId].Item2;
                int bulletAmnt = BULLET_PICKUPS[pickupId].Item3;
                bool isPickedUp = BULLET_PICKUPS[pickupId].Item5;
                Vector3 playerPos = Game.PlayerPed.Position;

                // Check if player is near pickup & $BULLET is not picked up yet
                //  NOTE: isPickedUp (.Item5) not needed anymore w/ removeBulletTokenPickup integration
                //      ie. Tick CheckForPickup trigger -> server side playerPickedUpAmmo
                //          server side playerPickedUpAmmo trigger -> ALL client side removeBulletTokenPickup
                //          client side removeBulletTokenPickup -> invokes BULLET_PICKUPS.Remove(pickupId);
                if (!isPickedUp && Vector3.Distance(playerPos, deathCoords) < 10000f)
                {
                    // Draw marker above pickup (fails if player not in range)
                    //  NOTE: fivem docs says needs to be called every tick/frame
                    DrawBulletTokenPickupMarker(playerPos, deathCoords, bulletAmnt);

                    // grok say delay 500 to avoid overloading w/ draw calls
                    // await Delay(500); // wait 500ms (0.5 sec)
                }
            }
            await Task.FromResult(0);
        }
        private async Task CreatePickups() {
            // Draw markers for all active pickups
            foreach (var kvp in BULLET_PICKUPS)
            {
                int pickupId = kvp.Key;
                // int pickupHandle = BULLET_PICKUPS[pickupId].Item1;
                Vector3 deathCoords = BULLET_PICKUPS[pickupId].Item2;
                int bulletAmnt = BULLET_PICKUPS[pickupId].Item3;
                bool isPickedUp = BULLET_PICKUPS[pickupId].Item5;
                Vector3 playerPos = Game.PlayerPed.Position;

                // Check if player is near pickup & $BULLET is not picked up yet
                //  NOTE: isPickedUp (.Item5) not needed anymore w/ removeBulletTokenPickup integration
                //      ie. Tick CheckForPickup trigger -> server side playerPickedUpAmmo
                //          server side playerPickedUpAmmo trigger -> ALL client side removeBulletTokenPickup
                //          client side removeBulletTokenPickup -> invokes BULLET_PICKUPS.Remove(pickupId);
                // note: 500f is way too far for game core to display (i think)
                if (!isPickedUp && Vector3.Distance(playerPos, deathCoords) < 500f)
                {
                    // generate pickup
                    uint pickupHash = (uint)API.GetHashKey("PICKUP_MONEY_VARIABLE");
                    int newPickup = API.CreatePickup(pickupHash, deathCoords.X, deathCoords.Y, deathCoords.Z, 0, bulletAmnt, false, 0);

                    // avoid overloading w/ create calls
                    await Delay(1000); // wait 1000ms (1 sec)
                }
            }
            await Task.FromResult(0);
        }
        private async Task SyncPickups()
        {
            // NOTE: requestPickupSync() triggers -> server side "requestPickupSync" 
            //  server side "requestPickupSync" -> checks for non-picked up $BULLET tokens
            //  server side "requestPickupSync" triggers -> client side "spawnBulletTokenPickup"
            //  client side "spawnBulletTokenPickup" -> invokes API.CreatePickup(<coords>)
            //  HOWEVER, game core doesn't generate pickups until player is within (some) range
            //   HENCE, requesting sync w/ (some) delay to compensate for player travel time
            //
            // UPDTE: the above might not be needed, if CreatePickups works correcly (ie. Tick checking distance)
            //  HENCE, this Tick SyncPickups is only needed to gradually keep sync (precautionary, 60 sec maybe?)
            requestPickupSync();

            // Check every 60000ms (60 seconds)
            await Delay(60000); 
        }
        private async Task CheckForPickup()
        {
            foreach(var kvp in BULLET_PICKUPS) {
                int pickupId = kvp.Key;
                int pickupHandle = BULLET_PICKUPS[pickupId].Item1;
                bool isPickedUp = BULLET_PICKUPS[pickupId].Item5;
                if (API.HasPickupBeenCollected(pickupHandle) && !isPickedUp)
                {
                    // NOTE: new removeBulletTokenPickup, allows no need to set BULLET_PICKUPS isPickedUp = true 
                    //  ie. bool isPickedUp can be removed from BULLET_PICKUPS data structure
                    // BULLET_PICKUPS[pickupId] = Tuple.Create(BULLET_PICKUPS[pickupId].Item1, BULLET_PICKUPS[pickupId].Item2, true);
                    TriggerServerEvent("playerPickedUpAmmo", pickupId);
                    hlog($"YOU picked-up {BULLET_PICKUPS[pickupId].Item3} reserve $BULLET tokens", true, true);
                }
            }
            await Delay(100); // Check every 100ms to reduce load
        }
        private async Task CheckDeath()
        {
            int playerPed = API.PlayerPedId();
            bool isDead = API.IsPedFatallyInjured(playerPed);

            if (isDead && !LAST_DEAD_CHECKED)
            {
                Vector3 deathCoords = API.GetEntityCoords(playerPed, false);
                hlog($"GetEntityCoords -> deathCoords: {deathCoords.X} {deathCoords.Y} {deathCoords.Z}", true, false); // debug, screen
                TriggerServerEvent("playerDiedDropAmmo", LIVE_AMMO, deathCoords);
                hlog($"YOU died, dropping {LIVE_AMMO} 'live' $BULLET tokens at: {deathCoords}", true, false); // debug, screen
                hlog($"YOU died, dropping {LIVE_AMMO} 'live' $BULLET tokens", false, true); // debug, screen
                LAST_DEAD_CHECKED = true;

                // Reset live ammo on death
                LIVE_AMMO = 0; 
                UpdateNui(LIVE_AMMO);
                await Delay(2000); // Cooldown to prevent repeat triggers
            }
            else if (!isDead && LAST_DEAD_CHECKED)
            {
                LAST_DEAD_CHECKED = false; // Reset after respawn
            }

            await Delay(100); // Check every 100ms
        }
        private async Task UpdateWeapon()
        {
            // get current player w/ weaponHash selected
            int playerPed = API.PlayerPedId();
            int weaponHashSel = API.GetSelectedPedWeapon(playerPed);

            // Check / log weapon switch
            if (LAST_WEAPONHASH_SELECT != weaponHashSel)
            {
                // get new and last weapon names for logging
                string weaponName = WEAPONHASH_TO_NAME.TryGetValue(weaponHashSel, out string name) ? name : $"Unknown (0x{weaponHashSel:X8})";
                string weaponNameLast = WEAPONHASH_TO_NAME.TryGetValue(LAST_WEAPONHASH_SELECT, out string nameLast) ? nameLast : $"Unknown (0x{LAST_WEAPONHASH_SELECT:X8})";

                // reset last weapon ammo to 0
                API.SetPedAmmo(playerPed, (uint)LAST_WEAPONHASH_SELECT, 0);
                int lastAmmo = API.GetAmmoInPedWeapon(playerPed, (uint)LAST_WEAPONHASH_SELECT);

                // SetPedAmmo w/ LIVE_AMMO & WEAPONHASH_BULLET_VALUE calc
                SetPedAmmoWithBulletValue();
                
                // log weapon switch
                hlog($"Weapon switched _from: {LAST_WEAPONHASH_SELECT} _to: {weaponHashSel}", false, false); // debug, screen
                hlog($"Weapon switched _from: {weaponNameLast} _to: {weaponName}, reset prev weapon ammo to: {lastAmmo}", true, false); // debug, screen

                // Update last weapon select & ammo count (for tracking LIVE_AMMO amount)
                LAST_WEAPONHASH_SELECT = weaponHashSel; 
            }
            await Task.FromResult(0);
        }
        private async Task UpdateHud()
        {
            // get current player w/ weaponHash selected
            int playerPed = API.PlayerPedId();
            int weaponHashSel = API.GetSelectedPedWeapon(playerPed);
            
            if (API.IsPedShooting(playerPed)) {
                hlog($"API.IsPedShooting invoked w/ LIVE_AMMO: {LIVE_AMMO}, LAST_WEAPON_AMMO_CNT: {LAST_WEAPON_AMMO_CNT}", true, false); // debug, screen

                // set 0 ammo if negative
                if (LIVE_AMMO <= 0) {
                    hlog($"No Loaded ammo. Use|Fill Reserve!", true, true); // bool: debug, screen
                    LIVE_AMMO = 0;
                    LAST_WEAPON_AMMO_CNT = 0;

                    // SetPedAmmo w/ LIVE_AMMO & WEAPONHASH_BULLET_VALUE calc
                    SetPedAmmoWithBulletValue();
                } else {
                    // calc weaponHash ammo discharged during this task (and update global for next task)
                    int weaponAmmoCurr = API.GetAmmoInPedWeapon(playerPed, (uint)weaponHashSel);
                    int weapAmmoDischarge = LAST_WEAPON_AMMO_CNT - weaponAmmoCurr; // NOTE: calc total discharge amnt incase frames/tasks are missed
                    hlog($"weaponAmmoCurr: {weaponAmmoCurr}, weapAmmoDischarge: {weapAmmoDischarge}", false, false); // debug, screen
                    
                    // calc loaded ammo for HUD update
                    int bulletVal = WEAPONHASH_BULLET_VALUE[weaponHashSel]; // get $BULLET token value per weapon type
                    // LIVE_AMMO = LIVE_AMMO - bulletVal; // calc new total LIVE_AMMO
                    LIVE_AMMO = LIVE_AMMO - (bulletVal * weapAmmoDischarge); // calc new total LIVE_AMMO
                    LAST_WEAPON_AMMO_CNT = weaponAmmoCurr; // save and update last ammo count for next task calc
                }

                // update HUD
                UpdateNui(LIVE_AMMO); 
            }
            await Task.FromResult(0);
        }

        /* -------------------------------------------------------- */
        /* PRIVATE - algorthimic support                            
        /* -------------------------------------------------------- */
        private void QuitServer() {
            // Notify the player
            hlog("YOU are Disconnecting from the server...", true, true); // debug, screen

            // Trigger server event to disconnect
            TriggerServerEvent("playerQuit");
        }
        private void giveDefaultWeapons() {
            // Give default weapons with 0 ammo
            int playerPed = API.PlayerPedId();
            uint weaponHash = (uint)API.GetHashKey(WEAPON_NAME_LIST[0]);
            API.GiveWeaponToPed(playerPed, weaponHash, 0, false, true); // Equip pistol
            API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[1]), 0, false, false);
            API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[2]), 0, false, false);
            API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[3]), 0, false, false);
            API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[4]), 0, false, false);
            API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[5]), 0, false, false);
            API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[6]), 0, false, false);
            API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[7]), 1, false, false);

            hlog($"Gave default guns with 0 ammo.", false, true); // debug, screen
        }
        private void requestPickupSync()
        {
            // Request active pickups from server
            // BULLET_PICKUPS.Clear(); // Clear old pickups (just in case, but shouldn't matter)
            TriggerServerEvent("requestPickupSync");
            hlog("YOU requested pickup sync", true, false); // debug, screen
        }
        private void SetPedAmmoWithBulletValue()
        {
            // SetPedAmmo w/ LIVE_AMMO & WEAPONHASH_BULLET_VALUE calc
            //  NOTE: uses WEAPONHASH_BULLET_VALUE to calc usable ammo
            int playerPed = API.PlayerPedId();
            int weaponHashSel = API.GetSelectedPedWeapon(playerPed);
            int bulletVal = WEAPONHASH_BULLET_VALUE[weaponHashSel]; // get $BULLET token value per weapon type
            int calcAmmoAvail = bulletVal > 0 ? LIVE_AMMO / bulletVal : 0; // calc available ammo using bulletVal for this weapon
            API.SetPedAmmo(playerPed, (uint)weaponHashSel, calcAmmoAvail);

            // update last ammo count for next task calc
            LAST_WEAPON_AMMO_CNT = API.GetAmmoInPedWeapon(playerPed, (uint)weaponHashSel);
        }
        private void UpdateNui(int loadedAmmo = -1)
        {
            if (loadedAmmo == -1)
                loadedAmmo = API.GetAmmoInPedWeapon(API.PlayerPedId(), (uint)API.GetHashKey("WEAPON_PISTOL"));
            
            API.SendNuiMessage($@"{{
                ""type"": ""updateAmmo"",
                ""live"": {LIVE_AMMO},
                ""reserve"": {RESERVE_AMMO}
            }}");
        }
        private void hlog(string message, bool debug, bool screen)
        {
            if (debug) Debug.WriteLine(message);
            if (screen) Screen.ShowNotification(message);
        }
    
        /* -------------------------------------------------------- */
        /* PRIVATE - display support                            
        /* -------------------------------------------------------- */
        private void DrawBulletTokenPickupMarker(Vector3 playerPos, Vector3 deathCoords, int bulletAmnt)
        {
            // Draw marker above pickup (if player in range)
            API.DrawMarker(
                1, // Marker type (ground circle)
                deathCoords.X, deathCoords.Y, deathCoords.Z - 0.9f,
                0f, 0f, 0f, // Direction
                0f, 0f, 0f, // Rotation
                1f, 1f, 0.5f, // Scale
                255, 215, 0, 100, // Gold color with alpha
                false, true, 2, false, null, null, false
            );

            // Draw floating text (if player in range)
            float dist = Vector3.Distance(playerPos, deathCoords);
            DrawText3D(deathCoords + new Vector3(0f, 0f, 0.5f), $"{bulletAmnt} $BULLET tokens\n DIST: {dist:F2}m");
        }
        private void DrawText3D(Vector3 pos, string text)
        {
            float screenX = 0f, screenY = 0f;
            bool onScreen = API.World3dToScreen2d(pos.X, pos.Y, pos.Z, ref screenX, ref screenY);
            if (onScreen)
            {
                API.SetTextScale(0.35f, 0.35f);
                API.SetTextFont(4);
                API.SetTextProportional(true);
                API.SetTextColour(255, 255, 255, 255);
                API.SetTextOutline();
                API.BeginTextCommandDisplayText("STRING");
                API.AddTextComponentSubstringPlayerName(text);
                API.EndTextCommandDisplayText(screenX, screenY);
            }
        }
    }
}