using CitizenFX.Core;
using System.Collections.Generic;

namespace yep.Shared.Schema
{
    public class RaceSettings
    {
        public string RaceName;
        public string RaceDesc;
        public string RaceCreator;
        public string RaceImage;
        public string RaceInfoType;
        public string RaceType;
        public int LapCount;
        public string RaceClass;
        public string TimeOfDay;
        public string Weather;
        public bool Traffic;
        public bool CustomVehicles;
        public bool Catchup;
        public bool Slipstream;
        public string CameraLock;

        public Vector4 SelectionCoords;
        public Vector4 RacePreviewCamera;
        public Dictionary<int, Vector4> RaceGridPositions;

        public object[] VehicleList;
        public RaceSettings() { }
        internal RaceSettings(string raceName, string raceType, int lapCount, string raceClass, string timeOfDay, string weather, bool traffic, bool customVehicles, bool catchup, bool slipstream, string cameraLock)
        {
            RaceName = raceName;
            RaceType = raceType;
            LapCount = lapCount;
            RaceClass = raceClass;
            TimeOfDay = timeOfDay;
            Weather = weather;
            Traffic = traffic;
            CustomVehicles = customVehicles;
            Catchup = catchup;
            Slipstream = slipstream;
            CameraLock = cameraLock;
        }
    }
}
