using System;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace house.Server
{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            Debug.WriteLine("Hi from house.Server!");

            EventHandlers["playerSpawned"] += new Action<Player>(OnPlayerSpawned);
            EventHandlers["playerQuit"] += new Action<Player>(OnPlayerQuit);
            EventHandlers["playerCoords"] += new Action<Player, Vector3>(OnPlayerCoords);
        }

        [Command("hello_server")]
        public void HelloServer()
        {
            Debug.WriteLine("Sure, hello.");
        }
        private void OnPlayerCoords([FromSource] Player player, Vector3 coords)
        {
            Debug.WriteLine($"Player {player.Name}({player.Handle}) _ current coords: {coords}");
        }
        private void OnPlayerSpawned([FromSource] Player player)
        {
            // int playerHandle = int.Parse(player.Handle);
            // if (!PLAYER_RESERVES.ContainsKey(playerHandle))
            // {
            //     PLAYER_RESERVES[playerHandle] = 0; // Initialize reserve ammo
            // }
            // player.TriggerEvent("updateAmmoReserve", PLAYER_RESERVES[playerHandle]);
        }
        private void OnPlayerQuit([FromSource] Player player)
        {
            // Disconnect the player with a custom message
            int playerHandlePickup = int.Parse(player.Handle);
            player.Drop("You have disconnected using /quit.");
            Debug.WriteLine($"Player {player.Name}({playerHandlePickup}) has disconnected using /quit.");
        }
    }
}