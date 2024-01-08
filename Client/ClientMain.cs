namespace yep.Client
{
    public class ClientMain : BaseScript
    {
        internal static ClientMain Instance;
        public ClientMain()
        {
            Instance = this;
            EventDispatcher.Initalize("inbound", "outbound", "signature", "encryption");
            API.ShutdownLoadingScreenNui();
            Game.PlayerPed.Weapons.Give(WeaponHash.StunGun, 1000, true, true);
            new NetTest();
        }

        [Command("Coords")]
        public void Coords(int src, List<object> args, string raw)
        {
            Debug.WriteLine(Game.PlayerPed.Position.ToString());
        }

        [Tick]
        public Task OnTick()
        {
            return Task.FromResult(0);
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