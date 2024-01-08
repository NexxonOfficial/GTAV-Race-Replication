using CitizenFX.Core;

namespace yep.Shared.Schema
{
    public class LobbyPlayer
    {
        public string PlayerName;
        public int PlayerLevel;
        public int PlayerState = 1; //Connecting
        public bool IsReady;

        public Player Player;
        public LobbyPlayer() { }
        internal LobbyPlayer(string playerName, int playerLevel, Player player)
        {
            PlayerName = playerName;
            PlayerLevel = playerLevel;
            IsReady = false;
            this.Player = player;
        }
    }
}
