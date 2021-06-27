using Newtonsoft.Json;

namespace AutosortLockers
{
    [JsonObject]
    public class Config
    {
        public bool ShowLabel { get; set; } = false;
        public bool EasyBuild { get; set; } = false;
        public float SortInterval { get; set; } = 1.0f;
        public int AutosorterWidth { get; set; } = 5;
        public int AutosorterHeight { get; set; } = 6;
        public int ReceptacleWidth { get; set; } = 6;
        public int ReceptacleHeight { get; set; } = 8;
        public int StandingReceptacleWidth { get; set; } = 6;
        public int StandingReceptacleHeight { get; set; } = 8;
#if SUBNAUTICA
        public char GameVersion { get; set; } = '1';
#elif BELOWZERO
        public char GameVersion { get; set; } = '2';
#endif
    }
}