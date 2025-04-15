using CitizenFX.Core;
using System;
using System.Collections.Generic;
// using System.Threading.Tasks;
// using System.Linq;

namespace DeathmatchServer
{
    public class AmmoManager : BaseScript
    {
        // Player handle -> reserve ammo
        private Dictionary<int, int> playerReserves = new Dictionary<int, int>(); 
        //  <pickupId, <bulletAmnt, playerPedDrop, playerPedPickup, isPickedUp>>
        private Dictionary<int, Tuple<int, int, Vector3, int, bool>> pickups = new Dictionary<int, Tuple<int, int, Vector3, int, bool>>();
            
        private int nextPickupId = 1;
        public AmmoManager()
        {
            EventHandlers["playerSpawned"] += new Action<Player>(OnPlayerSpawned);
            EventHandlers["purchaseAmmo"] += new Action<Player, int>(OnPurchaseAmmo);
            EventHandlers["loadReserveAmmo"] += new Action<Player, int>(OnLoadReserveAmmo);
            EventHandlers["playerDiedDropAmmo"] += new Action<Player, int, Vector3>(OnPlayerDiedDropAmmo);
            EventHandlers["playerPickedUpAmmo"] += new Action<Player, int>(OnPlayerPickedUpAmmo);
            EventHandlers["requestPickupSync"] += new Action<Player>(OnRequestPickupSync);
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
        //     // note_041225: 'GetValueOrDefault' fails to compile, even w/ 'using System.Linq;'
        //     //  alt 'OnPurchaseAmmo' integration below
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
        private void OnLoadReserveAmmo([FromSource] Player player, int amount)
        {
            int playerHandle = int.Parse(player.Handle);
            int currentReserve = playerReserves.ContainsKey(playerHandle) ? playerReserves[playerHandle] : 0;
            playerReserves[playerHandle] = currentReserve - amount;
            player.TriggerEvent("updateAmmoReserve", playerReserves[playerHandle]);
            Debug.WriteLine($"{player.Name} loaded {amount} reserve ammo. New reserve: {playerReserves[playerHandle]}");
        }
        private void OnPlayerDiedDropAmmo([FromSource] Player player, int bulletAmnt, Vector3 deathCoords)
        {
            int playerHandleDrop = int.Parse(player.Handle);
            int pickupId = nextPickupId++;
            pickups[pickupId] = new Tuple<int, int, Vector3, int, bool>(bulletAmnt, playerHandleDrop, deathCoords, -1, false);
            TriggerClientEvent("spawnBulletTokenPickup", player.Name, pickupId, bulletAmnt, deathCoords);
            Debug.WriteLine($"Player {player.Name}({playerHandleDrop}) Dropped {bulletAmnt} $BULLET tokens _ at: ({deathCoords})");
        }

        private void OnPlayerPickedUpAmmo([FromSource] Player player, int pickupId)
        {   
            int playerHandlePickup = int.Parse(player.Handle);

            // validate not already picked up
            if (pickups[pickupId].Item5 == true) { 
                Debug.WriteLine($"ERROR: Player {player.Name}({playerHandlePickup}) tried pick-up on id: {pickupId}; $BULLET token already picked-up. returning");
                return;
            }
            
            // calc new reserve amount
            int oldReserve = playerReserves.ContainsKey(playerHandlePickup) ? playerReserves[playerHandlePickup] : 0;
            int bullAmntPickup = pickups[pickupId].Item1;
            int newReserve = oldReserve + bullAmntPickup;

            // set pickup player new reserve amount
            playerReserves[playerHandlePickup] = newReserve;

            // update pickupId to picked-up = true (w/ playerHandlePickup)
            pickups[pickupId] = Tuple.Create(pickups[pickupId].Item1, pickups[pickupId].Item2, pickups[pickupId].Item3, playerHandlePickup, true);

            // trigger pickup client to update their reserve amount
            player.TriggerEvent("updateAmmoReserve", playerReserves[playerHandlePickup]);
            Debug.WriteLine($"Player {player.Name}({playerHandlePickup}) Picked-up {bullAmntPickup} $BULLET tokens _ Reserve (Old->New): {oldReserve} -> {newReserve}");
        }
        private void OnRequestPickupSync([FromSource] Player player)
        {
            foreach (var kvp in pickups)
            {
                int pickupId = kvp.Key;
                bool isPickedUp = pickups[pickupId].Item5;
                if (!isPickedUp) { 
                    int bulletAmnt = pickups[pickupId].Item1;
                    Vector3 deathCoords = pickups[pickupId].Item3;
                    player.TriggerEvent("spawnBulletTokenPickup", player.Name, pickupId, bulletAmnt, deathCoords);
                    Debug.WriteLine($"Sent $BULLET token pickup-sync to client: Player {player.Name}({int.Parse(player.Handle)}) _ pickupId: {pickupId} _ $BULLET token amount: {bulletAmnt} _ coords: {deathCoords}");

                }
            }
        }
    }
}