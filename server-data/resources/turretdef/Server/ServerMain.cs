using System;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace turretdef.Server
{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            Debug.WriteLine("Hi from turretdef.Server!");
            // RegisterCommands();
            EventHandlers["playerSpawned"] += new Action<Player>(OnPlayerSpawned);
        }
        [Command("hello_server")]
        public void HelloServer()
        {
            Debug.WriteLine("Sure, hello.");
        }
        // private void RegisterCommands()
        // {
        //     for (int i = 1; i <= 5; i++)
        //     {
        //         int level = i;
        //         float interval = i * 5.0f;                
        //         API.RegisterCommand($"level{level}", new Action<int, dynamic>((source, args) =>
        //         {
        //             TriggerClientEvent("setLevel", level, interval);
        //             TriggerClientEvent("chat:addMessage", new { color = new[] { 255, 0, 0 }, args = new[] { "[Level]", $"Set to level {level}" } });
        //         }), false);
        //     }
        // }
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