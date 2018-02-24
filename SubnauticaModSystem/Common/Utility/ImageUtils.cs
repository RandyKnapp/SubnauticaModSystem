using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Utility
{
	public static class ImageUtils
	{
		public static Sprite LoadSprite(string path, TextureFormat format = TextureFormat.DXT5, float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight)
		{
			Texture2D texture2D = ImageUtils.LoadTexture(path, format);
			if (!texture2D)
			{
				return null;
			}
			return TextureToSprite(texture2D, pixelsPerUnit, spriteType);
		}

		public static Sprite Load9SliceSprite(string path, RectOffset slices, TextureFormat format = TextureFormat.DXT5, float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight)
		{
			Texture2D texture2D = ImageUtils.LoadTexture(path, format);
			if (!texture2D)
			{
				return null;
			}
			Vector4 border = new Vector4(slices.left, slices.right, slices.top, slices.bottom);
			return TextureToSprite(texture2D, pixelsPerUnit, spriteType, border);
		}

		public static Texture2D LoadTexture(string path, TextureFormat format = TextureFormat.DXT5)
		{
			if (File.Exists(path))
			{
				byte[] data = File.ReadAllBytes(path);
				Texture2D texture2D = new Texture2D(2, 2, format, false);
				if (texture2D.LoadImage(data))
				{
					return texture2D;
				}
			}
			else
			{
				Console.WriteLine("[ImageUtils] ERROR: File not found " + path);
			}
			return null;
		}

		public static Sprite TextureToSprite(Texture2D tex, float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight, Vector4 border = new Vector4())
		{
			return Sprite.Create(tex, new Rect(0f, 0f, (float)tex.width, (float)tex.height), new Vector2(0f, 0f), pixelsPerUnit, 0u, spriteType, border);
		}
	}
}
