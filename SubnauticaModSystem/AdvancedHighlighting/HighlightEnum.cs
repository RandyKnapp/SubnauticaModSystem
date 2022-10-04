using System;
using System.Collections.Generic;
#if SN
#elif BZ
#endif

namespace AdvancedHighlighting
{
  public enum Highlight
  {
    ScannableItems,
    ScannableFragments,
    ScannedFragments,
    AnythingElse,

    Pickupable,
    PickupableCreatures,
    PickupableResource,
    BreakableResource,
    Interactables,
    StoryItems,
    JukeboxDisk,
    SealedDoor,
    GrownPlant,
    Bladderfish,
    CreatureEgg,
    BaseLadder,
    LaserCutObject
  }
  public static class HighlightExtenstions
  {
    private static readonly Dictionary<Highlight, string> highlightToText = new Dictionary<Highlight, string>()
    {
      [Highlight.ScannableItems] = "Scannable Items",
      [Highlight.ScannableFragments] = "Scannable Fragments",
      [Highlight.ScannedFragments] = "Already Scanned Fragments",
      [Highlight.AnythingElse] = "Anything Else",

      [Highlight.Pickupable] = "Pickupable Items",
      [Highlight.PickupableCreatures] = "Pickupable Creatures",
      [Highlight.PickupableResource] = "Pickupable Resources",
      [Highlight.BreakableResource] = "Breakable Outcrops",
      [Highlight.Interactables] = "Interactables",
      [Highlight.StoryItems] = "PDA Logs",
      [Highlight.JukeboxDisk] = "Jukebox Disks",
      [Highlight.SealedDoor] = "Sealed Ship Doors",
      [Highlight.GrownPlant] = "Grown Plant",
      [Highlight.Bladderfish] = "Bladder Fish",
      [Highlight.CreatureEgg] = "Creature Egg",
      [Highlight.BaseLadder] = "Base Ladder",
      [Highlight.LaserCutObject] = "Laser Cut Object",
    };
    private static readonly Dictionary<Highlight, Type> highlightToType = new Dictionary<Highlight, Type>()
    {
      [Highlight.Pickupable] = typeof(Pickupable),
      [Highlight.PickupableCreatures] = typeof(Creature),
      [Highlight.PickupableResource] = typeof(ResourceTracker),
      [Highlight.BreakableResource] = typeof(BreakableResource),
      [Highlight.Interactables] = typeof(HandTarget),
      [Highlight.StoryItems] = typeof(StoryHandTarget),
      [Highlight.JukeboxDisk] = typeof(JukeboxDisk),
      [Highlight.SealedDoor] = typeof(StarshipDoor),
      [Highlight.GrownPlant] = typeof(GrownPlant),
      [Highlight.Bladderfish] = typeof(Bladderfish),
      [Highlight.CreatureEgg] = typeof(CreatureEgg),
      [Highlight.BaseLadder] = typeof(BaseLadder),
      [Highlight.LaserCutObject] = typeof(Sealed),
    };
    private static readonly List<Highlight> notComponentHighlights = new List<Highlight>()
    {
      Highlight.ScannableItems,
      Highlight.ScannableFragments,
      Highlight.ScannedFragments,
      Highlight.AnythingElse
    };
    public static bool IsPressent(this Highlight highlight, UnityEngine.Behaviour behaviour) =>
        highlight.IsComponent() && behaviour.TryGetComponent(highlight.GetComponentType(), out _);
    public static bool IsPressentAndActive(this Highlight highlight, UnityEngine.Behaviour behaviour) =>
        highlight.IsComponent() && highlight.IsActive() && behaviour.TryGetComponent(highlight.GetComponentType(), out _);
    public static bool IsActive(this Highlight highlight) => Settings.GetInstance().IsHighlightActive(highlight);
    public static bool IsComponent(this Highlight highlight) => !notComponentHighlights.Contains(highlight);
    public static string GetName(this Highlight highlight) => highlightToText.TryGetValue(highlight, out string name) ? name : null;
    public static Type GetComponentType(this Highlight highlight) => highlightToType.TryGetValue(highlight, out Type type) ? type : null;
  }
}