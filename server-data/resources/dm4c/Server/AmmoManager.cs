using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
// using System.Threading.Tasks;
// using System.Linq;

namespace DeathmatchServer
{
    // PICKUPS[pickupId] = new Tuple<int, int, Vector3, int, bool>(bulletAmnt, playerHandleDrop, deathCoords, -1, false);
    // public class BulletPickupInfo
    // {
    //     public int pickupId { get; set; }
    //     public int pickupHandle { get; set; }
    //     public int bulletAmnt { get; set; }
    //     public int playerHandleDrop { get; set; }
    //     public Vector3 deathCoords { get; set; }
    //     public int playerHandlePickup { get; set; }
    //     public bool isCollected { get; set; }
    // }
    public class AmmoManager : BaseScript
    {
        private readonly float DIST_CREATE_PICKUP_MAX = 175f; // max range to create pickup for player to see & grab
        private readonly float DIST_CREATE_PICKUP_MIN = 150f; // min range to create pickup for player to see & grab (prevent overloading creates)
            // grok: game core uses a streaming distance to despawn entities like pickups
            //  typically around 150.0f to 200.0f units (meters) from the player
        private readonly uint PICKUPHASH_MONEY = (uint)API.GetHashKey("PICKUP_MONEY_VARIABLE");
        // Player handle -> reserve ammo
        private Dictionary<int, int> PLAYER_RESERVES = new Dictionary<int, int>(); 
        private Dictionary<int, Tuple<int, int, Vector3, int, bool>> PICKUPS = new Dictionary<int, Tuple<int, int, Vector3, int, bool>>();
            //  <pickupId, <bulletAmnt, playerPedDrop, deathCoords, playerPedPickup, isPickedUp>>
        // private readonly List<BulletPickupInfo> PICKUPS = new List<BulletPickupInfo>();
        private int NEXT_PICKUP_ID = 1;
        public AmmoManager() // constructror
        {
            EventHandlers["playerSpawned"] += new Action<Player>(OnPlayerSpawned);
            EventHandlers["purchaseAmmo"] += new Action<Player, int>(OnPurchaseAmmo);
            EventHandlers["loadReserveAmmo"] += new Action<Player, int>(OnLoadReserveAmmo);
            EventHandlers["playerDiedDropAmmo"] += new Action<Player, int, Vector3>(OnPlayerDiedDropAmmo);
            EventHandlers["playerPickedUpAmmo"] += new Action<Player, int>(OnPlayerPickedUpAmmo);
            EventHandlers["requestPickupSync"] += new Action<Player>(OnRequestPickupSync);
            
            Tick += CreatePickups;
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
            
            // LEFT OFF HERE ... should only spawnBulletTokenPickup for nearby players
            // Notify only NEARBY clients to spawn the $BULLET TOKEN pickup
            // foreach (Player p in Players)
            // {
            //     if (Vector3.Distance(p.Character.Position, deathCoords) < 50f)
            //         p.TriggerEvent("spawnBulletTokenPickup", player.Name, pickupId, bulletAmnt, deathCoords);
            // }
            
            Debug.WriteLine($"Player {player.Name}({playerHandleDrop}) Dropped {bulletAmnt} $BULLET tokens _ at: ({deathCoords}) _ pickupId: {pickupId}");
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

            Debug.WriteLine($"Player {player.Name}({playerHandlePickup}) Picked-up id: {pickupId}, w/ {bullAmntPickup} $BULLET tokens _ Reserve (Old->New): {oldReserve} -> {newReserve}");
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
        // private const float CheckInterval = 5.0f; // Check every 5 seconds
        private async System.Threading.Tasks.Task CreatePickups()
        {
            if (PICKUPS.Count == 0) return;

            // float currentTime = API.GetGameTimer() / 1000.0f;
            
            foreach (var player in Players)
            {
                Vector3 playerPos = player.Character?.Position ?? Vector3.Zero;

                foreach (var kvp in PICKUPS)
                {
                    int pickupId = kvp.Key;
                    int bulletAmnt = PICKUPS[pickupId].Item1;
                    Vector3 deathCoords = PICKUPS[pickupId].Item3;
                    bool isPickedUp = PICKUPS[pickupId].Item5;
                    if (isPickedUp) continue;
                    // if (pickup.isCollected) continue;

                    // Check if the player is within MaxCreateDistance
                    float playerDist = Vector3.Distance(playerPos, deathCoords);
                    // bool IsInRange(float value, float min, float max) => value >= min && value <= max;
                    // if (IsInRange(playerDist, 100f, DIST_CREATE_PICKUP_MAX))
                    
                    if (playerDist > DIST_CREATE_PICKUP_MIN && playerDist <= DIST_CREATE_PICKUP_MAX)
                    { 
                        // trigger: OnSpawnBulletTokenPickup(String playerName, int pickupId, int bulletAmnt, Vector3 deathCoords)
                        player.TriggerEvent("spawnBulletTokenPickup", player.Name, pickupId, bulletAmnt, deathCoords);
                        Debug.WriteLine($"Sent spawn pickup request to: {player.Name} dist: {playerDist} _ from {deathCoords} _ pickupId: {pickupId} _ has {bulletAmnt} $BULLET tokens");

                        // Request all clients to create the pickup
                        // TriggerClientEvent("spawnBulletTokenPickup", player.Name, pickupId, PICKUPS[pickupId].Item1, PICKUPS[pickupId].Item3);
                        await Delay(1000); // Delay to prevent flooding the client
                    }
                }
            }

            // Clean up collected pickups
            // pickups.RemoveAll(p => p.Collected);

            await System.Threading.Tasks.Task.FromResult(1000);
        }
    }
}