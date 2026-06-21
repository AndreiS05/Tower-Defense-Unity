using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Proiectil teleghidat: urmărește inamicul țintă și aplică daune la impact.
    /// Dacă are <see cref="splashRadius"/> &gt; 0, provoacă daune de zonă (AoE) tuturor
    /// inamicilor din raza de explozie. Dacă ținta dispare înainte de impact, explodează
    /// pe loc (pentru AoE) sau se autodistruge.
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        Enemy target;
        float damage;
        float splashRadius;
        Color color;
        float speed = 9f;

        public void Init(Enemy target, float damage, float splashRadius, Color color)
        {
            this.target = target;
            this.damage = damage;
            this.splashRadius = splashRadius;
            this.color = color;

            var sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = TextureFactory.CreateCircle(color);
            sr.sortingOrder = 7;
            transform.localScale = Vector3.one * (splashRadius > 0f ? 0.35f : 0.25f);
        }

        void Update()
        {
            if (target == null)
            {
                Impact(transform.position);
                return;
            }

            Vector3 dir = target.transform.position - transform.position;
            float step = speed * Time.deltaTime;
            if (dir.magnitude <= step + 0.1f)
            {
                Impact(target.transform.position);
                return;
            }
            transform.position += dir.normalized * step;
        }

        void Impact(Vector3 point)
        {
            if (splashRadius > 0f)
            {
                // Daune de zonă: lovește toți inamicii din raza de explozie.
                var list = Enemy.Active;
                float r2 = splashRadius * splashRadius;
                for (int i = 0; i < list.Count; i++)
                {
                    var e = list[i];
                    if (e == null) continue;
                    if ((e.transform.position - point).sqrMagnitude <= r2) e.TakeDamage(damage);
                }
                SplashEffect.Spawn(point, splashRadius, color, transform.parent);
            }
            else if (target != null)
            {
                target.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
