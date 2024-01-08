using ScaleformUI.Scaleforms;

namespace yep.Client
{
    public class VirtualEvent
    {
        public VirtualEvent()
        {
            InitiateEvents();
        }

        void InitiateEvents()
        {
            EventDispatcher.Mount("VirtualEvent:Client:TimerUpdate", new Action<int, int>(UpdateCountdownScaleform));
            EventDispatcher.Mount("VirtualEvent:Client:TimerCancelled", new Action(LobbyTimerCancelled));
            EventDispatcher.Mount("VirtualEvent:Client:Error", new Action<string>(Error));
            EventDispatcher.Mount("VirtualEvent:Client:InitialiseSession", new Action(InitialiseSession));
        }

        internal void Error(string msg)
        {
            List<InstructionalButton> buttons = new()
            {
                new(Control.PhoneSelect, Control.PhoneSelect, "Acknowledge")
            };
            ScaleformUI.Main.Warning.ShowWarningWithButtons("Error", msg, "", buttons);
        }

        void UpdateCountdownScaleform(int number, int type)
        {
            switch (type)
            {
                case 1:
                    ScaleformUI.Main.InstructionalButtons.AddSavingText(CitizenFX.Core.UI.LoadingSpinnerType.Clockwise1, "Server Is Host (" + (number == 60 ? "1:00" : number < 10 ? "0:0" + number : "0:" + number) + ")");
                    break;
                case 2:
                    ScaleformUI.Main.InstructionalButtons.AddSavingText(CitizenFX.Core.UI.LoadingSpinnerType.Clockwise1, "Launching Job (" + (number == 60 ? "1:00" : number < 10 ? "0:0" + number : "0:" + number) + ")");
                    if (number == 0)
                    {
                        API.PlaySoundFrontend(-1, "TIMER_STOP", "HUD_MINI_GAME_SOUNDSET", true);
                    }
                    else if (number < 6)
                    {
                        API.PlaySoundFrontend(-1, "MP_5_SECOND_TIMER", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
                    }
                    break;
            }
        }

        void LobbyTimerCancelled()
        {
            ScaleformUI.Main.InstructionalButtons.HideSavingText();
        }

        void InitialiseSession()
        {
            ScaleformUI.Main.InstructionalButtons.ClearButtonList();
            ScaleformUI.Main.InstructionalButtons.AddSavingText(CitizenFX.Core.UI.LoadingSpinnerType.Clockwise2, "Launching Session");
        }
    }
}
