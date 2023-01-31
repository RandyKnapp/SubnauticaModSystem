#if SN
using Oculus.Newtonsoft.Json;
#elif BZ
using Newtonsoft.Json;
#endif

namespace AutosortLockers
{
    [JsonObject]
    public class Config
    {
        public bool ShowLabel { get; set; } = false;
        public bool EasyBuild { get; set; } = true;
        public float SortInterval { get; set; } = 1.0f;
        public int AutosorterWidth { get; set; } = 5;
        public int AutosorterHeight { get; set; } = 6;
        public int ReceptacleWidth { get; set; } = 6;
        public int ReceptacleHeight { get; set; } = 8;
        public int StandingReceptacleWidth { get; set; } = 6;
        public int StandingReceptacleHeight { get; set; } = 8;
#if SN
        public char GameVersion { get; set; } = '1';
#elif BZ
        public char GameVersion { get; set; } = '2';
#endif
    }
}