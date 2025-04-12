using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;

namespace DeathmatchClient
{
    public class HudManager : BaseScript
    {
        private int reserveAmmo = 0;
        private bool hudVisible = true; // Track HUD state

        public HudManager()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["updateAmmoReserve"] += new Action<int>(OnUpdateAmmoReserve);
            Tick += UpdateHud;

            // Register test command
            API.RegisterCommand("togglehud", new Action<int, dynamic>((source, args) =>
            {
                hudVisible = !hudVisible; // Toggle state
                API.SendNuiMessage($@"{{""type"": ""showHud"", ""visible"": {hudVisible.ToString().ToLower()}}}");
                Screen.ShowNotification($"HUD {(hudVisible ? "enabled" : "disabled")}");
            }), false);

            // New givehandgun command
            API.RegisterCommand("givehandgun", new Action<int, dynamic>((source, args) =>
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
                // if (args.Count > 0 && int.TryParse(args[0].ToString(), out int parsedAmmo))
                // {
                //     ammo = parsedAmmo;
                // }

                // Give pistol and loaded ammo
                uint weaponHash = (uint)API.GetHashKey("WEAPON_PISTOL");
                int playerPed = API.PlayerPedId();
                API.GiveWeaponToPed(playerPed, weaponHash, 0, false, true); // Equip pistol
                API.AddAmmoToPed(playerPed, weaponHash, ammo); // Add loaded ammo
                Screen.ShowNotification($"Gave pistol with {ammo} loaded ammo.");

                // Trigger server event to update reserve ammo
                TriggerServerEvent("purchaseAmmo", ammo); // Reuse existing event
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
            reserveAmmo = reserve;
            UpdateNui();
        }

        private async Task UpdateHud()
        {
            int loadedAmmo = API.GetAmmoInPedWeapon(API.PlayerPedId(), (uint)API.GetHashKey("WEAPON_PISTOL"));
            UpdateNui(loadedAmmo);
            await Task.FromResult(0);
        }

        private void UpdateNui(int loadedAmmo = -1)
        {
            if (loadedAmmo == -1)
                loadedAmmo = API.GetAmmoInPedWeapon(API.PlayerPedId(), (uint)API.GetHashKey("WEAPON_PISTOL"));
            
            API.SendNuiMessage($@"{{
                ""type"": ""updateAmmo"",
                ""loaded"": {loadedAmmo},
                ""reserve"": {reserveAmmo}
            }}");
        }
    }
}