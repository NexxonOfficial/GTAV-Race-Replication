using CitizenFX.Core;
using CitizenFX.Core.Native;
using environment.Server;
using FxEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using yep.Shared.Schema;

namespace yep.Server
{
    internal class Race : VirtualEvent
    {
        internal RaceSettings Settings;
        internal int LobbyCountdown = 3;
        internal int SelectionCountdown = 10;
        internal Dictionary<LobbyPlayer, string> PlayerSelections;
        public Race(int maxPlayerCount, string raceName, string raceType, int lapCount, string raceClass, string timeOfDay, string weather, bool traffic, bool customVehicles, bool catchup, bool slipstream, string cameraLock) : base(maxPlayerCount, 1)
        {
            InitiateEvents();
            Initiate();
            Settings = new(raceName, raceType, lapCount, raceClass, timeOfDay, weather, traffic, customVehicles, catchup, slipstream, cameraLock);
            AssignRaceInformation();
        }

        void AssignRaceInformation()
        {
            string resourceData = API.LoadResourceFile("yep", "Server/Data/race.data.json");
            List<RaceData> parsedData = JsonConvert.DeserializeObject<List<RaceData>>(resourceData);
            foreach (RaceData raceData in parsedData)
            {
                if (raceData.RaceName == this.Settings.RaceName)
                {
                    this.Settings.RaceDesc = raceData.RaceDesc;
                    this.Settings.RaceCreator = raceData.RaceCreator;
                    this.Settings.RaceImage = raceData.RaceImage;
                    this.Settings.RaceInfoType = raceData.RaceType;
                    this.Settings.SelectionCoords = new(raceData.RaceSelection.x, raceData.RaceSelection.y, raceData.RaceSelection.z, raceData.RaceSelection.h);
                    this.Settings.RacePreviewCamera = new(raceData.RacePreviewCamera.x, raceData.RacePreviewCamera.y, raceData.RacePreviewCamera.z, raceData.RacePreviewCamera.h);
                }
            }
        }

        void InitiateEvents()
        {
            EventDispatcher.Mount("Race:Server:QueueRequest", new Action<Player>(RequestQueue));
            EventDispatcher.Mount("Race:Server:PlayerConnected", new Action<Player>(UserConnected));
            EventDispatcher.Mount("Race:Server:PlayerReady", new Action<Player>(PlayerReady));
            EventDispatcher.Mount("Race:Server:PlayerLeave", new Action<Player>(PlayerLeaveRequest));
            EventDispatcher.Mount("Race:Server:SelectionMade", new Action<Player, string>(PlayerSelected));
        }

        void Initiate()
        {
            foreach (Player player in ServerMain.Instance.AllPlayers)
            {
                Debug.WriteLine("Sent race initiation to: " + player.Name);
                EventDispatcher.Send(player, "Race:Client:Initiation");
            }
        }

        async void RequestQueue([FromSource] Player source)
        {
            LobbyPlayer queueRequest = await this.AttemptQueue(source);
            if (queueRequest is not null)
            {
                Dictionary<LobbyPlayer, bool> initPlayers = new();
                foreach (KeyValuePair<Player, LobbyPlayer> player in this.Players)
                {
                    if (player.Key != source)
                    {
                        initPlayers.Add(player.Value, player.Value.IsReady);
                    }
                    EventDispatcher.Send(player.Key, "Race:Client:PlayerQueued", queueRequest);
                }

                EventDispatcher.Send(source, "Race:Client:Queued", this.Settings, initPlayers);
            }
            else
            {
                EventDispatcher.Send(source, "Race:Client:Initiation");
                return;
            }
        }

        void UserConnected([FromSource] Player source)
        {
            this.Players.TryGetValue(source, out LobbyPlayer player);
            if (player != null)
            {
                foreach (KeyValuePair<Player, LobbyPlayer> lobbyPlayer in this.Players)
                {
                    Debug.WriteLine("Event sent to: " + lobbyPlayer.Key.Name);
                    EventDispatcher.Send(lobbyPlayer.Key, "Race:Client:UpdateStateForPlayer", player.PlayerName, 1);
                }
            }
        }

        async void PlayerReady([FromSource] Player source)
        {
            KeyValuePair<string, LobbyPlayer> isReady = await this.ReadyPlayer(source);
            if (this.IsLobbyReady && isReady.Key == "unready")
            {
                ServerMain.Instance.DetachTick(StartLobbyCountdown);
                this.IsLobbyReady = false;
                this.LobbyCountdown = 60;
            }
            else if (this.IsLobbyReady && isReady.Key == "ready")
            {
                int readyPlayers = CheckPlayersReady();

                if (readyPlayers == this.Players.Count)
                {
                    ServerMain.Instance.AttachTick(StartLobbyCountdown);
                    this.IsLobbyReady = true;
                }
            }

            if (isReady.Key == "ready")
            {
                foreach (KeyValuePair<Player, LobbyPlayer> lobbyPlayer in this.Players)
                {
                    EventDispatcher.Send(lobbyPlayer.Key, "Race:Client:UpdateStateForPlayer", isReady.Value.PlayerName, 3);
                }
            }
            else if (isReady.Key == "unready")
            {
                foreach (KeyValuePair<Player, LobbyPlayer> lobbyPlayer in this.Players)
                {
                    EventDispatcher.Send(lobbyPlayer.Key, "Race:Client:UpdateStateForPlayer", isReady.Value.PlayerName, 2);
                }
            }
        }

