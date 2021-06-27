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
		private static readonly FieldInfo canvasesInfo = typeof(CanvasLink).GetField("canvases", BindingFlags.NonPublic | BindingFlags.Instance);
	
		[HarmonyPatch("SetCanvasesEnabled")]
		[HarmonyPrefix]
		public static bool PrefixSetCanvasesEnabled(ref CanvasLink __instance)
		{
			if (__instance == null)
				return false;

			Canvas[] array = (Canvas[])(canvasesInfo.GetValue(__instance));
			List<Canvas> list = new List<Canvas>();
			if (array != null && array.Length > 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] != null)
					{
						list.Add(array[i]);
					}
					else
					{
					}
				}
				canvasesInfo.SetValue(__instance, list.ToArray());
			}
			return true;
		}
	}
}