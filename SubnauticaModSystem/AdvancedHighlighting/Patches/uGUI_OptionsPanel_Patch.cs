using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if SN
#elif BZ
#endif

namespace AdvancedHighlighting.Patches
{
  [HarmonyPatch(typeof(uGUI_OptionsPanel), "OnDisable")]
  public class uGUI_OptionsPanel_OnDisable_Patch
  {
    public static void Prefix()
    {
      Settings.GetInstance().Save();
    }
  }

  [HarmonyPatch(typeof(uGUI_OptionsPanel), "AddAccessibilityTab")]
  public class uGUI_OptionsPanel_AddAccessibilityTab_Patch
  {
    private static readonly Dictionary<Highlight, Toggle> toggleOptions = new Dictionary<Highlight, Toggle>();
    private static readonly Dictionary<Highlight, GameObject> colorOptions = new Dictionary<Highlight, GameObject>();
    private static readonly List<GameObject> options = new List<GameObject>();

    private static void UpdatePingColors()
    {
      foreach (var pingKey in Highlighting_OnUpdate_Patch.pings.Keys)
      {
        var ping = PingManager.Get(Highlighting_OnUpdate_Patch.pings[pingKey]);
        if (ping != null)
          ping.SetColor(1);
      }
    }
    public static bool Prefix(uGUI_OptionsPanel __instance)
    {
      var settings = Settings.GetInstance();
      int tabIndex = __instance.AddTab("Accessibility");
      options.Clear();
      toggleOptions.Clear();
      colorOptions.Clear();
      __instance.AddSliderOption(tabIndex, "UIScale", MiscSettings.GetUIScale(DisplayOperationMode.Current), 0.7f, 1.4f, 1f, 0.01f, (value) =>
     {
       MiscSettings.SetUIScale(value, DisplayOperationMode.Current);
     }, SliderLabelMode.Float, "0.00");
      __instance.AddToggleOption(tabIndex, "PDAPause", MiscSettings.pdaPause, new UnityAction<bool>((val) => MiscSettings.pdaPause = val), null);
      __instance.AddToggleOption(tabIndex, "Flashes", MiscSettings.flashes, new UnityAction<bool>((val) => MiscSettings.flashes = val), null);
      //highlightingColorOption = __instance.AddColorOption(tabIndex, "HighlightingColor", MiscSettings.highlightingColor, new UnityAction<Color>((val) => MiscSettings.highlightingColor = val ));

      __instance.AddHeading(tabIndex, "Advanced Highlighting");
      __instance.AddToggleOption(tabIndex, "Enable Highlighting", MiscSettings.highlighting, new UnityAction<bool>(OnHighlightingChanged), null);

      options.Add(__instance.AddChoiceOption(tabIndex, "Highlighting Renderer", Utils.Presets.Select(x => x.name).ToArray(), settings.HighlightingRenderer, new UnityAction<int>((val) =>
      {
        settings.HighlightingRenderer = val;
        settings.PresetApplied = false;
      }), null).gameObject);
      //__instance.AddToggleOption(tabIndex, "Destrey Nulls", settings.destoryNull, new UnityAction<bool>((val) => settings.destoryNull = val), null);
      options.Add(__instance.AddToggleOption(tabIndex, "Notify about nearby PDA Logs", settings.StoryNotification, new UnityAction<bool>((val) => settings.StoryNotification = val), null).gameObject);
      options.Add(__instance.AddToggleOption(tabIndex, "Notify about nearby Jukebox Disks", settings.DisksNotification, new UnityAction<bool>((val) => settings.DisksNotification = val), null).gameObject);
      options.Add(__instance.AddToggleOption(tabIndex, "Show pings for PDA Logs and Disks", settings.ShowPings, new UnityAction<bool>((val) =>
     {
       settings.ShowPings = val;
       TogglePings();
     }), null).gameObject);

      options.Add(__instance.AddToggleOption(tabIndex, "Highlight while in Exosuit", settings.ExosuiteHighlightsEnabled, new UnityAction<bool>((val) => settings.ExosuiteHighlightsEnabled = val), null).gameObject);
      options.Add(__instance.AddSliderOption(tabIndex, "Highlight search radius", settings.HighlightSearchDistance, 10f, 100f, 20f, 0.5f, (value) =>
      {
        settings.HighlightSearchDistance = value;
      }, SliderLabelMode.Float, "0.0"));
      options.Add(__instance.AddSliderOption(tabIndex, "Minimum allowed fade value", settings.MinimumAlpha, 0f, 1f, 0f, 0.1f, (value) =>
     {
       settings.MinimumAlpha = value;
     }, SliderLabelMode.Float, "0.0"));
      foreach (var highlight in Utils.GetHighlights())
      {
        toggleOptions[highlight] = __instance.AddToggleOption(tabIndex, $"Highlight {highlight.GetName()}", settings.IsHighlightEnabled(highlight), new UnityAction<bool>((val) =>
        {
          settings.HighlightsEnabled[highlight] = val;
          if (colorOptions.TryGetValue(highlight, out GameObject color))
            color.SetActive(val);
          settings.UpdateActiveHighlights();
        }), null);
        colorOptions[highlight] = __instance.AddColorOption(tabIndex, $"{highlight.GetName()} color", settings.GetHighlightColor(highlight), new UnityAction<Color>((val) =>
        {
          settings.HighlightsColor[highlight] = ColorUtility.ToHtmlStringRGBA(val);
          if ((highlight == Highlight.StoryItems || highlight == Highlight.JukeboxDisk) && settings.ShowPings)
            UpdatePingColors();
        }));
      }
      OnHighlightingChanged(MiscSettings.highlighting);
      return false;
    }
    private static void TogglePings()
    {
      if (!MiscSettings.highlighting || !Settings.GetInstance().ShowPings)
      {
        foreach (var pingKey in Highlighting_OnUpdate_Patch.pings.Keys)
        {
          var ping = PingManager.Get(Highlighting_OnUpdate_Patch.pings[pingKey]);
          if (ping != null)
            UnityEngine.Object.Destroy(ping);
        }
        Highlighting_OnUpdate_Patch.pings.Clear();
      }
      else
      {
        Highlighting_OnUpdate_Patch.stories.Clear();
      }
    }
    private static void OnHighlightingChanged(bool value)
    {
      MiscSettings.highlighting = value;
      TogglePings();
      foreach (var gameObject in options)
        if (gameObject != null)
          gameObject.SetActive(value);
      foreach (var highlight in Utils.GetHighlights())
      {
        toggleOptions[highlight].gameObject.SetActive(value);
        colorOptions[highlight].SetActive(value && toggleOptions[highlight].isOn);
      }
      Settings.GetInstance().UpdateActiveHighlights();
    }
  }
}