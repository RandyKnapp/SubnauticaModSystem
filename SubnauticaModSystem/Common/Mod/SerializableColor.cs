using System;
using UnityEngine;

namespace Common.Mod
{
	[Serializable]
	public class SerializableColor
	{
		public float r = 1;
		public float g = 1;
		public float b = 1;
		public float a = 1;

		public static implicit operator SerializableColor(Color c)
		{
			return new SerializableColor() { r = c.r, g = c.g, b = c.b, a = c.a };
		}

		public Color ToColor()
		{
			return new Color(r, g, b, a);
		}
	}

}
