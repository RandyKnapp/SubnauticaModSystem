using HighlightingSystem;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static PDAScanner;
#if SN
#elif BZ
#endif

namespace AdvancedHighlighting
{
  public static class Utils
  {
    public static readonly Type FragmentsType = typeof(TechFragment);
    public static readonly Type ScannableType = typeof(ScannerTarget);
    public static readonly Type EverythingType = typeof(Utils);
    public static readonly List<HighlightingPreset> Presets = new List<HighlightingPreset>
    {
      new HighlightingPreset { name = "Narrow", fillAlpha = 0f, downsampleFactor = 4, iterations = 2, blurMinSpread = 0.65f, blurSpread = 0.25f, blurIntensity = 0.3f, blurDirections = BlurDirections.Diagonal},
      new HighlightingPreset { name = "Wide (Game Default)", fillAlpha = 0f, downsampleFactor = 4, iterations = 4, blurMinSpread = 0.65f, blurSpread = 0.25f, blurIntensity = 0.3f, blurDirections = BlurDirections.Diagonal },
      new HighlightingPreset { name = "Strong", fillAlpha = 0f, downsampleFactor = 4, iterations = 2, blurMinSpread = 0.5f, blurSpread = 0.15f, blurIntensity = 0.325f, blurDirections = BlurDirections.Diagonal },
      new HighlightingPreset { name = "Speed", fillAlpha = 0f, downsampleFactor = 4, iterations = 1, blurMinSpread = 0.75f, blurSpread = 0f, blurIntensity = 0.35f, blurDirections = BlurDirections.Diagonal },
      new HighlightingPreset { name = "Quality", fillAlpha = 0f, downsampleFactor = 2, iterations = 3, blurMinSpread = 0.5f, blurSpread = 0.5f, blurIntensity = 0.28f, blurDirections = BlurDirections.Diagonal },
      new HighlightingPreset { name = "Solid 1px", fillAlpha = 0f, downsampleFactor = 1, iterations = 1, blurMinSpread = 1f, blurSpread = 0f, blurIntensity = 1f, blurDirections = BlurDirections.All },
      new HighlightingPreset { name = "Solid 2px", fillAlpha = 0f, downsampleFactor = 1, iterations = 2, blurMinSpread = 1f, blurSpread = 0f, blurIntensity = 1f, blurDirections = BlurDirections.All },
      new HighlightingPreset { name = "Solid 3px", fillAlpha = 0f, downsampleFactor = 1, iterations = 3, blurMinSpread = 1f, blurSpread = 0f, blurIntensity = 1f, blurDirections = BlurDirections.All },
      new HighlightingPreset { name = "Solid 4px", fillAlpha = 0f, downsampleFactor = 1, iterations = 4, blurMinSpread = 1f, blurSpread = 0f, blurIntensity = 1f, blurDirections = BlurDirections.All },
      new HighlightingPreset { name = "Solid 5px", fillAlpha = 0f, downsampleFactor = 1, iterations = 5, blurMinSpread = 1f, blurSpread = 0f, blurIntensity = 1f, blurDirections = BlurDirections.All }
    };
    public static Highlight[] GetHighlights() => (Highlight[])Enum.GetValues(typeof(Highlight));
    private static Sprite eyeSprite;
    public static bool GetEyeSprite(out Sprite sprite)
    {
      if (eyeSprite == null && File.Exists(Path.Combine(ModDirectory, "Assets", "Eye.png")))
        eyeSprite = ImageUtils.LoadSprite(Path.Combine(ModDirectory, "Assets", "Eye.png"), new Vector2(0.5f, 0.5f), TextureFormat.BC7, 100f, SpriteMeshType.Tight);
      sprite = eyeSprite;
      return (sprite != null);
    }
    public static bool GetColorForType(string type, out Color newColor)
    {
      if (type == "story")
      {
        newColor = Settings.GetInstance().GetHighlightColor(Highlight.StoryItems);
        return true;
      }
      else if (type == "disk")
      {
        newColor = Settings.GetInstance().GetHighlightColor(Highlight.JukeboxDisk);
        return true;
      }
      newColor = Settings.nullColor;
      return false;
    }
    public static List<Highlight> GetHighlightsForObject(this GameObject gameObject, out GameObject targetObject, out Behaviour behaviour)
    {
      var supported = new List<Highlight>();
      targetObject = null;
      behaviour = null;
      if (gameObject == null)
        return supported;
      if (Targeting.GetRoot(gameObject, out TechType techType, out GameObject result))
      {
        var scannableFragments = Settings.GetInstance().IsHighlightActive(Highlight.ScannableFragments);
        targetObject = result;
        if (PDAScanner.IsFragment(techType))
        {
          if (!PDAScanner.ContainsCompleteEntry(techType))
          {
            if (scannableFragments) supported.Add(Highlight.ScannableFragments);
          }
          else if (techType.GetEntryData(out EntryData data))
          {
            if (data.destroyAfterScan && Highlight.ScannedFragments.IsActive()) supported.Add(Highlight.ScannedFragments);
          }
        }
        else if (result != null)
        {
          if (techType.GetEntryData(out EntryData data) && PDAScanner.CanScan(result))
          {
            if (data.blueprint > 0 && scannableFragments)
              supported.Add(Highlight.ScannableFragments);
            else if (data.blueprint == 0 && Highlight.ScannableItems.IsActive())
              supported.Add(Highlight.ScannableItems);
          }
        }
      }
      behaviour = gameObject.GetComponentInParent<IHandTarget>() as Behaviour;
      if (behaviour != null)
      {
        if (Highlight.BreakableResource.IsPressent(behaviour))
        {
          if (Highlight.BreakableResource.IsActive()) supported.Add(Highlight.BreakableResource);
        }
        else if (Highlight.PickupableResource.IsPressent(behaviour))
        {
          if (Highlight.CreatureEgg.IsPressent(behaviour))
          {
            if (Highlight.CreatureEgg.IsActive()) supported.Add(Highlight.CreatureEgg);
          }
          else 
          {
            if (Highlight.PickupableResource.IsActive()) supported.Add(Highlight.PickupableResource);
          }
        }
        else if (Highlight.PickupableCreatures.IsPressent(behaviour))
        {
          if (Highlight.Bladderfish.IsPressent(behaviour))
          {
            if (Highlight.Bladderfish.IsActive()) supported.Add(Highlight.Bladderfish);
          }
          else
          {
            if (Highlight.PickupableCreatures.IsActive()) supported.Add(Highlight.PickupableCreatures);
          }
        }
        else if (Highlight.Pickupable.IsPressent(behaviour))
        {
          if (Highlight.Pickupable.IsActive()) supported.Add(Highlight.Pickupable);
        }
        else if (Highlight.Interactables.IsPressent(behaviour))
        {
          if(Highlight.BaseLadder.IsPressent(behaviour))
          {
            if (Highlight.BaseLadder.IsActive()) supported.Add(Highlight.BaseLadder);
          }
          else if (Highlight.GrownPlant.IsPressent(behaviour))
          {
            if (Highlight.GrownPlant.IsActive()) supported.Add(Highlight.GrownPlant);
          }
          else if (Highlight.SealedDoor.IsPressent(behaviour))
          {
            if (Highlight.SealedDoor.IsActive()) supported.Add(Highlight.SealedDoor);
          }
          else if (Highlight.LaserCutObject.IsPressent(behaviour))
          {
            if (Highlight.LaserCutObject.IsActive()) supported.Add(Highlight.LaserCutObject);
          }
          else
          {
            if (Highlight.Interactables.IsActive()) supported.Add(Highlight.Interactables);
          }
        }
        
        if (Highlight.StoryItems.IsPressentAndActive(behaviour)) supported.Add(Highlight.StoryItems);
        if (Highlight.JukeboxDisk.IsPressentAndActive(behaviour)) supported.Add(Highlight.JukeboxDisk);
        if (supported.Count == 0 && Highlight.AnythingElse.IsActive()) supported.Add(Highlight.AnythingElse);
      }
      //if (!logged.Contains(gameObject.GetInstanceID()))
      //{
      //    var types1 = gameObject.GetComponents<Component>().Select(x => x.GetType());
      //    var types2 = behaviour?.gameObject?.GetComponents<Component>().Select(x => x.GetType());
      //    Console.WriteLine($"[AdvancedHighlighting] GetSupportedComponentsTypes ([{gameObject.GetInstanceID()}] {gameObject.name},[{behaviour?.gameObject?.GetInstanceID()}] {behaviour?.gameObject?.name}), Supported = ({string.Join(", ", supported)}), Types1 = ({string.Join(", ", types1)}), Types2 = ({(types2 != null ? string.Join(", ", types2) : "")})");
      //    logged.Add(gameObject.GetInstanceID());
      //}
      return supported;
    }
    //private static readonly List<int> logged = new List<int>();
    public static string ModDirectory
    {
      get => Environment.CurrentDirectory + "\\QMods\\AdvancedHighlighting\\";
    }
    public static bool GetEntryData(this TechType tech, out EntryData data)
    {
      data = PDAScanner.GetEntryData(tech);
      return data != null;
    }
  }
}