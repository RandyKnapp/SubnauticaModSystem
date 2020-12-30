using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;

namespace QuitToDesktop.Configuration
{
    [Menu("Quit To Desktop", LoadOn = MenuAttribute.LoadEvents.MenuOpened | MenuAttribute.LoadEvents.MenuRegistered)]
    public class Config : ConfigFile
    {
        [Toggle("Show Confirmation Dialog (restart required)")]
        public bool ShowConfirmationDialog = true;
    }
}
