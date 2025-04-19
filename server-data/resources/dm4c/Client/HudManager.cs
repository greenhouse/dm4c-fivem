using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;
// using System.Diagnostics;
// using Mono.CSharp;

namespace DeathmatchClient
{
    public class BulletPickupInfo
    {
        public int pickupId { get; set; }
        public int pickupHandle { get; set; }
        public Vector3 deathCoords { get; set; }
        public int bulletAmnt { get; set; }
        public bool isCollected { get; set; }
    }
    public class HudManager : BaseScript
    {
        /* -------------------------------------------------------- */
        /* GLOBALS - settings
        /* -------------------------------------------------------- */
        private bool SHOW_CMD_HUD = false;
        private bool SHOW_PRICE_HUD = false;
        private int F4_KEY_RESERVE_AMNT = 200; // default reserve ammo amount
        private int LIVE_AMMO = 0;
        private int RESERVE_AMMO = 0;
        private int LAST_WEAPON_AMMO_CNT = 0;
        private int LAST_WEAPONHASH_SELECT = 0; // Track last equipped weapon
        private bool LAST_DEAD_CHECKED = false; // Debounce death triggers
        private bool HUD_VISIBLE = true; // Track HUD state

        /* -------------------------------------------------------- */
        /* GLOBALS - weapons
        /* -------------------------------------------------------- */
        private readonly uint PICKUPHASH_MONEY = (uint)API.GetHashKey("PICKUP_MONEY_VARIABLE");
        private readonly int WEAPONHASH_NONE = -1569615261;
        private readonly string[] WEAPON_NAME_LIST;
        private readonly Dictionary<int, int> WEAPONHASH_BULLET_VALUE;
        private readonly Dictionary<int, string> WEAPONHASH_TO_NAME;

        /* -------------------------------------------------------- */
        /* GLOBALS - bullet pickup tracking
        /* -------------------------------------------------------- */
        private readonly List<BulletPickupInfo> BULLET_PICKUPS = new List<BulletPickupInfo>();
        // private Dictionary<int, Tuple<Vector3, int, int, bool>> BULLET_PICKUPS = new Dictionary<int, Tuple<Vector3, int, int, bool>>(); // <pickupId, <deathCoords, bulletAmnt, blip, isPickedUp>>
        private Dictionary<int, Tuple<int, bool>> BULLET_PICKUP_HANDLES = new Dictionary<int, Tuple<int, bool>>(); // <pickupId, <pickupHandle, isCollected>>
        private Dictionary<int, int> BULLET_BLIPS = new Dictionary<int, int>(); // <pickupId, blip>
        private bool FLAG_PICKUPIDS_WRITING = false; // pickupId flag for reads to wait
        private bool FLAG_PICKUPIDS_READING = true; // pickupId flag for writes to wait
        // private bool FLAG_CREATING_PICKUP = false; // pickup create flag
        // private bool FLAG_CHECKING_PICKUP = true; // pickup check flag
        private readonly float DIST_DRAW_PLAYER_DEATHCOORD = 10000f; // max range to draw pickup text for player to see
        // private readonly float DIST_CREATE_PLAYER_DEATHCOORD = 500f; // max range to create pickup for player to see & grab

