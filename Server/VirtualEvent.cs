using CitizenFX.Core;
using FxEvents;
using System.Collections.Generic;
using System.Threading.Tasks;
using yep.Shared.Schema;

namespace yep.Server
{
    public class VirtualEvent
    {
        internal Dictionary<Player, LobbyPlayer> Players;
        internal int PlayState;
        internal int MaxPlayerCount;
        internal bool IsLobbyReady;
        internal int EventType;
        public VirtualEvent(int maxPlayerCount, int eventType)
        {
            Players = new();
            PlayState = 0;
            MaxPlayerCount = maxPlayerCount;
            IsLobbyReady = false;
            EventType = eventType;
        }

        internal Task<LobbyPlayer> AttemptQueue(Player player)
        {
            if (this.Players.Count + 1 < this.MaxPlayerCount)
            {
                LobbyPlayer newPlayer = new(player.Name, 1, player);
                this.Players.Add(player, newPlayer);
                return Task.FromResult(newPlayer);
            }
            else
            {
                EventDispatcher.Send(player, "VirtualEvent:Client:Error", "This race is currently full!\nPlease try again when the player count decreases.");
                return Task.FromResult<LobbyPlayer>(null);
                //Server full error
            }
        }

        internal Task<bool> AttemptDequeue(Player player)
        {
            this.Players.Remove(player);
            if (this.IsLobbyReady)
            {
                foreach (KeyValuePair<Player, LobbyPlayer> lobbyPlayer in this.Players)
                {
                    EventDispatcher.Send(lobbyPlayer.Key, "VirtualEvent:Client:TimerCancelled");
                }
            }
            return Task.FromResult(true);
        }

        internal Task<KeyValuePair<string, LobbyPlayer>> ReadyPlayer(Player player)
        {
            this.Players.TryGetValue(player, out LobbyPlayer found);
            if (found != null)
            {
                if (found.IsReady)
                {
                    found.IsReady = false;
                    if (this.IsLobbyReady)
                    {
                        foreach (KeyValuePair<Player, LobbyPlayer> lobbyPlayer in this.Players)
                        {
                            EventDispatcher.Send(lobbyPlayer.Key, "VirtualEvent:Client:TimerCancelled");
                        }
                    }
                    return Task.FromResult<KeyValuePair<string, LobbyPlayer>>(new("unready", found));
                }
                else
                {
                    found.IsReady = true;

                    return Task.FromResult<KeyValuePair<string, LobbyPlayer>>(new("ready", found));
                }
            }
            else
            {
                EventDispatcher.Send(player, "VirtualEvent:Client:Error", "Sorry, but it seems you were not in the race to ready up!\nIf this is wrong, please contact a member of staff.");
                LeaveEvent(player);
                return Task.FromResult<KeyValuePair<string, LobbyPlayer>>(new());
            }
        }

        internal void LeaveEvent(Player source)
        {
            this.Players.TryGetValue(source, out LobbyPlayer found);
            if (found != null)
            {
                this.Players.Remove(source);
            }

            // Need to send event to put them back to the game
        }

        internal int CheckPlayersReady()
        {
            int readyPlayers = 0;
            foreach (KeyValuePair<Player, LobbyPlayer> readyCheckPlayer in this.Players)
            {
                if (readyCheckPlayer.Value.IsReady)
                {
                    readyPlayers++;
                }
            }

            return readyPlayers;
        }
    }
}
