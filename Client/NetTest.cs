using CitizenFX.Core.UI;
using FxEvents.Shared.TypeExtensions;
using Newtonsoft.Json;
using ScaleformUI.LobbyMenu;
using ScaleformUI.Scaleforms;
using yep.Shared.Schema;

namespace yep.Client
{
    internal class NetTest : VirtualEvent
    {
        bool LobbyMenu = false;
        Dictionary<LobbyPlayer, FriendItem> LobbyPlayers = new();
        RaceSettings RaceSettings;
        private long txd;

        internal Vehicle SelectedVehicle;
        public NetTest()
        {
            API.SwitchToMultiSecondpart(Game.PlayerPed.Handle);
            API.TransitionFromBlurred(500);
            API.EnableAllControlActions(0);
            API.NetworkFadeInEntity(Game.PlayerPed.Handle, true);
            Screen.Fading.FadeIn(1);
            API.AnimpostfxStopAll();
            API.SetGamePaused(false);
            Game.PlayerPed.Position = new(0f, 0f, 71f);
            ClientMain.Instance.DetachTick(LobbyControlHandle);
            Game.PlayerPed.IsPositionFrozen = false;
            Game.Player.CanControlCharacter = true;
            ShowInstructionalButtons(false);
            InitiateEvents();
            //CreateLobbyMenu();


            API.ClearArea(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 100f, true, false, false, false);

            GridPositionFinder(new(34f, 6.2f, 70f, 0f));
            ///ClientMain.Instance.AttachTick(DrawGrid);
        }

        void InitiateEvents()
        {
            EventDispatcher.Mount("Race:Client:Initiation", new Action(Initiate));
            EventDispatcher.Mount("Race:Client:Queued", new Action<RaceSettings, Dictionary<LobbyPlayer, bool>>(Queued));
            EventDispatcher.Mount("Race:Client:PlayerQueued", new Action<LobbyPlayer>(AddPlayerToLobby));
            EventDispatcher.Mount("Race:Client:UpdateStateForPlayer", new Action<string, int>(UpdatePlayerItemState));
            EventDispatcher.Mount("Race:Client:InitialiseSession", new Action(InitialiseSession));
            EventDispatcher.Mount("Race:Client:Initialise", new Action(Initialise));
            EventDispatcher.Mount("Race:Client:RetrieveHoveringVehicle", new Action(ForceVehicleRetrieval));

            // NUI Callbacks
            API.RegisterNuiCallback("lobbyButtonPressed", new Action<IDictionary<string, object>, CallbackDelegate>((data, cb) =>
            {
                if (data.TryGetValue("buttonAction", out var actionObj))
                {
                    var action = (actionObj as string) ?? " ";
                    switch (action.ToInt())
                    {
                        //Ready Up
                        case 1:
                            EventDispatcher.Send("Race:Server:PlayerReady");
                            cb(new
                            {
                                action = 1
                            });
                            break;
                        case 2:
                            EventDispatcher.Send("Race:Server:SelectionMade", this.SelectedVehicle.DisplayName);
                            cb(new
                            {
                                action = 2
                            });
                            break;
                        default:
                            cb(new
                            {
                                action = 0
                            });
                            break;
                    }
                }
                else
                {
                    cb(new
                    {
                        error = "No action specified"
                    });
                }
            }));

            API.RegisterNuiCallback("lobbyListChange", new Action<IDictionary<string, object>, CallbackDelegate>(async (data, cb) =>
            {
                if (data.TryGetValue("value", out var valueObj))
                {
                    var value = (valueObj as string) ?? " ";

                    foreach (object selection in this.RaceSettings.VehicleList)
                    {
                        if (selection.GetType().GetProperty("ListName").GetValue(selection, null).ToString() == value)
                        {
                            this.SelectedVehicle.Delete();
                            Vehicle newVeh = await World.CreateVehicle(new(selection.GetType().GetProperty("SpawnName").GetValue(selection, null).ToString()), new(this.RaceSettings.SelectionCoords.X, this.RaceSettings.SelectionCoords.Y, this.RaceSettings.SelectionCoords.Z), this.RaceSettings.SelectionCoords.W);
                            this.SelectedVehicle = newVeh;
                            UpdateSelectionTextItemText(0, selection.GetType().GetProperty("BrandName").GetValue(selection, null).ToString(), selection.GetType().GetProperty("ModelName").GetValue(selection, null).ToString());
                            break;
                        }
                    }

                    cb(new
                    {
                        success = true
                    });
                }
                else
                {
                    cb(new
                    {
                        error = "No action specified"
                    });
                }
            }));

            Debug.WriteLine("Events for Race initiated successfully!");
        }

