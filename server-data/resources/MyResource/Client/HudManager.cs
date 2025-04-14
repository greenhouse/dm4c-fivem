using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
// using Mono.CSharp;

namespace DeathmatchClient
{
    public class HudManager : BaseScript
    {
        private int LIVE_AMMO = 0;
        private int RESERVE_AMMO = 0;
        private int LAST_WEAPON_AMMO_CNT = 0;
        private int LAST_WEAPONHASH_SELECT = 0; // Track last equipped weapon
        private bool hudVisible = true; // Track HUD state

        // weapon type to bullet token value
        private readonly int WEAPONHASH_NONE = -1569615261;
        private readonly string[] WEAPON_NAME_LIST;
        private readonly Dictionary<int, int> WEAPONHASH_BULLET_VALUE;
        private readonly Dictionary<int, string> WEAPONHASH_TO_NAME;

        public HudManager()
        {
            WEAPON_NAME_LIST = new string[]
            {
                "WEAPON_PISTOL",       // Index 0
                "WEAPON_ASSAULTRIFLE", // Index 1
                "WEAPON_PUMPSHOTGUN",  // Index 2
                "WEAPON_SNIPERRIFLE",  // Index 3
                "WEAPON_GRENADE",      // Index 4
                "WEAPON_RPG",          // Index 5
                "WEAPON_HOMINGLAUNCHER", // Index 6
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
                int playerPed = API.PlayerPedId();
                uint weaponHash = (uint)API.GetHashKey(WEAPON_NAME_LIST[0]);
                API.GiveWeaponToPed(playerPed, weaponHash, 0, false, true); // Equip pistol
                API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[1]), 0, false, false);
                API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[2]), 0, false, false);
                API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[3]), 0, false, false);
                API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[4]), 0, false, false);
                API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[5]), 0, false, false);
                API.GiveWeaponToPed(playerPed, (uint)API.GetHashKey(WEAPON_NAME_LIST[6]), 0, false, false);

                hlog($"Gave default guns with 0 ammo.", false, true); // debug, screen
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

            // transfer reserve ammo to loaded ammo
            API.RegisterCommand("/loadreserve", new Action<int, dynamic>((source, args) =>
            {
                if (RESERVE_AMMO == 0)
                {
                    hlog("No reserve ammo available. Use /givereserve first.", true, true); // debug, screen
                    return;
                }

                // set reserve ammo (TODO: need to acceept arg for amount)
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
    }
}