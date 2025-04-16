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
        private Dictionary<int, int> PLAYER_RESERVES = new Dictionary<int, int>(); 
        //  <pickupId, <bulletAmnt, playerPedDrop, playerPedPickup, isPickedUp>>
        private Dictionary<int, Tuple<int, int, Vector3, int, bool>> PICKUPS = new Dictionary<int, Tuple<int, int, Vector3, int, bool>>();
        private int NEXT_PICKUP_ID = 1;
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
            if (!PLAYER_RESERVES.ContainsKey(playerHandle))
            {
                PLAYER_RESERVES[playerHandle] = 0; // Initialize reserve ammo
            }
            player.TriggerEvent("updateAmmoReserve", PLAYER_RESERVES[playerHandle]);
        }

        // private void OnPurchaseAmmo([FromSource] Player player, int amount)
        // {
        //     int playerHandle = int.Parse(player.Handle);
        //     // note_041225: 'GetValueOrDefault' fails to compile, even w/ 'using System.Linq;'
        //     //  alt 'OnPurchaseAmmo' integration below
        //     PLAYER_RESERVES[playerHandle] = PLAYER_RESERVES.GetValueOrDefault(playerHandle, 0) + amount;
        //     player.TriggerEvent("updateAmmoReserve", PLAYER_RESERVES[playerHandle]);
        //     Debug.WriteLine($"{player.Name} purchased {amount} ammo. New reserve: {PLAYER_RESERVES[playerHandle]}");
        // }
        private void OnPurchaseAmmo([FromSource] Player player, int amount)
        {
            int playerHandle = int.Parse(player.Handle);
            int currentReserve = PLAYER_RESERVES.ContainsKey(playerHandle) ? PLAYER_RESERVES[playerHandle] : 0;
            PLAYER_RESERVES[playerHandle] = currentReserve + amount;
            player.TriggerEvent("updateAmmoReserve", PLAYER_RESERVES[playerHandle]);
            Debug.WriteLine($"{player.Name} purchased {amount} ammo. New reserve: {PLAYER_RESERVES[playerHandle]}");
        }
        private void OnLoadReserveAmmo([FromSource] Player player, int amount)
        {
            int playerHandle = int.Parse(player.Handle);
            int currentReserve = PLAYER_RESERVES.ContainsKey(playerHandle) ? PLAYER_RESERVES[playerHandle] : 0;
            PLAYER_RESERVES[playerHandle] = currentReserve - amount;
            player.TriggerEvent("updateAmmoReserve", PLAYER_RESERVES[playerHandle]);
            Debug.WriteLine($"{player.Name} loaded {amount} reserve ammo. New reserve: {PLAYER_RESERVES[playerHandle]}");
        }
        private void OnPlayerDiedDropAmmo([FromSource] Player player, int bulletAmnt, Vector3 deathCoords)
        {
            int playerHandleDrop = int.Parse(player.Handle);
            int pickupId = NEXT_PICKUP_ID++;
            PICKUPS[pickupId] = new Tuple<int, int, Vector3, int, bool>(bulletAmnt, playerHandleDrop, deathCoords, -1, false);

            // Notify ALL clients to spawn the $BULLET token pickup
            TriggerClientEvent("spawnBulletTokenPickup", player.Name, pickupId, bulletAmnt, deathCoords);
            
            // Notify only NEARBY clients to spawn the $BULLET TOKEN pickup
            // foreach (Player p in Players)
            // {
            //     if (Vector3.Distance(p.Character.Position, deathCoords) < 50f)
            //         p.TriggerEvent("spawnBulletTokenPickup", player.Name, pickupId, bulletAmnt, deathCoords);
            // }
            
            Debug.WriteLine($"Player {player.Name}({playerHandleDrop}) Dropped {bulletAmnt} $BULLET tokens _ at: ({deathCoords})");
        }

        private void OnPlayerPickedUpAmmo([FromSource] Player player, int pickupId)
        {   
            int playerHandlePickup = int.Parse(player.Handle);

            // validate not already picked up
            if (PICKUPS[pickupId].Item5 == true) { 
                Debug.WriteLine($"ERROR: Player {player.Name}({playerHandlePickup}) tried pick-up on id: {pickupId}; $BULLET token already picked-up. returning");
                return;
            }
            
            // calc new reserve amount
            int oldReserve = PLAYER_RESERVES.ContainsKey(playerHandlePickup) ? PLAYER_RESERVES[playerHandlePickup] : 0;
            int bullAmntPickup = PICKUPS[pickupId].Item1;
            int newReserve = oldReserve + bullAmntPickup;

            // set pickup player new reserve amount
            PLAYER_RESERVES[playerHandlePickup] = newReserve;

            // update pickupId to picked-up = true (w/ playerHandlePickup)
            PICKUPS[pickupId] = Tuple.Create(PICKUPS[pickupId].Item1, PICKUPS[pickupId].Item2, PICKUPS[pickupId].Item3, playerHandlePickup, true);

            // trigger pickup client to update their reserve amount
            player.TriggerEvent("updateAmmoReserve", PLAYER_RESERVES[playerHandlePickup]);

            // Notify ALL clients to remove the $BULLET token pickup
            TriggerClientEvent("removeBulletTokenPickup", player.Name, pickupId);

            Debug.WriteLine($"Player {player.Name}({playerHandlePickup}) Picked-up {bullAmntPickup} $BULLET tokens _ Reserve (Old->New): {oldReserve} -> {newReserve}");
        }
        private void OnRequestPickupSync([FromSource] Player player)
        {
            foreach (var kvp in PICKUPS)
            {
                int pickupId = kvp.Key;
                bool isPickedUp = PICKUPS[pickupId].Item5;
                if (!isPickedUp) { 
                    int bulletAmnt = PICKUPS[pickupId].Item1;
                    Vector3 deathCoords = PICKUPS[pickupId].Item3;
                    player.TriggerEvent("spawnBulletTokenPickup", player.Name, pickupId, bulletAmnt, deathCoords);
                    // Debug.WriteLine($"Sent $BULLET token pickup-sync to client: Player {player.Name}({int.Parse(player.Handle)}) _ pickupId: {pickupId} _ $BULLET token amount: {bulletAmnt} _ coords: {deathCoords}");
                }
            }
        }
    }
}