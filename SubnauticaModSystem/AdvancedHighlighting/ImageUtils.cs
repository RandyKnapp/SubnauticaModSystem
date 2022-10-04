using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if SN
#elif BZ
#endif

namespace AdvancedHighlighting
{
  public static class ImageUtils
  {
    private static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    public static Sprite LoadSprite(string path, Vector2 pivot = default(Vector2), TextureFormat format = TextureFormat.BC7, float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {
      Sprite sprite;
      bool flag = ImageUtils.spriteCache.TryGetValue(path, out sprite);
      Sprite result;
      if (flag)
      {
        result = sprite;
      }
      else
      {
        Texture2D texture2D = ImageUtils.LoadTexture(path, format, 2, 2);
        bool flag2 = !texture2D;
        if (flag2)
        {
          result = null;
        }
        else
        {
          Sprite sprite2 = ImageUtils.TextureToSprite(texture2D, pivot, pixelsPerUnit, spriteType, default(Vector4));
          ImageUtils.spriteCache.Add(path, sprite2);
          result = sprite2;
        }
      }
      return result;
    }

    public static Sprite Load9SliceSprite(string path, RectOffset slices, Vector2 pivot = default(Vector2), TextureFormat format = TextureFormat.BC7, float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {
      string key = path + slices;
      Sprite sprite;
      bool flag = ImageUtils.spriteCache.TryGetValue(key, out sprite);
      Sprite result;
      if (flag)
      {
        result = sprite;
      }
      else
      {
        Texture2D texture2D = ImageUtils.LoadTexture(path, format, 2, 2);
        bool flag2 = !texture2D;
        if (flag2)
        {
          result = null;
        }
        else
        {
          Vector4 border = new Vector4((float)slices.left, (float)slices.right, (float)slices.top, (float)slices.bottom);
          Sprite sprite2 = ImageUtils.TextureToSprite(texture2D, pivot, pixelsPerUnit, spriteType, border);
          ImageUtils.spriteCache.Add(key, sprite2);
          result = sprite2;
        }
      }
      return result;
    }

    public static Texture2D LoadTexture(string path, TextureFormat format = TextureFormat.BC7, int width = 2, int height = 2)
    {
      Texture2D texture2D;
      bool flag = ImageUtils.textureCache.TryGetValue(path, out texture2D);
      Texture2D result;
      if (flag)
      {
        result = texture2D;
      }
      else
      {
        bool flag2 = File.Exists(path);
        if (flag2)
        {
          byte[] data = File.ReadAllBytes(path);
          Texture2D texture2D2 = new Texture2D(width, height, format, false);
          bool flag3 = texture2D2.LoadImage(data);
          if (flag3)
          {
            ImageUtils.textureCache.Add(path, texture2D2);
            return texture2D2;
          }
        }
        else
        {
          Console.WriteLine("[ImageUtils] ERROR: File not found " + path);
        }
        result = null;
      }
      return result;
    }

    public static Sprite TextureToSprite(Texture2D tex, Vector2 pivot = default(Vector2), float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight, Vector4 border = default(Vector4))
    {
      return Sprite.Create(tex, new Rect(0f, 0f, (float)tex.width, (float)tex.height), pivot, pixelsPerUnit, 0U, spriteType, border);
    }
  }
}