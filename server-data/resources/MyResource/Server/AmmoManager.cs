using CitizenFX.Core;
using System;
using System.Collections.Generic;
// using System.Threading.Tasks;
// using System.Linq;

namespace DeathmatchServer
{
    public class AmmoManager : BaseScript
    {
        private Dictionary<int, int> playerReserves = new Dictionary<int, int>(); // Player handle -> reserve ammo

        public AmmoManager()
        {
            EventHandlers["playerSpawned"] += new Action<Player>(OnPlayerSpawned);
            EventHandlers["purchaseAmmo"] += new Action<Player, int>(OnPurchaseAmmo);
        }

        private void OnPlayerSpawned([FromSource] Player player)
        {
            int playerHandle = int.Parse(player.Handle);
            if (!playerReserves.ContainsKey(playerHandle))
            {
                playerReserves[playerHandle] = 0; // Initialize reserve ammo
            }
            player.TriggerEvent("updateAmmoReserve", playerReserves[playerHandle]);
        }

        // private void OnPurchaseAmmo([FromSource] Player player, int amount)
        // {
        //     int playerHandle = int.Parse(player.Handle);
        //     playerReserves[playerHandle] = playerReserves.GetValueOrDefault(playerHandle, 0) + amount;
        //     player.TriggerEvent("updateAmmoReserve", playerReserves[playerHandle]);
        //     Debug.WriteLine($"{player.Name} purchased {amount} ammo. New reserve: {playerReserves[playerHandle]}");
        // }
        private void OnPurchaseAmmo([FromSource] Player player, int amount)
        {
            int playerHandle = int.Parse(player.Handle);
            int currentReserve = playerReserves.ContainsKey(playerHandle) ? playerReserves[playerHandle] : 0;
            playerReserves[playerHandle] = currentReserve + amount;
            player.TriggerEvent("updateAmmoReserve", playerReserves[playerHandle]);
            Debug.WriteLine($"{player.Name} purchased {amount} ammo. New reserve: {playerReserves[playerHandle]}");
        }
    }
}