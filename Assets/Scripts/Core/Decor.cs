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
}
