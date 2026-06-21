using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    /// <summary>Parametrii cu care este inițializat un inamic la apariție.</summary>
    public struct EnemyStats
    {
        public float maxHealth;
        public float speed;
        public int reward;
        public int baseDamage;
        public Color color;
        public float scale;
    }

    /// <summary>
    /// Inamic care urmează traiectoria (Waypoints). Are puncte de viață, poate fi
    /// lovit de proiectile, oferă o recompensă la moarte și provoacă pagube bazei
    /// dacă ajunge la capătul traseului.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        /// <summary>Registru global al inamicilor activi, folosit de ture pentru țintire.</summary>
        public static readonly List<Enemy> Active = new List<Enemy>();

        /// <summary>Eveniment declanșat când un inamic dispare (ucis sau ajuns la bază).</summary>
        public static event Action OnRemoved;

        float maxHealth;
        float health;
        float speed;
        int reward;
        int baseDamage;

        WaypointPath path;
        int waypointIndex;
        Transform healthBar;
        bool removed;

        public void Init(WaypointPath path, EnemyStats stats)
        {
            this.path = path;
            maxHealth = stats.maxHealth;
            health = stats.maxHealth;
            speed = stats.speed;
            reward = stats.reward;
            baseDamage = stats.baseDamage;
            waypointIndex = 0;
            transform.position = path.Start;
            BuildVisual(stats.color, stats.scale);
        }

        void BuildVisual(Color color, float scale)
        {
            var body = gameObject.AddComponent<SpriteRenderer>();
            body.sprite = TextureFactory.CreateCircle(color);
            body.sortingOrder = 5;
            transform.localScale = Vector3.one * scale;

            // Bară de viață simplă deasupra inamicului.
            var bar = new GameObject("HealthBar");
            bar.transform.SetParent(transform);
            bar.transform.localPosition = new Vector3(0, 0.75f, 0);
            bar.transform.localScale = new Vector3(1f, 0.14f, 1f);
            var sr = bar.AddComponent<SpriteRenderer>();
            sr.sprite = TextureFactory.CreateSquare(Color.white);
            sr.color = Color.green;
            sr.sortingOrder = 6;
            healthBar = bar.transform;
        }

        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        void Update()
        {
            if (path == null) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing) return;

            Vector3 target = path.GetPoint(waypointIndex);
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            if ((transform.position - target).sqrMagnitude < 0.0025f)
            {
                if (path.IsLast(waypointIndex)) { ReachBase(); return; }
                waypointIndex++;
            }
        }

        /// <summary>Aplică daune; actualizează bara de viață și verifică moartea.</summary>
        public void TakeDamage(float amount)
        {
            if (removed) return;
            health -= amount;
            float t = Mathf.Clamp01(health / maxHealth);
            if (healthBar != null)
            {
                healthBar.localScale = new Vector3(t, 0.14f, 1f);
                var sr = healthBar.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.Lerp(Color.red, Color.green, t);
            }
            if (health <= 0f) Die();
        }

        void Die()
        {
            if (removed) return;
            removed = true;
            if (GameManager.Instance != null) GameManager.Instance.AddMoney(reward);
            Remove();
        }

        void ReachBase()
        {
            if (removed) return;
            removed = true;
            if (GameManager.Instance != null) GameManager.Instance.LoseLives(baseDamage);
            Remove();
        }

        void Remove()
        {
            OnRemoved?.Invoke();
            Destroy(gameObject);
        }
    }
}
