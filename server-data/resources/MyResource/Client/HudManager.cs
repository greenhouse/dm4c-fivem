using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DeathmatchClient
{
    public class HudManager : BaseScript
    {
        private int LOADED_AMMO = 0;
        private int RESERVE_AMMO = 0;
        private int LAST_WEAPON_AMMO = 0;
        private int LAST_WEAPONHASH_SELECT = 0; // Track last equipped weapon
        private bool hudVisible = true; // Track HUD state

        // weapon type to bullet token value
        private readonly string[] WEAPON_NAME_LIST = new string[]
        {
            "WEAPON_PISTOL",       // Index 0
            "WEAPON_ASSAULTRIFLE", // Index 1
            "WEAPON_PUMPSHOTGUN",  // Index 2
            "WEAPON_SNIPERRIFLE",  // Index 3
            "WEAPON_GRENADE",      // Index 4
            "WEAPON_RPG",          // Index 5
            "WEAPON_HOMINGLAUNCHER" // Index 6
        };
        // String text = WEAPON_NAME_LIST[0];
        private readonly Dictionary<int, int> WEAPONHASH_BULLET_VALUE;
        private readonly Dictionary<int, string> WEAPONHASH_TO_NAME;
        // private readonly Dictionary<int, int> WEAPONHASH_BULLET_VALUE = new Dictionary<int, int>
        // {
        //     { API.GetHashKey(WEAPON_NAME_LIST[0]), 1 },      // Handgun: 1 $BULLET = $0.01
        //     { API.GetHashKey("WEAPON_ASSAULTRIFLE"), 2 }, // AR: 2 $BULLET = $0.02
        //     { API.GetHashKey("WEAPON_PUMPSHOTGUN"), 4 }, // Shotgun: 4 $BULLET = $0.04
        //     { API.GetHashKey("WEAPON_SNIPERRIFLE"), 5 }, // Sniper: 5 $BULLET = $0.05
        //     { API.GetHashKey("WEAPON_GRENADE"), 7 },     // Grenade: 7 $BULLET = $0.07
        //     { API.GetHashKey("WEAPON_RPG"), 10 },         // Rocket Launcher: 10 $BULLET = $0.10
        //     { API.GetHashKey("WEAPON_HOMINGLAUNCHER"), 20 } // Homing Launcher: 20 $BULLET = $0.20
            
        // };
        // private readonly Dictionary<int, string> WEAPONHASH_TO_NAME = new Dictionary<int, string>
        // {
        //     { unchecked(API.GetHashKey("WEAPON_PISTOL")), "WEAPON_PISTOL" },      // Handgun
        //     { unchecked(API.GetHashKey("WEAPON_ASSAULTRIFLE")), "WEAPON_ASSAULTRIFLE" }, // AR
        //     { unchecked(API.GetHashKey("WEAPON_PUMPSHOTGUN")), "WEAPON_PUMPSHOTGUN" },  // Shotgun
        //     { unchecked(API.GetHashKey("WEAPON_SNIPERRIFLE")), "WEAPON_SNIPERRIFLE" }, // Sniper
        //     { unchecked(API.GetHashKey("WEAPON_GRENADE")), "WEAPON_GRENADE" },     // Grenade
        //     { unchecked(API.GetHashKey("WEAPON_RPG")), "WEAPON_RPG" },          // Rocket Launcher
        //     { unchecked(API.GetHashKey("WEAPON_HOMINGLAUNCHER")), "WEAPON_HOMINGLAUNCHER" }          // Homing Launcher
        // };
        public HudManager()
        {
            WEAPONHASH_BULLET_VALUE = new Dictionary<int, int>
            {
                { API.GetHashKey(WEAPON_NAME_LIST[0]), 1 },      // Handgun: 1 $BULLET = $0.01
                { API.GetHashKey(WEAPON_NAME_LIST[1]), 2 }, // AR: 2 $BULLET = $0.02
                { API.GetHashKey(WEAPON_NAME_LIST[2]), 4 }, // Shotgun: 4 $BULLET = $0.04
                { API.GetHashKey(WEAPON_NAME_LIST[3]), 5 }, // Sniper: 5 $BULLET = $0.05
                { API.GetHashKey(WEAPON_NAME_LIST[4]), 7 },     // Grenade: 7 $BULLET = $0.07
                { API.GetHashKey(WEAPON_NAME_LIST[5]), 10 },         // Rocket Launcher: 10 $BULLET = $0.10
                { API.GetHashKey(WEAPON_NAME_LIST[6]), 20 } // Homing Launcher: 20 $BULLET = $0.20
                
            };
            WEAPONHASH_TO_NAME = new Dictionary<int, string>
            {
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[0])), WEAPON_NAME_LIST[0] },      // Handgun
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[1])), WEAPON_NAME_LIST[1] }, // AR
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[2])), WEAPON_NAME_LIST[2] },  // Shotgun
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[3])), WEAPON_NAME_LIST[3] }, // Sniper
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[4])), WEAPON_NAME_LIST[4] },     // Grenade
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[5])), WEAPON_NAME_LIST[5] },          // Rocket Launcher
                { unchecked(API.GetHashKey(WEAPON_NAME_LIST[6])), WEAPON_NAME_LIST[6] }          // Homing Launcher
            };

            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["updateAmmoReserve"] += new Action<int>(OnUpdateAmmoReserve);
            Tick += UpdateHud;
            Tick += UpdateWeapon;

            // Register test command
            API.RegisterCommand("/togglehud", new Action<int, dynamic>((source, args) =>
            {
                hudVisible = !hudVisible; // Toggle state
                API.SendNuiMessage($@"{{""type"": ""showHud"", ""visible"": {hudVisible.ToString().ToLower()}}}");
                Screen.ShowNotification($"HUD {(hudVisible ? "enabled" : "disabled")}");
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
                        Screen.ShowNotification("Invalid ammo amount. Using default (50).");
                    }
                }

                // Give pistol and loaded ammo
                uint weaponHash = (uint)API.GetHashKey("WEAPON_PISTOL");
                int playerPed = API.PlayerPedId();
                API.GiveWeaponToPed(playerPed, weaponHash, 0, false, true); // Equip pistol
                API.AddAmmoToPed(playerPed, weaponHash, ammo); // Add loaded ammo
                Screen.ShowNotification($"Gave pistol with {ammo} loaded ammo.");

                // Trigger server event to update reserve ammo
                TriggerServerEvent("purchaseAmmo", ammo); // Reuse existing event
            }), false);

            // New giveguns command
            API.RegisterCommand("/giveguns", new Action<int, dynamic>((source, args) =>
            {
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

                Screen.ShowNotification($"Gave default guns with 0 ammo.");
            }), false);

            // // New give gun choice command: givegun 
            // API.RegisterCommand("givegun", new Action<int, dynamic>((source, args) =>
            // {
            //     int ammo = 10; // Default ammo
            //     string weaponName = "WEAPON_PISTOL"; // Default weapon
            //     uint weaponHash = (uint)API.GetHashKey("WEAPON_PISTOL"); // Default weapon
            //     int playerPed = API.PlayerPedId();
                
            //     if (args.count > 0) {
            //         weaponName = args[0].ToString();
            //         weaponHash = (uint)API.GetHashKey(weaponName);   

            //         if (args.Count > 1) {
            //             if (int.TryParse(args[1].ToString(), out int parsedAmmo)) {
            //                 ammo = parsedAmmo;
            //             } else {
            //                 Screen.ShowNotification($"No|Invalid ammo amount. Using default {ammo}.");
            //             }
            //         }
            //     } else {
            //         Screen.ShowNotification($"No weapon name or ammount count provided. Using default {weaponName} & {ammo}.");
            //     }

            //     // Give gun and loaded ammo
            //     API.GiveWeaponToPed(playerPed, weaponHash, 0, false, true); // Equip weapon
            //     API.AddAmmoToPed(playerPed, weaponHash, ammo); // Add loaded ammo
            //     Screen.ShowNotification($"Gave {weaponName} with {ammo} loaded ammo.");

            //     // Trigger server event to update reserve ammo
            //     TriggerServerEvent("purchaseAmmo", ammo); // Reuse existing event
            // }), false);

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
                        Screen.ShowNotification($"Invalid ammo amount. Using default {ammo}.");
                    }
                }

                // Trigger server event to update reserve ammo
                TriggerServerEvent("purchaseAmmo", ammo); // Reuse existing event
            }), false);

            // transfer reserve ammo to loaded ammo
            API.RegisterCommand("/loadreserve", new Action<int, dynamic>((source, args) =>
            {
                if (RESERVE_AMMO == 0)
                {
                    Screen.ShowNotification("No reserve ammo available. Use /givegun first.");
                    return;
                }

                // set reserve ammo (TODO: need to acceept arg for amount)
                int ammo = RESERVE_AMMO;
                if (args.Count > 0) {
                    if (int.TryParse(args[0].ToString(), out int parsedAmmo)) {
                        if (parsedAmmo <= RESERVE_AMMO) {
                            ammo = parsedAmmo;
                        } else {
                            Screen.ShowNotification($"Not enough reserve ammo. Using default max reserve {ammo}.");
                        }
                    } else {
                        Screen.ShowNotification($"No|Invalid ammo amount. Using default {ammo}.");
                    }
                }

                LOADED_AMMO = ammo;

                // // Give pistol and loaded ammo
                // uint weaponHash = (uint)API.GetHashKey("WEAPON_PISTOL");
                // int playerPed = API.PlayerPedId();
                // int weaponHashSlot = API.GetPedWeapontypeInSlot(playerPed, weaponHash); // returns 0 if weaponHash slot is empty
                // int weaponHashSel = API.GetSelectedPedWeapon(playerPed); // returns weaponHash player currently holding
                // // int curr_weaponHash = API.GetCurrentPedWeapon(playerPed); // Get current weapon
                // // API.GetAmmoInPedWeapon
                // API.GiveWeaponToPed(playerPed, weaponHash, 0, false, true); // Equip pistol
                // API.AddAmmoToPed(playerPed, weaponHash, ammo); // Add loaded ammo
                // API.SetPedAmmo(int ped, uint weaponHash, int ammo);
                // Screen.ShowNotification($"Gave pistol with {ammo} loaded ammo.");

                // Trigger server event to update reserve ammo
                TriggerServerEvent("loadReserveAmmo", ammo); // Reuse existing event
            }), false);
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

            // set ped's selected weapon with LOADED_AMMO amount
            SetWeaponSelectLoadedAmmo();
            
            // update HUD
            UpdateNui(LOADED_AMMO);
        }
        private void SetWeaponSelectLoadedAmmo()
        {
            // get current player w/ weaponHash selected
            int playerPed = API.PlayerPedId();
            int weaponHashSel = API.GetSelectedPedWeapon(playerPed);

            // set ped's selected weapon with LOADED_AMMO amount
            //  NOTE: maxes out weapon ammount count if needed, doesn't matter
            API.SetPedAmmo(playerPed, (uint)weaponHashSel, LOADED_AMMO);
        }

        // LEFT OFF HERE ... this doesn't seem to work proplery 
        //      every weapon change seems to add to total LOADED_AMMO count in HUD
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

                // set ped's selected weapon with LOADED_AMMO amount
                API.SetPedAmmo(playerPed, (uint)weaponHashSel, LOADED_AMMO);

                // log weapon switch
                String log = $"Weapon switched from: {weaponNameLast} _ to: {weaponName}, reset last ammo to: {lastAmmo}";
                Debug.WriteLine(log);
                Screen.ShowNotification(log);

                // Update last weapon
                LAST_WEAPONHASH_SELECT = weaponHashSel; 
            }
            await Task.FromResult(0);
        }
        private async Task UpdateHud()
        {
            // get current player w/ weaponHash selected
            int playerPed = API.PlayerPedId();
            int weaponHashSel = API.GetSelectedPedWeapon(playerPed);

            // // Check / log weapon switch
            // if (LAST_WEAPONHASH_SELECT != weaponHashSel)
            // {
            //     // get new and last weapon names for logging
            //     string weaponName = WEAPONHASH_TO_NAME.TryGetValue(weaponHashSel, out string name) ? name : $"Unknown (0x{weaponHashSel:X8})";
            //     string weaponNameLast = WEAPONHASH_TO_NAME.TryGetValue(LAST_WEAPONHASH_SELECT, out string nameLast) ? nameLast : $"Unknown (0x{LAST_WEAPONHASH_SELECT:X8})";

            //     // set ped's selected weapon with LOADED_AMMO amount
            //     SetWeaponSelectLoadedAmmo();

            //     // reset last weapon ammo to 0
            //     API.SetPedAmmo(playerPed, (uint)LAST_WEAPONHASH_SELECT, 0);
            //     int lastAmmo = API.GetAmmoInPedWeapon(playerPed, (uint)LAST_WEAPONHASH_SELECT);

            //     // log weapon switch
            //     String log = $"Weapon switched from: {weaponNameLast} _ to: {weaponName}, reset last ammo to: {lastAmmo}";
            //     Debug.WriteLine(log);
            //     Screen.ShowNotification(log);

            //     // Update last weapon
            //     LAST_WEAPONHASH_SELECT = weaponHashSel; 
            // }
            
            // calc weaponHash ammo discharged during this task (and update global for next task)
            int weaponAmmoCurr = API.GetAmmoInPedWeapon(playerPed, (uint)weaponHashSel);
            // int weapAmmoDischarge = LAST_WEAPON_AMMO - weaponAmmoCurr;
            // LAST_WEAPON_AMMO = weaponAmmoCurr; // save and update for next task calc

            // calc loaded ammo to update HUD with
            int bulletVal = WEAPONHASH_BULLET_VALUE[weaponHashSel]; // get $BULLET token value per weapon type
            // LOADED_AMMO = LOADED_AMMO - (bulletVal * weapAmmoDischarge); // calc new total LOADED_AMMO
            LOADED_AMMO = LOADED_AMMO - bulletVal; // calc new total LOADED_AMMO
            UpdateNui(LOADED_AMMO); // update HUD
            await Task.FromResult(0);
        }

        private void UpdateNui(int loadedAmmo = -1)
        {
            if (loadedAmmo == -1)
                loadedAmmo = API.GetAmmoInPedWeapon(API.PlayerPedId(), (uint)API.GetHashKey("WEAPON_PISTOL"));
            
            API.SendNuiMessage($@"{{
                ""type"": ""updateAmmo"",
                ""loaded"": {LOADED_AMMO},
                ""reserve"": {RESERVE_AMMO}
            }}");
        }
    }
}