        internal void Initiate()
        {
            ClientMain.Instance.AttachTick(DrawRaceInitiationMarker);
        }

        internal async void Queued(RaceSettings settings, Dictionary<LobbyPlayer, bool> players)
        {
            this.RaceSettings = settings;
            API.DisableAllControlActions(0);
            Game.Player.CanControlCharacter = false;
            API.SwitchToMultiFirstpart(Game.PlayerPed.Handle, 0, 1);
            API.NetworkFadeOutEntity(Game.PlayerPed.Handle, false, true);
            CreateLobbyMenu(players);

            while (API.GetPlayerSwitchState() != 5)
            {
                await BaseScript.Delay(1);
            }

            Game.PlayerPed.Position = new(this.RaceSettings.RacePreviewCamera.X, this.RaceSettings.RacePreviewCamera.Y, -100f);
            Game.PlayerPed.IsPositionFrozen = true;

            API.SwitchToMultiSecondpart(Game.PlayerPed.Handle);
            Camera previewCam = World.CreateCamera(new(this.RaceSettings.RacePreviewCamera.X, this.RaceSettings.RacePreviewCamera.Y, this.RaceSettings.RacePreviewCamera.Z), new(0f, 0f, 0f), 60f);

            while (API.GetPlayerSwitchState() != 10)
            {
                await BaseScript.Delay(1);
            }

            World.RenderingCamera = previewCam;
            Screen.Hud.IsVisible = false;
            Screen.Hud.IsRadarVisible = false;
            API.TransitionToBlurred(500);
            LobbyMenuVisible(true);
            ClientMain.Instance.AttachTick(LobbyControlHandle);
            EventDispatcher.Send("Race:Server:PlayerConnected");
            ShowInstructionalButtons(true);
        }

        void CreateLobbyMenu(Dictionary<LobbyPlayer, bool> players)
        {
            CreateBase(1);

            CreateTitleAndSubtitle(this.RaceSettings.RaceName, this.RaceSettings.RaceDesc);

            CreateColumn("SETTINGS");
            CreateColumn("PLAYERS");
            CreateColumn("DETAILS");

            CreateDetailItem(0, "Ready Up", null, "Will ready you up for the race. This is cancelable, if you would like to delay the race", "rgb(0, 71, 133)", false, false, 1);
            CreateDetailItem(0, "Race Type", this.RaceSettings.RaceType, null, null, true, true);
            CreateDetailItem(0, "Number Of Laps", this.RaceSettings.LapCount.ToString());
            CreateDetailItem(0, "Vehicle Class", this.RaceSettings.RaceClass);
            CreateDetailItem(0, "Time Of Day", this.RaceSettings.TimeOfDay);
            CreateDetailItem(0, "Weather Conditions", this.RaceSettings.Weather);
            CreateDetailItem(0, "Traffic Enabled", this.RaceSettings.Traffic ? "Yes" : "No");
            CreateDetailItem(0, "Custom Vehicles", this.RaceSettings.CustomVehicles ? "Yes" : "No");
            CreateDetailItem(0, "Catchup Enabled", this.RaceSettings.Catchup ? "Yes" : "No");
            CreateDetailItem(0, "Slipstream Enabled", this.RaceSettings.Slipstream ? "Yes" : "No");
            CreateDetailItem(0, "Camera Lock", this.RaceSettings.CameraLock);

            CreateDetailImageItem(2, "./img/test.jpg", this.RaceSettings.RaceName);
            CreateDetailItem(2, "Created By", this.RaceSettings.RaceCreator, null, null, false, true);
            CreateDetailItem(2, "Race Type", this.RaceSettings.RaceInfoType, null, null, false, true);
            CreateDetailItem(2, "Distance", "TBC", null, null, false, true);

            foreach (KeyValuePair<LobbyPlayer, bool> player in players)
            {
                CreatePlayerItem(1, player.Key.PlayerName, player.Key.PlayerLevel, player.Value == true ? "READY" : "JOINED", player.Value == true ? "rgb(0, 71, 133)" : "rgb(33, 118, 37)", null);
            }

            this.LobbyMenu = true;
        }

