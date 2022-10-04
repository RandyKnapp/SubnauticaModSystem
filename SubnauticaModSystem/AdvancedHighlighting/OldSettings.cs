using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
#if SN
#elif BZ
#endif

namespace AdvancedHighlighting
{
  public class OldSettings
  {
    private static readonly Dictionary<Type, Highlight> TypeToHighlight = new Dictionary<Type, Highlight>
    {
      [typeof(Pickupable)] = Highlight.Pickupable,
      [typeof(ResourceTracker)] = Highlight.PickupableResource,
      [typeof(Creature)] = Highlight.PickupableCreatures,
      [typeof(Bladderfish)] = Highlight.Bladderfish,
      [typeof(CreatureEgg)] = Highlight.CreatureEgg,
      [typeof(BaseLadder)] = Highlight.BaseLadder,
      [typeof(BreakableResource)] = Highlight.BreakableResource,
      [typeof(HandTarget)] = Highlight.Interactables,
      [typeof(GrownPlant)] = Highlight.GrownPlant,
      [typeof(StoryHandTarget)] = Highlight.StoryItems,
      [typeof(JukeboxDisk)] = Highlight.JukeboxDisk,
      [typeof(StarshipDoor)] = Highlight.SealedDoor,
      [typeof(Sealed)] = Highlight.LaserCutObject,
      [Utils.FragmentsType] = Highlight.ScannableFragments,
      [Utils.ScannableType] = Highlight.ScannableItems,
      [Utils.EverythingType] = Highlight.AnythingElse,
    };

    public Dictionary<Type, bool> enableHighlights;
    public Dictionary<Type, string> highlightingColors;
    public bool storyNearbyNotification = false;
    public bool jukeboxDiskNotification = false;
    public bool highlightEverythingExosuit = false;
    public bool destoryNull = true;
    public bool showPings = false;
    public float highlightRadius = 20f;
    public float minAlpha = 0f;
    public static Settings Load()
    {
      Console.WriteLine("[AdvancedHighlighting] Loading Settings");
      string path = Path.Combine(Utils.ModDirectory, "settings.json");
      Settings settings = null;
      if (File.Exists(path))
      {
        try
        {
          var text = File.ReadAllText(path);
          if (text.Contains("PublicKeyToken") || text.Contains("storyNearbyNotification"))
            settings = JsonConvert.DeserializeObject<OldSettings>(text).ConvertToNewSettings();
          else
            settings = JsonConvert.DeserializeObject<Settings>(text);
        }
        catch (Exception) { }
      }
      if (settings == null) settings = new Settings();
      if (settings.HighlightsEnabled == null) settings.HighlightsEnabled = new Dictionary<Highlight, bool>();
      if (settings.HighlightsColor == null) settings.HighlightsColor = new Dictionary<Highlight, string>();
      foreach (var highlight in Utils.GetHighlights())
      {
        if (!settings.HighlightsEnabled.ContainsKey(highlight)) settings.HighlightsEnabled[highlight] = true;
        if (!settings.HighlightsColor.ContainsKey(highlight)) settings.HighlightsColor[highlight] = Settings.defaultColor;
      }
      settings.UpdateActiveHighlights();
      return settings;
    }
    public Settings ConvertToNewSettings()
    {
      var settings = new Settings
      {
        HighlightsEnabled = new Dictionary<Highlight, bool>(),
        HighlightsColor = new Dictionary<Highlight, string>(),
        StoryNotification = storyNearbyNotification,
        DisksNotification = jukeboxDiskNotification,
        ExosuiteHighlightsEnabled = highlightEverythingExosuit,
        DestroyNullPings = destoryNull,
        ShowPings = showPings,
        HighlightSearchDistance = highlightRadius,
        MinimumAlpha = minAlpha,
      };
      foreach (var highlight in enableHighlights)
        if (TypeToHighlight.TryGetValue(highlight.Key, out Highlight val))
          settings.HighlightsEnabled[val] = highlight.Value;
      foreach (var highlight in highlightingColors)
        if (TypeToHighlight.TryGetValue(highlight.Key, out Highlight val))
          settings.HighlightsColor[val] = highlight.Value;
      return settings;
    }
  }
}