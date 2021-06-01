using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutosortLockers
{
	[HarmonyPatch(typeof(CanvasLink))]
    public class CanvasLink_Patch
    {
		private static FieldInfo canvasesInfo = typeof(CanvasLink).GetField("canvases", BindingFlags.NonPublic | BindingFlags.Instance);
		/*
		private void SetCanvasesEnabled(bool enabled)
		{
			Canvas[] array = this.canvases;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = enabled;
			}
		}
		*/
		[HarmonyPatch("SetCanvasesEnabled")]
		[HarmonyPrefix]
		public static bool PrefixSetCanvasesEnabled(ref CanvasLink __instance)
		{
			if (__instance == null)
				return false;

			//QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"PrefixSetCanvasesEnabled(): begin");
			Canvas[] array = (Canvas[])(canvasesInfo.GetValue(__instance));
			//QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"PrefixSetCanvasesEnabled(): canvases array " + ((array == null || array.Length < 1) ? "not " : "") + "successfully retrieved"); 
			List<Canvas> list = new List<Canvas>();
			if (array != null && array.Length > 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					//QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"PrefixSetCanvasesEnabled(): checking canvas array index {i}");
					if (array[i] != null)
					{
						list.Add(array[i]);
					}
					else
					{
						//QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"PrefixSetCanvasesEnabled(): Found null entry in canvas array at index {i}");
					}

				}
				canvasesInfo.SetValue(__instance, list.ToArray());
			}

			//QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"PrefixSetCanvasesEnabled(): end");
			return true;
		}
	}
}
