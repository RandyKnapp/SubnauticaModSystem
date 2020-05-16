using System.Collections.Generic;
using UnityEngine;


namespace Common.Utility
{
	public static class ImageUtils
	{
		private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

		public static Sprite LoadSprite(string path, Vector2 pivot = new Vector2(), TextureFormat format = TextureFormat.BC7, float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight)
		{
			if (spriteCache.TryGetValue(path, out Sprite foundSprite))
			{
				return foundSprite;
			}

			Texture2D texture2D = SMLHelper.V2.Utility.ImageUtils.LoadTextureFromFile(path, format);
			if (!texture2D)
			{
				return null;
			}
			Sprite sprite = TextureToSprite(texture2D, pivot, pixelsPerUnit, spriteType);
			spriteCache.Add(path, sprite);
			return sprite;
		}

		public static Sprite Load9SliceSprite(string path, RectOffset slices, Vector2 pivot = new Vector2(), TextureFormat format = TextureFormat.BC7, float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight)
		{
			string spriteKey = path + slices;
			if (spriteCache.TryGetValue(spriteKey, out Sprite foundSprite))
			{
				return foundSprite;
			}

			Texture2D texture2D = SMLHelper.V2.Utility.ImageUtils.LoadTextureFromFile(path, format);
			if (!texture2D)
			{
				return null;
			}
			var border = new Vector4(slices.left, slices.right, slices.top, slices.bottom);
			Sprite sprite = TextureToSprite(texture2D, pivot, pixelsPerUnit, spriteType, border);
			spriteCache.Add(spriteKey, sprite);
			return sprite;
		}

		public static Sprite TextureToSprite(Texture2D tex, Vector2 pivot = new Vector2(), float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight, Vector4 border = new Vector4())
		{
			return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), pivot, pixelsPerUnit, 0u, spriteType, border);
		}
	}
}
