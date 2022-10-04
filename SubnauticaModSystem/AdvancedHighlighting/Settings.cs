using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if SN
#elif BZ
#endif

namespace AdvancedHighlighting
{
  public class Settings
  {
    public static readonly string defaultColor = "e0e238";
    public static readonly Color nullColor = new Color(0f, 0f, 0f, 0f);
    [JsonIgnore]
    public bool PresetApplied = false;

    public bool StoryNotification = false;
    public bool DisksNotification = false;
    public bool ShowPings = false;
    public bool ExosuiteHighlightsEnabled = false;
    public bool DestroyNullPings = true;
    public float HighlightSearchDistance = 20f;
    public float MinimumAlpha = 0f;
    public int HighlightingRenderer = 1;

    public Dictionary<Highlight, bool> HighlightsEnabled;
    public Dictionary<Highlight, string> HighlightsColor;

    [JsonIgnore]
    public List<Highlight> ActiveHighlights = new List<Highlight>();

    [JsonIgnore]
    private static Settings instance;
    public static Settings GetInstance()
    {
      if (instance == null)
        instance = OldSettings.Load();
      return instance;
    }
    public void Save()
    {
      Console.WriteLine("[AdvancedHighlighting] Saving Settings");
      File.WriteAllText(Path.Combine(Utils.ModDirectory, "settings.json"), JsonConvert.SerializeObject(this, Formatting.Indented));
    }
    public void UpdateActiveHighlights()
    {
      ActiveHighlights.Clear();
      if (MiscSettings.highlighting)
      {
        foreach (var highlight in HighlightsEnabled)
          if (highlight.Value)
            ActiveHighlights.Add(highlight.Key);
        //Console.WriteLine($"[AdvancedHighlighting] UpdateActiveHighlights {string.Join(",", ActiveHighlights)}...");
      }
    }

    public bool IsHighlightActive(Highlight highlight) =>
        ActiveHighlights.Contains(highlight);
    public bool IsHighlightEnabled(Highlight highlight) =>
        (HighlightsEnabled.TryGetValue(highlight, out bool e) ? e : MiscSettings.highlighting);
    public Color GetHighlightColor(Highlight highlight) =>
        (HighlightsColor.TryGetValue(highlight, out string h) && ColorUtility.TryParseHtmlString("#" + h, out Color c) ? c : nullColor);
    public string GetHtmlColor(Highlight highlight) =>
        (HighlightsColor.TryGetValue(highlight, out string h) ? h : null);
  }
}