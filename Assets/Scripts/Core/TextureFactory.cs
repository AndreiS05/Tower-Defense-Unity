using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Generează sprite-uri simple (pătrat, cerc, inel) direct din cod, astfel încât
    /// proiectul să poată rula fără a importa fișiere grafice externe.
    /// </summary>
    public static class TextureFactory
    {
        /// <summary>Creează un sprite pătrat alb (colorabil prin SpriteRenderer.color).</summary>
        public static Sprite CreateSquare(Color color, int size = 32)
        {
            var tex = NewTex(size);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return ToSprite(tex, size);
        }

        /// <summary>Creează un sprite circular (folosit pentru inamici, ture, proiectile).</summary>
        public static Sprite CreateCircle(Color color, int size = 64)
        {
            var tex = NewTex(size);
            float r = size * 0.5f;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - r;
                    float dy = y + 0.5f - r;
                    bool inside = dx * dx + dy * dy <= r * r;
                    pixels[y * size + x] = inside ? color : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return ToSprite(tex, size);
        }

        /// <summary>Creează un inel (cerc gol) — folosit pentru contururi, portaluri, bază.</summary>
        public static Sprite CreateRing(Color color, float innerRatio = 0.62f, int size = 64)
        {
            var tex = NewTex(size);
            float r = size * 0.5f;
            float inner = r * innerRatio;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - r;
                    float dy = y + 0.5f - r;
                    float d2 = dx * dx + dy * dy;
                    bool inRing = d2 <= r * r && d2 >= inner * inner;
                    pixels[y * size + x] = inRing ? color : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return ToSprite(tex, size);
        }

        static Texture2D NewTex(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        static Sprite ToSprite(Texture2D tex, int size)
        {
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