        /* -------------------------------------------------------- */
        /* CONSTRUCTOR
        /* -------------------------------------------------------- */
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
                { API.GetHashKey(WEAPON_NAME_LIST[7]), 0 } // parachute: 20 $BULLET = $0.00
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
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[7])), WEAPON_NAME_LIST[7] }          // parachute
            };

            // NOTE: these registers depend on globals above
            RegisterEventHanlders();
            RegisterTickHandlers();
            RegisterCommands();

            API.SetThisScriptCanRemoveBlipsCreatedByAnyScript(true); // Allow other scripts to remove blips
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
            // Tick += SyncPickups;
            Tick += DrawPickups;
            Tick += OnKeyPress;
            Tick += OnKeyPressF1;
            Tick += OnKeyPressF2;
            Tick += InfiniteSprint;
        }
        private void RegisterCommands() {

            // get curreny coords
            API.RegisterCommand("/givetest", new Action<int, dynamic>((source, args) =>
            {
                GiveTestSetup();

            }), false);

            // get curreny coords
            API.RegisterCommand("/givetest", new Action<int, dynamic>((source, args) =>
            {
                GiveTestSetup();

            }), false);

            // Register test command
            API.RegisterCommand("/togglehud", new Action<int, dynamic>((source, args) =>
            {
                HUD_VISIBLE = !HUD_VISIBLE; // Toggle state
                API.SendNuiMessage($@"{{""type"": ""showHud"", ""visible"": {HUD_VISIBLE.ToString().ToLower()}}}");
                hlog($"HUD {(HUD_VISIBLE ? "enabled" : "disabled")}", false, true); // debug, screen
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
                // uint vehicleHash = (uint)API.GetHashKey("PCJ");
                // uint vehicleHash = (uint)API.GetHashKey("BMX");
                // uint vehicleHash = (uint)API.GetHashKey("policeb");
                // uint vehicleHash = (uint)API.GetHashKey("BATI");
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
        private void OnPlayerSpawned()
        {   
            hlog("YOU respawned", true, true); // debug, screen
            requestPickupSync(); // need to get BULLET_PICKUPS from server (for "draw" and blips)
            giveDefaultWeapons();
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
            // Add blip to minimap (no game core player range requirements)
            int blip = GenerateMinimapBlip(deathCoords.X, deathCoords.Y, deathCoords.Z);
            BULLET_BLIPS[pickupId] = blip;

            // Draw marker above pickup (fails if player not in game core supported range)
            DrawBulletTokenPickupMarker(Game.PlayerPed.Position, deathCoords, bulletAmnt, pickupId); 

            // generate pickup and store in BULLET_PICKUPS
            int pickupHandle = API.CreatePickup(PICKUPHASH_MONEY, deathCoords.X, deathCoords.Y, deathCoords.Z, 0, bulletAmnt, false, 0);
                // API.CreateMoneyPickups(deathCoords.X, deathCoords.Y, deathCoords.Z, bulletAmnt, 10, 0);

            // var pickup = BULLET_PICKUPS.Find(p => Vector3.Distance(p.Position, position) < 0.1f && p.AmmoType == ammoType);
            int index = BULLET_PICKUPS.FindIndex(p => p.pickupId == pickupId);
            if (index >= 0)
            {
                BULLET_PICKUPS[index].pickupHandle = pickupHandle;
                BULLET_PICKUPS[index].deathCoords = deathCoords;
                BULLET_PICKUPS[index].bulletAmnt = bulletAmnt;
                BULLET_PICKUPS[index].isCollected = false;
            }
            else
            {
                BULLET_PICKUPS.Add(new BulletPickupInfo
                        {
                            pickupId = pickupId,
                            pickupHandle = pickupHandle,
                            deathCoords = deathCoords,
                            bulletAmnt = bulletAmnt,
                            isCollected = false
                        });
            }
            // BULLET_PICKUPS[pickupId] = new Tuple<Vector3, int, int, bool>(deathCoords, bulletAmnt, blip, false);

            // WriteToBulletPickups(pickupId, pickupHandle, new Tuple<Vector3, int, int, bool>(deathCoords, bulletAmnt, blip, false), false);
            hlog($"YOU Dropped {bulletAmnt} $BULLET tokens _ at: ({deathCoords})", true, false); // debug, screen
        }
        private async void OnRemoveBulletTokenPickup(String playerName, int pickupId)
        {
            // wait on writing flag to finish
            while (FLAG_PICKUPIDS_WRITING) await Delay(10);

            // set reading flag (so other threads wait to write)
            FLAG_PICKUPIDS_READING = true;
            // int blip = BULLET_PICKUPS[pickupId].Item3;
            int blip = BULLET_BLIPS[pickupId];
            
            // API.RemoveBlip(ref blip);
            API.RemoveBlip(ref blip);
            BULLET_BLIPS.Remove(pickupId);

            // unset reading flag (so other threads can write)
            FLAG_PICKUPIDS_READING = false;

            // WriteToBulletPickups(pickupId, BULLET_PICKUP_HANDLES[pickupId].Item1, Tuple.Create(BULLET_PICKUPS[pickupId].Item1, BULLET_PICKUPS[pickupId].Item2, BULLET_PICKUPS[pickupId].Item3, true), true);
            hlog($"YOU Removed $BULLET token pickup _ pickupId: {pickupId}", true, true); // debug, screen
        }
        // public async void WriteToBulletPickups(int pickupId, int pickupHandle, Tuple<Vector3, int, int, bool> bulletPickup, bool isRemove) 
        // {
        //     // wait on reading flag to finish 
        //     while (FLAG_PICKUPIDS_READING) await Delay(10);

        //     // set writing flag (so other threads wait to read)
        //     FLAG_PICKUPIDS_WRITING = true;
        //     hlog("NOTE: flag set to true, attempting write", true, false); // debug, screen
            
        //     BULLET_PICKUPS.Add(new BulletPickupInfo
        //             {
        //                 pickupId = pickupId,
        //                 pickupHandle = pickupHandle,
        //                 deathCoords = bulletPickup.Item1,
        //                 bulletAmnt = bulletPickup.Item2,
        //                 isCollected = bulletPickup.Item4
        //             });
        //     // perform write (remove or add accordingly)
        //     // if (isRemove) BULLET_PICKUPS.Remove(pickupId);
        //     // else BULLET_PICKUPS[pickupId] = bulletPickup;
        //     // BULLET_PICKUPS[pickupId] = bulletPickup;
        //     BULLET_PICKUP_HANDLES[pickupId] = Tuple.Create(pickupHandle, isRemove); // always overwrite (never remove handles)
        //     hlog("NOTE: write complete, attempting flag set to false", true, false); // debug, screen

        //     // unset writing flag (so other threads can read)
        //     FLAG_PICKUPIDS_WRITING = false;
        //     hlog("NOTE: flag set to false", true, false); // debug, screen
        // }

        /* -------------------------------------------------------- */
        /* PRIVATE - frame/task loop support                            
        /* -------------------------------------------------------- */
        private async Task CheckForPickup()
        {
            if (BULLET_PICKUPS.Count == 0) return;
            Vector3 playerPos = Game.PlayerPed.Position;
        
            foreach(BulletPickupInfo pickup in BULLET_PICKUPS) 
            {
                if (pickup.isCollected) continue; // skip this task if writing
                if (Vector3.Distance(playerPos, pickup.deathCoords) > 2.0f) continue;
                if (API.DoesPickupExist(pickup.pickupHandle) && API.HasPickupBeenCollected(pickup.pickupHandle)) {
                    pickup.isCollected = true;
                    TriggerServerEvent("playerPickedUpAmmo", pickup.pickupId); // triggers client side: OnRemoveBulletTokenPickup
                    hlog($"YOU picked-up pickupId: {pickup.pickupId} w/ {pickup.bulletAmnt} reserve $BULLET tokens", true, true);
                }
            }

            // Clean up collected pickups
            BULLET_PICKUPS.RemoveAll(p => p.isCollected && !API.DoesPickupExist(p.pickupHandle));
            await Task.FromResult(0);
        }
        private async Task DrawPickups() 
        {
            if (BULLET_PICKUPS.Count == 0) return;
            Vector3 playerPos = Game.PlayerPed.Position;

            // Draw markers for all active pickups
            foreach(BulletPickupInfo pickup in BULLET_PICKUPS) 
            {
                if (pickup.isCollected) continue; // skip this task if writing
                if (Vector3.Distance(playerPos, pickup.deathCoords) > DIST_DRAW_PLAYER_DEATHCOORD) continue;
                DrawBulletTokenPickupMarker(playerPos, pickup.deathCoords, pickup.bulletAmnt, pickup.pickupId);
            }
            await Task.FromResult(0);
        }
        // private async Task SyncPickups()
        // {
        //     hlog($"Syncing pickups...{DateTime.Now}", true, false); // debug, screen
        //     // NOTE: requestPickupSync() triggers -> server side "requestPickupSync" 
        //     //  server side "requestPickupSync" -> checks for non-picked up $BULLET tokens
        //     //  server side "requestPickupSync" triggers -> client side "spawnBulletTokenPickup"
        //     //  client side "spawnBulletTokenPickup" -> invokes API.CreatePickup(<coords>)
        //     //  HOWEVER, game core doesn't generate pickups until player is within (some) range
        //     //   HENCE, requesting sync w/ (some) Delay to compensate for player travel time
        //     //
        //     // UPDTE: the above might not be needed, if CreatePickups works correcly (ie. Tick checking distance)
        //     //  HENCE, this Tick SyncPickups is only needed to gradually keep sync (precautionary, 60 sec maybe?)
        //     // requestPickupSync();

        //     // Check every 60000ms (60 seconds)
        //     await Delay(60000); 
        // }
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
        private async Task OnKeyPress()
        {
            // Keybind logic (note_041725: tried IsControlJustReleased as well, still not working correctly)
            if (API.IsControlJustReleased(0, 288)) { // F1
                hlog("F1: show/hid HUD commands", true, true); // debug, screen
                SHOW_PRICE_HUD = false;
                SHOW_CMD_HUD = !SHOW_CMD_HUD;
            }
            else if (API.IsControlJustReleased(0, 289)) { // F2 -> show HUD weapon ammo to $BULLET pricing
                hlog("F2: show/hide HUD $BULLET ammo pricing", true, true); // debug, screen
                SHOW_CMD_HUD = false;
                SHOW_PRICE_HUD = !SHOW_PRICE_HUD;
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
            
            await Delay(0);
        }
        public async Task OnKeyPressF1()
        {
            if (SHOW_CMD_HUD) {
                // Define text lines to display
                string[] textLines = new[]
                {
                    "Welcome to DM4C (Death Match 4 Cash)!",
                    "------------------------------------",
                    "F1: Show/Hide this commands HUD",
                    "F2: Show/Hide ammo pricing HUD",
                    "------------------------------------",
                    "Info ...",
                    "------------------------------------",
                    "when you get killed, you drop your 'live' $BULLET ammo (view HUD in bottom right)",
                    "GOAL: kill others, pickup their dropped $BULLET tokens, swap your $BULLET for USD on blockchain",
                    "------------------------------------",
                    "Commands ... (press F8; init: /givetest)",
                    "------------------------------------",
                    "/giveguns: get default weapons (no ammo)",
                    "/givereserve [amnt]: buy $BULLET tokens (HUD bottom right)",
                    "/loadreserve [amnt]: load 'reserve' $BULLET into 'live' ammo (HUD bottom right)",
                    "/givebike: get motor bike",
                    "/giveboat: get wave runner",
                    "/givecar: get police car",
                    "/quit: exit server",
                };

                DrawInfoHud(textLines);
            }

            await Task.FromResult(0);
        }
        public async Task OnKeyPressF2()
        {
            if (SHOW_PRICE_HUD) {
                // Define text lines to display
                String descr = $"$BULLET per shot";
                string[] textLines = new[]
                {
                    "Welcome to DM4C (Death Match 4 Cash)!",
                    "------------------------------------",
                    "F1: Show/Hide commands HUD",
                    "F2: Show/Hide this ammo pricing HUD",
                    "------------------------------------",
                    "Ammo Pricing ... Note: 1 $BULLET = $0.01 (1 penny)",
                    " (pay attention to HUD in bottom right) ",
                    "------------------------------------",
                    $"{WEAPON_NAME_LIST[0]} = {WEAPONHASH_BULLET_VALUE[API.GetHashKey(WEAPON_NAME_LIST[0])]} {descr}",
                    $"{WEAPON_NAME_LIST[1]} = {WEAPONHASH_BULLET_VALUE[API.GetHashKey(WEAPON_NAME_LIST[1])]} {descr}",
                    $"{WEAPON_NAME_LIST[2]} = {WEAPONHASH_BULLET_VALUE[API.GetHashKey(WEAPON_NAME_LIST[2])]} {descr}",
                    $"{WEAPON_NAME_LIST[3]} = {WEAPONHASH_BULLET_VALUE[API.GetHashKey(WEAPON_NAME_LIST[3])]} {descr}",
                    $"{WEAPON_NAME_LIST[4]} = {WEAPONHASH_BULLET_VALUE[API.GetHashKey(WEAPON_NAME_LIST[4])]} {descr}",
                    $"{WEAPON_NAME_LIST[5]} = {WEAPONHASH_BULLET_VALUE[API.GetHashKey(WEAPON_NAME_LIST[5])]} {descr}",
                    $"{WEAPON_NAME_LIST[6]} = {WEAPONHASH_BULLET_VALUE[API.GetHashKey(WEAPON_NAME_LIST[6])]} {descr}",
                    $"{WEAPON_NAME_LIST[7]} = {WEAPONHASH_BULLET_VALUE[API.GetHashKey(WEAPON_NAME_LIST[7])]} {descr}",
                };

                DrawInfoHud(textLines);
            }

            await Task.FromResult(0);
        }
        private async Task InfiniteSprint()
        {
            // Set stamina to 100%
            API.SetPlayerStamina(Game.Player.Handle, 100.0f);
            await Delay(100); // Run every 100ms
        }

        /* -------------------------------------------------------- */
        /* PRIVATE - algorthimic support                            
        /* -------------------------------------------------------- */
        private void GiveTestSetup(){
            // Give default weapons with 0 ammo
            giveDefaultWeapons();
            TriggerServerEvent("purchaseAmmo", 500); // Reuse existing event
            int ammo = 50;
            LIVE_AMMO += ammo;
            TriggerServerEvent("loadReserveAmmo", ammo); // Reuse existing event
            hlog($"YOU got weapons test setup", true, true); // debug, screen
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
            // if (loadedAmmo == -1)
            //     loadedAmmo = API.GetAmmoInPedWeapon(API.PlayerPedId(), (uint)API.GetHashKey("WEAPON_PISTOL"));
            
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
        private int GenerateMinimapBlip(float X, float Y, float Z) {
            // Add blip to minimap (regardless of player range)
            int blip = API.AddBlipForCoord(X, Y, Z);
            API.SetBlipSprite(blip, 1); // Circle sprite
            API.SetBlipColour(blip, 46); // Gold color
            API.SetBlipScale(blip, 0.8f);
            API.SetBlipAsShortRange(blip, true); // Only show when nearby (optional)
            return blip;       
        }
        private void DrawBulletTokenPickupMarker(Vector3 playerPos, Vector3 deathCoords, int bulletAmnt, int pickupId)
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
            DrawText3D(deathCoords + new Vector3(0f, 0f, 0.5f), $"{bulletAmnt} $BULLET\n DIST: {dist:F2}m\n ID: {pickupId}");
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
        // Helper function to draw 2D text
        private void DrawText2D(string text, float x, float y, float scale, int r, int g, int b, int a, bool center)
        {
            API.SetTextFont(0); // Default font
            API.SetTextScale(scale, scale);
            API.SetTextColour(r, g, b, a);
            API.SetTextCentre(center);
            API.SetTextOutline(); // Add outline for readability
            API.BeginTextCommandDisplayText("STRING");
            API.AddTextComponentString(text);
            API.EndTextCommandDisplayText(x, y);
        }
        private void DrawInfoHud(string[] textLines) {
                // Draw a semi-transparent white square in the center of the screen
                DrawRect(0.5f, 0.5f, 0.6f, 0.95f, 0, 0, 0, 150);

                // Draw each line of text inside the square
                float startY = 0.03f; // Starting Y position (top of the square)
                float lineSpacing = 0.05f; // Spacing between lines
                for (int i = 0; i < textLines.Length; i++)
                {
                    DrawText2D(textLines[i], 0.5f, startY + (i * lineSpacing), 0.35f, 255, 255, 255, 255, true);
                }
        }
    }
}