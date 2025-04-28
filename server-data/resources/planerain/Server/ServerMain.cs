using System;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace planerain.Server

{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            Debug.WriteLine("Hi from plainrain.Server!");
            EventHandlers["playerSpawned"] += new Action<Player>(OnPlayerSpawned);

            EventHandlers["startrain"] += new Action<Player>(StartRainCommand);
            EventHandlers["stoprain"] += new Action<Player>(StopRainCommand);
        }

        [Command("hello_server")]
        public void HelloServer()
        {
            Debug.WriteLine("Sure, hello.");
        }
        [Command("startrain1")]
        public void StartRainCommand([FromSource] Player player)
        {
            // Trigger client event for the player who issued the command
            // Player player = Players[source];
            // player.TriggerEvent("plainrain:startRain");
            Debug.WriteLine($"{player.Name} started raining planes!");
        }

        [Command("stoprain1")]
        public void StopRainCommand([FromSource] Player player)
        {
            // Trigger client event for the player who issued the command
            // Player player = Players[source];
            // player.TriggerEvent("plainrain:stopRain");
            Debug.WriteLine($"{player.Name} stopped raining planes!");
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
    }
}