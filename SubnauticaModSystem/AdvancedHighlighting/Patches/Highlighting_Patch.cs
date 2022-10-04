using HarmonyLib;
using HighlightingSystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;

namespace AdvancedHighlighting.Patches
{
  public static class Highlighting_Patch
  {
    private static MethodInfo updateActiveHighlightersAsyncMethod;
    private static readonly FieldInfo highlightersField = typeof(Highlighting).GetField("highlighters", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo handlersField = typeof(Highlighting).GetField("handlers", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo updateActiveHighligtersCoroutineField = typeof(Highlighting).GetField("updateActiveHighligtersCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
    public static HashSet<Highlighter> GetHighlighters(this Highlighting instance) =>
         (HashSet<Highlighter>)highlightersField.GetValue(instance);
    public static Dictionary<Type, Highlighting.Handler> GetHandlers(this Highlighting instance) =>
         (Dictionary<Type, Highlighting.Handler>)handlersField.GetValue(instance);
    public static Coroutine GetUpdateActiveHighligtersCoroutine(this Highlighting instance) =>
         (Coroutine)updateActiveHighligtersCoroutineField.GetValue(instance);
    public static void SetUpdateActiveHighligtersCoroutine(this Highlighting instance, Coroutine coroutine) =>
        updateActiveHighligtersCoroutineField.SetValue(instance, coroutine);
    public static System.Collections.IEnumerator InvokeUpdateActiveHighlightersAsync(this Highlighting instance, Vector3 vector)
    {
      if (updateActiveHighlightersAsyncMethod == null)
        updateActiveHighlightersAsyncMethod = typeof(Highlighting).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.Name == "UpdateActiveHighlightersAsync").FirstOrDefault();
      return (System.Collections.IEnumerator)updateActiveHighlightersAsyncMethod.Invoke(instance, new object[] { vector });
    }

  }
  [HarmonyPatch(typeof(Highlighting), "OnUpdate")]
  public class Highlighting_OnUpdate_Patch
  {
    public static readonly Dictionary<int, string> stories = new Dictionary<int, string>();
    public static readonly Dictionary<int, string> pings = new Dictionary<int, string>();
    public static readonly List<int> notifications = new List<int>();
    private static int lastActiveTargetId = 0;
    private static float hoverStart = 0f;
    public static bool isLogged = false;
    public static bool Prefix(Highlighting __instance)
    {
      Highlighting.Mode mode = Highlighting.GetMode();
      var settings = Settings.GetInstance();
      if (mode == Highlighting.Mode.None || settings.ActiveHighlights.Count == 0)
      {
        lastActiveTargetId = 0;
      }
      else
      {
        var time = Time.time;
        Vector3 vector = MainCamera.camera?.transform.position ?? Vector3.zero;
        GameObject gameObject = null;
        if (mode == Highlighting.Mode.Exosuit) gameObject = (Player.main.GetVehicle() as Exosuit).GetActiveTarget();
        else if (mode == Highlighting.Mode.Player) gameObject = Player.main?.guiHand?.GetActiveTarget();
        int id = gameObject?.GetInstanceID() ?? 0;
        if (lastActiveTargetId != id)
        {
          hoverStart = time;
          lastActiveTargetId = id;
        }
        if (__instance.GetUpdateActiveHighligtersCoroutine() == null)
        {
          if (__instance.radiusOuter != settings.HighlightSearchDistance)
            __instance.radiusOuter = settings.HighlightSearchDistance;
          __instance.SetUpdateActiveHighligtersCoroutine(__instance.StartCoroutine(__instance.InvokeUpdateActiveHighlightersAsync(vector)));
        }

        var highlighters = __instance.GetHighlighters();
        if (settings.StoryNotification || settings.DisksNotification || settings.ShowPings)
        {
          var removed = new List<int>();
          foreach (var key in stories.Keys)
          {
            if (!highlighters.Any(x => x.GetInstanceID() == key))
              removed.Add(key);
          }
          foreach (var key in removed)
          {
            if (notifications.Contains(key))
            {
              notifications.Remove(key);
              ErrorMessage.AddMessage($"[Advanced Highlighting] {Language.main.Get(stories[key])} is no longer nearby");
            }
            if (pings.ContainsKey(key))
            {
              var ping = PingManager.Get(pings[key]);
              if (ping != null)
                UnityEngine.Object.Destroy(ping);
              pings.Remove(key);
            }
            stories.Remove(key);
          }
        }
        var radiusRange = settings.HighlightSearchDistance - __instance.radiusInner;

        if (!settings.PresetApplied && MainCamera.camera != null && MainCamera.camera.TryGetComponent(out HighlightingRenderer renderer))
        {
          settings.PresetApplied = true;
          if (settings.HighlightingRenderer >= 0 && settings.HighlightingRenderer < Utils.Presets.Count)
            renderer.ApplyPreset(Utils.Presets[settings.HighlightingRenderer]);
          //Console.WriteLine($"[AdvancedHighlighting] Active {settings.HighlightingRenderer},{Utils.Presets.Count} = [{renderer.name}, {renderer.blurDirections}, {renderer.blurIntensity}, {renderer.blurMinSpread}, {renderer.blurSpread}, {renderer.downsampleFactor}, {renderer.fillAlpha}, {renderer.iterations}]");
        }
        foreach (Highlighter highlighter in highlighters)
        {
          if (highlighter != null)
          {
            var highlights = highlighter.gameObject.GetHighlightsForObject(out _, out _);
            if (highlights.Count > 0)
            {
              Color highlightingColor = settings.GetHighlightColor(highlights.First());
              if (highlightingColor.a != 0f)
              {
                float magnitude = (highlighter.bounds.center - vector).magnitude;
                float a = Mathf.Max(Mathf.Clamp01(1f - (magnitude - __instance.radiusInner) / radiusRange), settings.MinimumAlpha);
                highlightingColor.a = a * a;
                highlighter.ConstantOn(highlightingColor, 0f);
                highlighter.filterMode = RendererFilterMode.None;

                if (magnitude <= settings.HighlightSearchDistance)
                  NotifiyAndPing(highlighter);
              }
              else
                highlighter.ConstantOn(highlightingColor, 0f);
            }
          }
        }
        if (gameObject != null)
        {
          GameObject highlightableRoot = null;
          Highlighting_GetHighlightableRoot_Patch.Prefix(__instance, ref highlightableRoot, mode, gameObject);
          if (highlightableRoot != null)
          {
            Highlighter highlighter = highlightableRoot.GetComponent<Highlighter>();
            if (highlighter != null)
            {
              Color highlightingColor = highlighter.constantColor;
              float a2 = Mathf.Lerp(0.4f, 1f, Mathf.Cos((time - hoverStart) * 2f * Mathf.PI * __instance.hoverFlashingSpeed) * 0.5f + 0.5f);
              highlightingColor.a = a2 * a2;
              highlighter.Hover(highlightingColor);
            }
          }
        }
      }
      return false;
    }
    private static void NotifiyAndPing(Highlighter highlighter)
    {
      var objId = highlighter.GetInstanceID();
      if (!stories.ContainsKey(objId))
      {
        string name = null;
        string type = null;
        if (highlighter.TryGetComponent(out StoryHandTarget story))
        {
          name = Language.main.Get(story.primaryTooltip);
          type = "story";
        }
        else if (highlighter.TryGetComponent(out JukeboxDisk disk))
        {
          name = Language.main.Get(Jukebox.GetInfo(disk.track).label);
          type = "disk";
        }
        if (name != null)
        {
          var settings = Settings.GetInstance();
          stories[objId] = name;
          if ((type == "story" && settings.StoryNotification) ||
              (type == "disk" && settings.DisksNotification))
          {
            notifications.Add(objId);
            ErrorMessage.AddMessage($"[Advanced Highlighting] {name} is nearby");
          }
          if (settings.ShowPings)
          {
            PingInstance ping = highlighter.gameObject.EnsureComponent<PingInstance>();
            ping.origin = highlighter.gameObject.transform;
            ping.SetFakePosition(highlighter.bounds.center);
            ping.SetVisible(true);
            ping.SetLabel(name);
            ping._id = $"ahping_{type}_{objId}";
            ping.pingType = PingType.Signal;
            ping.displayPingInManager = false;
            ping.visitable = false;
            ping.minDist = 5f;
            ping.range = 5f;
            ping.enabled = true;
            pings[objId] = ping._id;
          }
        }
      }
    }
  }

  [HarmonyPatch(typeof(Highlighting), "GetHighlightableRoot", new Type[] { typeof(Highlighting.Mode), typeof(GameObject) })]
  public static class Highlighting_GetHighlightableRoot_Patch
  {
    public static bool Prefix(Highlighting __instance, ref GameObject __result, Highlighting.Mode mode, GameObject target)
    {
      __result = null;
      if (mode == Highlighting.Mode.None)
        return false;
      var settings = Settings.GetInstance();
      if (mode == Highlighting.Mode.Exosuit && !settings.ExosuiteHighlightsEnabled)
      {
        Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
        if (exosuit != null)
          __result = exosuit.GetInteractableRoot(target);
        return false;
      }
      else
      {
        var highlights = target.GetHighlightsForObject(out GameObject result, out Behaviour behaviour);
        if (highlights.Count == 0)
          return false;
        if (highlights.Contains(Highlight.ScannableFragments) ||
            highlights.Contains(Highlight.ScannedFragments) ||
            highlights.Contains(Highlight.ScannableItems))
        {
          __result = result;
          return false;
        }

        if (behaviour == null || !behaviour.enabled || behaviour.GetComponentInParent<Player>() != null ||
            behaviour.GetComponentInParent<HighlightingBlocker>() != null)
        {
          return false;
        }

        var handlers = __instance.GetHandlers();
        if (handlers != null && handlers.TryGetValue(behaviour.GetType(), out Highlighting.Handler handler))
        {
          handler(ref behaviour);
          if (behaviour == null)
            return false;
        }
        __result = behaviour.gameObject;
      }
      return false;
    }
  }

}