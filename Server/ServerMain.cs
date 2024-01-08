using CitizenFX.Core;
using FxEvents;
using System;
using System.Threading.Tasks;
using yep.Server;

namespace environment.Server
{
    public class ServerMain : BaseScript
    {
        public PlayerList AllPlayers;
        public static ServerMain Instance;
        public ServerMain()
        {
            Debug.WriteLine("Hi from environment.Server!");
            EventDispatcher.Initalize("inbound", "outbound", "signature", "encryption");
            AllPlayers = Players;
            Instance = this;
            OnLoad();
        }

        async void OnLoad()
        {
            await BaseScript.Delay(2000);
            new Race(64, "Some Race", "Normal", 2, "Super", "Noon", "Sunny", true, true, true, true, "None");
        }

        [Command("hello_server")]
        public void HelloServer()
        {
            Debug.WriteLine("Sure, hello.");
        }

        internal void AttachTick(Func<Task> task)
        {
            Tick += task;
        }

        internal void DetachTick(Func<Task> task)
        {
            Tick -= task;
        }
    }
}