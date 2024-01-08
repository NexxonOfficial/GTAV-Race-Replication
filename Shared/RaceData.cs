namespace yep.Shared.Schema
{
    public class RaceData
    {
        public string RaceName;
        public string RaceDesc;
        public string RaceImage;
        public string RaceCreator;
        public string RaceType;
        public RaceSelection RaceSelection;
        public RacePreviewCamera RacePreviewCamera;
    }

    public class RaceSelection
    {
        public float x;
        public float y;
        public float z;
        public float h;
    }

    public class RacePreviewCamera
    {
        public float x;
        public float y;
        public float z;
        public float h;
    }
}
