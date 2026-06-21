using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Unitatea de apărare. Caută automat cel mai apropiat inamic din rază, își
    /// orientează țeava spre el și trage proiectile la intervale regulate.
    /// </summary>
    public class Turret : MonoBehaviour
    {
        float range;
        float fireRate;
        float damage;
        Color bulletColor;

        Transform barrel;
        Enemy target;
        float cooldown;

        public void Init(TurretBlueprint bp)
        {
            range = bp.range;
            fireRate = bp.fireRate;
            damage = bp.damage;
            bulletColor = bp.bulletColor;

            transform.localScale = Vector3.one * 0.8f;

            var bodySr = gameObject.AddComponent<SpriteRenderer>();
            bodySr.sprite = TextureFactory.CreateCircle(bp.color);
            bodySr.sortingOrder = 3;

            // Țeava (se rotește spre țintă).
            var b = new GameObject("Barrel");
            b.transform.SetParent(transform);
            b.transform.localPosition = new Vector3(0, 0.25f, 0);
            b.transform.localScale = new Vector3(0.3f, 0.85f, 1f);
            var sr = b.AddComponent<SpriteRenderer>();
            sr.sprite = TextureFactory.CreateSquare(Color.Lerp(bp.color, Color.black, 0.4f));
            sr.sortingOrder = 4;
            barrel = b.transform;
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing) return;

            cooldown -= Time.deltaTime;
            AcquireTarget();

            if (target != null)
            {
                AimAt(target.transform.position);
                if (cooldown <= 0f)
                {
                    Shoot();
                    cooldown = 1f / Mathf.Max(0.01f, fireRate);
                }
            }
        }

        void AcquireTarget()
        {
            // Păstrează ținta curentă cât timp e validă și în rază.
            if (target != null)
            {
                float d = (target.transform.position - transform.position).sqrMagnitude;
                if (d > range * range) target = null;
            }
            if (target != null) return;

            // Altfel, caută cel mai apropiat inamic din rază.
            float best = range * range;
            Enemy nearest = null;
            var list = Enemy.Active;
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e == null) continue;
                float d = (e.transform.position - transform.position).sqrMagnitude;
                if (d <= best)
                {
                    best = d;
                    nearest = e;
                }
            }
            target = nearest;
        }

        void AimAt(Vector3 worldPos)
        {
            Vector3 dir = worldPos - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            barrel.rotation = Quaternion.Euler(0, 0, angle);
        }

        void Shoot()
        {
            var go = new GameObject("Bullet");
            if (transform.parent != null) go.transform.SetParent(transform.parent);
            go.transform.position = transform.position;
            go.AddComponent<Bullet>().Init(target, damage, bulletColor);
        }
    }
}