        async void PlayerLeaveRequest([FromSource] Player source)
        {
            await this.AttemptDequeue(source);

            if (this.IsLobbyReady)
            {
                this.IsLobbyReady = false;
                this.LobbyCountdown = 60;
                ServerMain.Instance.DetachTick(StartLobbyCountdown);
            }

            foreach (KeyValuePair<Player, LobbyPlayer> lobbyPlayer in this.Players)
            {
                EventDispatcher.Send(lobbyPlayer.Key, "Race:Client:UpdateStateForPlayer", source.Name, 4);
            }
        }

        internal async Task StartLobbyCountdown()
        {
            int readyPlayers = this.CheckPlayersReady();

            if (readyPlayers == this.Players.Count && this.IsLobbyReady)
            {
                foreach (KeyValuePair<Player, LobbyPlayer> lobbyPlayer in this.Players)
                {
                    EventDispatcher.Send(lobbyPlayer.Key, "VirtualEvent:Client:TimerUpdate", this.LobbyCountdown, 1);
                }
            }

            await BaseScript.Delay(1000);

            if (this.LobbyCountdown <= 0)
            {
                ServerMain.Instance.DetachTick(StartLobbyCountdown);
                foreach (KeyValuePair<Player, LobbyPlayer> player in this.Players)
                {
                    EventDispatcher.Send(player.Key, "VirtualEvent:Client:InitialiseSession");
                    EventDispatcher.Send(player.Key, "Race:Client:InitialiseSession");
                    await BaseScript.Delay(1000);
                    ServerMain.Instance.AttachTick(StartSelectionCountdown);
                }
                this.PlayerSelections = new();
                return;
            }

            this.LobbyCountdown--;
        }

        internal async Task StartSelectionCountdown()
        {
            foreach (KeyValuePair<Player, LobbyPlayer> lobbyPlayer in this.Players)
            {
                EventDispatcher.Send(lobbyPlayer.Key, "VirtualEvent:Client:TimerUpdate", this.SelectionCountdown, 2);
            }

            await BaseScript.Delay(1000);

            if (this.SelectionCountdown <= 0)
            {
                ServerMain.Instance.DetachTick(StartSelectionCountdown);
                switch (this.EventType)
                {
                    case 1:
                        foreach (KeyValuePair<Player, LobbyPlayer> player in this.Players)
                        {
                            EventDispatcher.Send(player.Key, "Race:Client:Initialise");
                            if (!this.PlayerSelections.ContainsKey(player.Value))
                            {
                                EventDispatcher.Send(player.Key, "Race:Client:RetrieveHoveringVehicle");
                            }
                        }
                        await BaseScript.Delay(500);
                        break;
                }
                return;
            }

            this.SelectionCountdown--;
        }

        internal void PlayerSelectionPlacement(Player player, string spawnName)
        {
            this.Players.TryGetValue(player, out LobbyPlayer found);
            if (found != null)
            {
                this.PlayerSelections.Add(found, spawnName);
                Debug.WriteLine("Placed player: " + player.Name + "'s vehicle: " + spawnName + " in Selections.");
            }
            else
            {
                EventDispatcher.Send(player, "VirtualEvent:Client:Error", "Sorry, but it seems you were not in the race to ready up!\nIf this is wrong, please contact a member of staff.");
            }
        }

        void PlayerSelected([FromSource] Player source, string spawnName)
        {
            this.PlayerSelectionPlacement(source, spawnName);
        }

        internal void GridPlacements(Dictionary<LobbyPlayer, string> players)
        {
            List<int> availablePositions = new();
            for (int i = 1; i < players.Count; i++)
            {
                availablePositions.Add(i);
            }

            Dictionary<int, LobbyPlayer> selectedPositions = new();
            Random ourRandom = new Random();

            foreach (KeyValuePair<LobbyPlayer, string> player in players)
            {
                int randomValue = ourRandom.Next(availablePositions.Count);
                int gridPos = availablePositions[randomValue];

                selectedPositions.Add(gridPos, player.Key);
            }

            CreateGrid();
        }

        internal async void CreateGrid(Vector4 init)
        {
            init.W = init.W + 70f;
            Vehicle baseVehicle = await World.CreateVehicle(new("adder"), new(init.X, init.Y, init.Z), init.W);
            baseVehicle.IsPositionFrozen = true;
            Dictionary<int, Vector4> Positions = new();
            int placement = 2;
            int row = 1;
            for (int i = 2; i < 31; i++)
            {
                Vector4 final = new();
                if (placement == 1)
                {
                    Vector3 offset = baseVehicle.GetOffsetPosition(new(0f, (-10f * (row - 1)) + -2f, 0f));
                    final = new(offset.X, offset.Y, init.Z, init.W);
                    Positions.Add(i, final);
                    placement++;
                }
                else if (placement == 2)
                {
                    Vector3 offset = baseVehicle.GetOffsetPosition(new(5f, (-10f * row) + 3f, 0f));
                    final = new(offset.X, offset.Y, init.Z, init.W);
                    Positions.Add(i, final);
                    placement++;
                }
                else if (placement == 3)
                {
                    Vector3 offset = baseVehicle.GetOffsetPosition(new(-5f, (-10f * row) + 3f, 0f));
                    final = new(offset.X, offset.Y, init.Z, init.W);
                    Positions.Add(i, final);
                    placement = 1;
                    row++;
                }
            }
        }

        void Start()
        {

        }
    }
}
