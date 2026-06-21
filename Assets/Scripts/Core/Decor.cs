using UnityEngine;

namespace TowerDefense
{
    /// <summary>Rotește continuu obiectul (folosit pentru portalul de start / inelul bazei).</summary>
    public class Rotate : MonoBehaviour
    {
        public float speed = 60f;

        void Update()
        {
            transform.Rotate(0f, 0f, speed * Time.deltaTime);
        }
    }

    /// <summary>Pulsează scara obiectului (efect „viu" pentru portal, bază, boss).</summary>
    public class Pulse : MonoBehaviour
    {
        public float amplitude = 0.1f;
        public float frequency = 2f;

        Vector3 baseScale;
        float offset;

        void Start()
        {
            baseScale = transform.localScale;
            offset = Random.value * 6.283f;
        }

        void Update()
        {
            float s = 1f + Mathf.Sin(Time.time * frequency + offset) * amplitude;
            transform.localScale = baseScale * s;
        }
    }

    /// <summary>Efect vizual de explozie pentru daunele de zonă (AoE): un cerc care se stinge.</summary>
    public class SplashEffect : MonoBehaviour
    {
        const float Life = 0.28f;
        float t;
        Color baseColor;
        SpriteRenderer sr;

        public static void Spawn(Vector3 pos, float radius, Color color, Transform parent)
        {
            var go = new GameObject("Splash");
            if (parent != null) go.transform.SetParent(parent);
            go.transform.position = new Vector3(pos.x, pos.y, -0.2f);
            go.transform.localScale = Vector3.one * radius * 2f; // diametru = 2 * rază
            go.AddComponent<SplashEffect>().Setup(color);
        }

        void Setup(Color color)
        {
            baseColor = color;
            baseColor.a = 0.45f;
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = TextureFactory.CreateCircle(Color.white);
            sr.color = baseColor;
            sr.sortingOrder = 7;
        }

        void Update()
        {
            t += Time.deltaTime / Life;
            if (t >= 1f) { Destroy(gameObject); return; }
            var c = baseColor;
            c.a = baseColor.a * (1f - t);
            sr.color = c;
        }
    }
}