        void ShowInstructionalButtons(bool toggle)
        {
            List<InstructionalButton> buttons = new()
            {
                new(Control.PhoneSelect, Control.PhoneSelect, "Select"),
                new(Control.PhoneCancel, Control.PhoneCancel, "Back")
            };

            if (toggle)
            {
                ScaleformUI.Main.InstructionalButtons.SetInstructionalButtons(buttons);
            }
            else
            {
                ScaleformUI.Main.InstructionalButtons.ClearButtonList();
                if (ScaleformUI.Main.InstructionalButtons.IsSaving)
                {
                    ScaleformUI.Main.InstructionalButtons.HideSavingText();
                }
            }
        }

        void CreateBase(int baseType)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "baseCreate",
                baseType = baseType
            }));
        }

        void CreateTitleAndSubtitle(string title, string subtitle)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "createTitleAndSubtitle",
                title = title,
                subtitle = subtitle
            }));
        }

        void CreateColumn(string headerName, int span = 1)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "createColumn",
                headername = headerName,
                span = span
            }));
        }

        void CreateDetailItem(int columnIndex, string leftText, string rightText = null, string description = null, string color = null, bool seperator = false, bool closeGap = false, int onClickFunction = 0)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "createDetailItem",
                column = columnIndex,
                leftText = leftText,
                rightText = rightText,
                description = description,
                color = color,
                seperator = seperator,
                closeGap = closeGap,
                onClickFunction = onClickFunction
            }));
        }

        void CreateListItem(int columnIndex, string leftText, string list = null, string description = null, string color = null, bool seperator = false, bool closeGap = false, int onClickFunction = 0)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "createListItem",
                column = columnIndex,
                leftText = leftText,
                list = list,
                description = description,
                color = color,
                seperator = seperator,
                closeGap = closeGap,
                onClickFunction = onClickFunction
            }));
        }

        void CreatePlayerItem(int columnIndex, string name, int level = 0, string statusText = null, string statusColor = null, string crewTag = null, string color = null, string onClickFunction = null)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "createPlayerItem",
                column = columnIndex,
                name = name,
                level = level,
                statusText = statusText,
                statusColor = statusColor,
                crewTag = crewTag,
                color = color,
                onClickFunction = onClickFunction
            }));
        }

        void CreateSelectionTextItem(int columnIndex, string topText, string bottomText, bool isSelected)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "createSelectionTextItem",
                column = columnIndex,
                topText = topText,
                bottomText = bottomText,
                isSelected = isSelected
            }));
        }

        void CreateDetailImageItem(int columnIndex, string imgSource, string imgTitle = null)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "createDetailImageItem",
                column = columnIndex,
                imgSource = imgSource,
                imgTitle = imgTitle
            }));
        }

        void UpdatePlayerStatus(string playerName, string statusText, string statusColor = null, bool useAudio = false)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "updatePlayerStatus",
                playerName = playerName,
                statusText = statusText,
                statusColor = statusColor
            }));
            if (useAudio)
            {
                API.PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            }
        }

        void UpdateSelectionTextItemText(int columnIndex, string topText, string bottomText)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "updateSelectionTextItemText",
                column = columnIndex,
                topText = topText,
                bottomText = bottomText
            }));
        }

        void LobbyMenuVisible(bool useTransition)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "baseVisible",
                useTransition = useTransition
            }));
        }

        void LobbyMenuDispose(bool useTransition)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "baseDispose",
                useTransition = useTransition
            }));
            this.LobbyMenu = false;
        }

        async void AddPlayerToLobby(LobbyPlayer player)
        {
            while (!this.LobbyMenu)
            {
                await BaseScript.Delay(100);
            }

            CreatePlayerItem(1, player.PlayerName, player.PlayerLevel, "JOINING", "rgb(240, 160, 1)", null);
        }

        void UpdatePlayerItemState(string playerName, int state)
        {
            switch (state)
            {
                case 1:
                    //Init join, use audio noise
                    UpdatePlayerStatus(playerName, "JOINED", "rgb(33, 118, 37)", true);
                    break;
                case 2:
                    UpdatePlayerStatus(playerName, "JOINED", "rgb(33, 118, 37)");
                    break;
                case 3:
                    UpdatePlayerStatus(playerName, "READY", "rgb(0, 71, 133)");
                    break;
                case 4:
                    UpdatePlayerStatus(playerName, "LEFT", "rgb(207, 44, 48)");
                    break;
                default: break;
            }
        }

        internal async Task DrawRaceInitiationMarker()
        {
            Vector3 playerCoords = Game.PlayerPed.Position;
            Vector3 markerCoords = new(0f, 0f, 71f);
            API.DrawMarker(1, markerCoords.X, markerCoords.Y, markerCoords.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f * 1, 1.0f * 1, 1.0f * 1, 0, 128, 255, 200, false, true, 2, false, null, null, false);
            if (markerCoords.DistanceToSquared2D(playerCoords) <= 1f)
            {
                API.AddTextEntry("race_init_message", "Press ~INPUT_ENTER~ to join the race!");
                API.DisplayHelpTextThisFrame("race_init_message", true);
                if (API.IsControlJustPressed(0, (int)Control.Enter))
                {
                    EventDispatcher.Send("Race:Server:QueueRequest");
                    ClientMain.Instance.DetachTick(DrawRaceInitiationMarker);
                }
            }
            await Task.FromResult(0);
        }

        internal async Task LobbyControlHandle()
        {
            Dictionary<int, int> controlChecklist = new()
            { { 32, 172 }, { 33, 173 }, { 34, 174 }, { 35, 175 }, { 177, 9999 }, { 191, 9999 } };

            foreach (KeyValuePair<int, int> controls in controlChecklist)
            {
                if (API.IsDisabledControlJustPressed(0, controls.Key))
                {
                    API.SendNuiMessage(JsonConvert.SerializeObject(new
                    {
                        type = "controlpress",
                        control = controls.Key,
                        keyboard = API.IsUsingKeyboard(0)
                    }));

                    switch (controls.Key)
                    {
                        case 32:
                            API.PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                            break;
                        case 33:
                            API.PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                            break;
                        case 34:
                            API.PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                            break;
                        case 35:
                            API.PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                            break;
                        case 177:
                            LeaveRace();
                            API.PlaySoundFrontend(-1, "BACK", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                            break;
                        case 191:
                            API.PlaySoundFrontend(-1, "CONTINUE", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                            break;
                    }
                }
                else if (API.IsDisabledControlJustPressed(0, controls.Value))
                {
                    API.SendNuiMessage(JsonConvert.SerializeObject(new
                    {
                        type = "controlpress",
                        control = controls.Value,
                        keyboard = API.IsUsingKeyboard(0)
                    }));

                    switch (controls.Key)
                    {
                        case 172:
                            API.PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                            break;
                        case 173:
                            API.PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                            break;
                        case 174:
                            API.PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                            break;
                        case 175:
                            API.PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                            break;
                    }
                }
            }

            await Task.FromResult(0);
        }

        async void InitialiseSession()
        {
            API.NetworkFadeInEntity(Game.PlayerPed.Handle, false);
            await BaseScript.Delay(500);
            ClientMain.Instance.DetachTick(LobbyControlHandle);
            API.TransitionFromBlurred(1);
            LobbyMenuDispose(false);
            ShowInstructionalButtons(false);
            Vehicle vehicle = await World.CreateVehicle(new("pfister811"), new(this.RaceSettings.SelectionCoords.X, this.RaceSettings.SelectionCoords.Y, this.RaceSettings.SelectionCoords.Z), this.RaceSettings.SelectionCoords.W);
            API.PlaceObjectOnGroundProperly(vehicle.Handle);
            this.SelectedVehicle = vehicle;
            Vector3 playerPosition = vehicle.GetOffsetPosition(new(-1.5f, -1f, 0f));
            Game.PlayerPed.Position = playerPosition;
            Game.PlayerPed.Heading = vehicle.Heading;

            Vector3 camPosition = Game.PlayerPed.GetOffsetPosition(new(-1.4f, 4.7f, 0f));
            Camera cam = World.CreateCamera(camPosition, new(0f, 0f, 0f), 45f);
            cam.PointAt(Game.PlayerPed, new(3f, 0f, -0.5f));

            World.RenderingCamera = cam;

            ClientMain.Instance.AttachTick(LobbyControlHandle);
            await BaseScript.Delay(100);
            CreateVehicleSelectionScreen();
        }

        void CreateVehicleSelectionScreen()
        {
            CreateBase(1);

            CreateTitleAndSubtitle(this.RaceSettings.RaceName, this.RaceSettings.RaceDesc);

            CreateColumn("OPTIONS");
            CreateColumn("VEHICLE", 2);

            object[] newList = [
                new
                {
                    ListName = "Custom 811 Spyder",
                    BrandName = "Pfister",
                    ModelName = "811",
                    SpawnName = "pfister811"
                },
                new
                {
                    ListName = "T20",
                    BrandName = "Progen",
                    ModelName = "T20",
                    SpawnName = "T20"
                },
                new
                {
                    ListName = "X80 Proto",
                    BrandName = "Grotti",
                    ModelName = "X80 Proto",
                    SpawnName = "prototipo"
                },
                new
                {
                    ListName = "Reaper",
                    BrandName = "Pegassi",
                    ModelName = "Reaper",
                    SpawnName = "reaper"
                }
            ];

            this.RaceSettings.VehicleList = newList;

            CreateListItem(0, "Vehicle", JsonConvert.SerializeObject(newList), null, null, false, false, 2);
            CreateDetailItem(0, "Confirm", null, "Confirm the use of this vehicle. You cannot back out once selection is locked in.", "rgb(0, 71, 133)", false, false, 2);
            CreateSelectionTextItem(0, "Pfister", "811", false);

            this.LobbyMenu = true;

            LobbyMenuVisible(true);
            ShowInstructionalButtons(true);
        }

        async void LeaveRace()
        {
            EventDispatcher.Send("Race:Server:PlayerLeave");
            API.SwitchToMultiFirstpart(Game.PlayerPed.Handle, 0, 1);
            API.TransitionFromBlurred(500);
            LobbyMenuDispose(true);
            ClientMain.Instance.DetachTick(LobbyControlHandle);
            ShowInstructionalButtons(false);
            while (API.GetPlayerSwitchState() != 5)
            {
                await BaseScript.Delay(1);
            }
            World.RenderingCamera = null;
            Game.PlayerPed.Position = new(0f, 0f, 71f);
            Game.PlayerPed.IsPositionFrozen = false;
            API.SwitchToMultiSecondpart(Game.PlayerPed.Handle);
            API.NetworkFadeInEntity(Game.PlayerPed.Handle, true);
            while (API.GetPlayerSwitchState() != 12)
            {
                await BaseScript.Delay(1);
            }
            Screen.Hud.IsVisible = true;
            Screen.Hud.IsRadarVisible = true;
            ClientMain.Instance.AttachTick(DrawRaceInitiationMarker);
            API.EnableAllControlActions(0);
        }

        void Initialise()
        {
            LobbyMenuDispose(false);
            ShowInstructionalButtons(false);
            ClientMain.Instance.DetachTick(LobbyControlHandle);
            API.PlaySoundFrontend(-1, "CHECKPOINT_NORMAL", "HUD_MINI_GAME_SOUNDSET", true);
            API.AnimpostfxPlay("MenuMGSelectionTint", 1000, true);
            API.SetGamePaused(true);


            //this.SelectedVehicle.Delete();
        }

        void ForceVehicleRetrieval()
        {
            EventDispatcher.Send("Race:Server:SelectionMade", this.SelectedVehicle.DisplayName);
        }

        void Start()
        {

        }

        internal async Task DrawGrid()
        {
            foreach (KeyValuePair<int, Vector4> pos in this.RaceSettings.RaceGridPositions)
            {
                API.DrawSphere(pos.Value.X, pos.Value.Y, pos.Value.Z + 1f, 0.2f, 0, 0, 255, 0.5f);
            }

            await Task.FromResult(0);
        }
    }
}
