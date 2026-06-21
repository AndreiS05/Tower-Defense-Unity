using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Unitatea de apărare. Caută automat cele mai apropiate ținte din rază, își
    /// orientează țeava spre prima și trage proiectile. Suportă lovirea mai multor
    /// ținte simultan (ex. Inferno) și daune de zonă la impact (ex. Mortieră).
    /// </summary>
    public class Turret : MonoBehaviour
    {
        float range;
        float fireRate;
        float damage;
        float splashRadius;
        int maxTargets;
        Color bulletColor;

        Transform barrel;
        float cooldown;
        readonly List<Enemy> targets = new List<Enemy>();

        public void Init(TurretBlueprint bp)
        {
            range = bp.range;
            fireRate = bp.fireRate;
            damage = bp.damage;
            splashRadius = bp.splashRadius;
            maxTargets = Mathf.Max(1, bp.maxTargets);
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
            AcquireTargets();

            if (targets.Count > 0)
            {
                AimAt(targets[0].transform.position);
                if (cooldown <= 0f)
                {
                    for (int i = 0; i < targets.Count; i++) ShootAt(targets[i]);
                    cooldown = 1f / Mathf.Max(0.01f, fireRate);
                }
            }
        }

        /// <summary>Selectează cele mai apropiate „maxTargets" ținte aflate în rază.</summary>
        void AcquireTargets()
        {
            targets.Clear();
            float r2 = range * range;
            var list = Enemy.Active;
            for (int k = 0; k < maxTargets; k++)
            {
                Enemy best = null;
                float bestD = r2;
                for (int i = 0; i < list.Count; i++)
                {
                    var e = list[i];
                    if (e == null || targets.Contains(e)) continue;
                    float d = (e.transform.position - transform.position).sqrMagnitude;
                    if (d <= bestD) { bestD = d; best = e; }
                }
                if (best == null) break;
                targets.Add(best);
            }
        }

        void AimAt(Vector3 worldPos)
        {
            Vector3 dir = worldPos - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            barrel.rotation = Quaternion.Euler(0, 0, angle);
        }

        void ShootAt(Enemy target)
        {
            var go = new GameObject("Bullet");
            if (transform.parent != null) go.transform.SetParent(transform.parent);
            go.transform.position = transform.position;
            go.AddComponent<Bullet>().Init(target, damage, splashRadius, bulletColor);
        }
    }
}
