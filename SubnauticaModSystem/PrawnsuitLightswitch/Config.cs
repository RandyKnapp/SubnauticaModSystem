using System;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;

namespace PrawnsuitLightswitch
{
    [Menu("Prawnsuit Lightswitch")]
    public class Config : ConfigFile
    {
        [Toggle("Lights Use Energy")]
        public bool PrawnsuitLightsUseEnergy = true;
    }
}
