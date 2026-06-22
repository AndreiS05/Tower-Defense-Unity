using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Gestionează cele 8 valuri (Waves) ale unui nivel. Inamicii apar pe una dintre
    /// rutele nivelului, aleasă aleatoriu. Ultimul val (8) aduce un BOSS mare. Când tot
    /// nivelul a fost curățat, anunță <see cref="LevelManager"/> pentru a trece mai departe.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        const int WavesPerLevel = 8;

        class Wave
        {
            public int count;
            public float interval;
            public float health;
            public float speed;
            public int reward;
            public Color color;
            public float scale;
        }

        List<WaypointPath> routes;
        LevelDefinition def;
        int level;
        readonly List<Wave> waves = new List<Wave>();
        float timeBetweenWaves = 6f;
        int aliveCount;
        bool allSpawned;

        public int CurrentWave { get; private set; }
        public int TotalWaves => WavesPerLevel;

        public void Init(List<WaypointPath> routes, LevelDefinition def, int level)
        {
            this.routes = routes;
            this.def = def;
            this.level = level;
            GenerateWaves();
        }

        void GenerateWaves()
        {
            for (int i = 0; i < WavesPerLevel; i++)
            {
                float tNorm = i / (float)(WavesPerLevel - 1);
                waves.Add(new Wave
                {
                    count = 5 + i * 2,
                    interval = Mathf.Max(0.35f, 0.9f - i * 0.05f),
                    health = (40f + i * 18f) * def.healthMultiplier,
                    speed = 2.2f + i * 0.1f,
                    // Economie: recompensă de bază (x1.5) înmulțită cu un bonus pe nivel
                    // (nivel 1 x1.0, nivel 2 x1.5, nivel 3 x2.0), fiindcă inamicii devin mai grei.
                    reward = Mathf.RoundToInt((4 + i * 0.5f) * 1.5f * (1f + 0.5f * (level - 1))),
                    color = Color.Lerp(def.enemyColorA, def.enemyColorB, tNorm),
                    scale = 0.55f + i * 0.015f
                });
            }
        }

        void OnEnable() => Enemy.OnRemoved += HandleEnemyRemoved;
        void OnDisable() => Enemy.OnRemoved -= HandleEnemyRemoved;

        void HandleEnemyRemoved()
        {
            aliveCount = Mathf.Max(0, aliveCount - 1);
            TryComplete();
        }

        IEnumerator Start()
        {
            yield return null; // lasă restul sistemului să se inițializeze

            for (int w = 0; w < waves.Count; w++)
            {
                CurrentWave = w + 1;

                float t = timeBetweenWaves;
                while (t > 0f)
                {
                    if (IsDefeated()) yield break;
                    t -= Time.deltaTime;
                    yield return null;
                }

                yield return StartCoroutine(SpawnWave(waves[w]));

                // Ultimul val: după valul de inamici de rând, apare BOSS-ul.
                if (w == waves.Count - 1)
                {
                    yield return new WaitForSeconds(1.2f);
                    if (IsDefeated()) yield break;
                    SpawnBoss();
                }
            }

            allSpawned = true;
            TryComplete();
        }

        IEnumerator SpawnWave(Wave wave)
        {
            for (int i = 0; i < wave.count; i++)
            {
                if (IsDefeated()) yield break;
                SpawnEnemy(wave);
                yield return new WaitForSeconds(wave.interval);
            }
        }

        void SpawnEnemy(Wave wave)
        {
            var go = new GameObject("Enemy");
            go.transform.SetParent(transform.parent);
            go.AddComponent<Enemy>().Init(RandomRoute(), new EnemyStats
            {
                maxHealth = wave.health,
                speed = wave.speed,
                reward = wave.reward,
                baseDamage = 1,
                color = wave.color,
                scale = wave.scale,
                isBoss = false
            });
            aliveCount++;
        }

        void SpawnBoss()
        {
            if (GameUI.Instance != null) GameUI.Instance.ShowBanner("BOSS!", 2f);

            var go = new GameObject("Boss");
            go.transform.SetParent(transform.parent);
            go.AddComponent<Enemy>().Init(RandomRoute(), new EnemyStats
            {
                maxHealth = (600f + 600f * (level - 1)) * def.healthMultiplier,
                speed = 1.6f,
                reward = Mathf.RoundToInt((80 + level * 20) * 1.5f),
                baseDamage = 10,
                color = def.bossColor,
                scale = 1.8f,
                isBoss = true
            });
            aliveCount++;
        }

        WaypointPath RandomRoute()
        {
            return routes[Random.Range(0, routes.Count)];
        }

        void TryComplete()
        {
            if (allSpawned && aliveCount == 0 && !IsDefeated() && LevelManager.Instance != null)
                LevelManager.Instance.NotifyLevelCleared();
        }

        static bool IsDefeated()
        {
            return GameManager.Instance != null &&
                   GameManager.Instance.State == GameManager.GameState.Defeat;
        }
    }
}
