using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Gestionează valurile de inamici (Waves). Generează automat valuri din ce în ce
    /// mai dificile, le lansează pe rând cu o pauză între ele și declanșează victoria
    /// când toate valurile au fost eliminate.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
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

        WaypointPath path;
        readonly List<Wave> waves = new List<Wave>();
        float timeBetweenWaves = 6f;
        int aliveCount;
        bool allSpawned;

        public int CurrentWave { get; private set; }
        public int TotalWaves => waves.Count;
        public float CountdownToNext { get; private set; }

        public void Init(WaypointPath path, int waveCount)
        {
            this.path = path;
            GenerateWaves(waveCount);
        }

        void GenerateWaves(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float tNorm = i / (float)Mathf.Max(1, count - 1);
                waves.Add(new Wave
                {
                    count = 5 + i * 2,
                    interval = Mathf.Max(0.35f, 0.9f - i * 0.05f),
                    health = 40f + i * 18f,
                    speed = 2.2f + i * 0.12f,
                    reward = 12 + i,
                    color = Color.Lerp(new Color(1f, 0.65f, 0.2f), new Color(0.85f, 0.1f, 0.1f), tNorm),
                    scale = 0.55f + i * 0.015f
                });
            }
        }

        void OnEnable() => Enemy.OnRemoved += HandleEnemyRemoved;
        void OnDisable() => Enemy.OnRemoved -= HandleEnemyRemoved;

        void HandleEnemyRemoved()
        {
            aliveCount = Mathf.Max(0, aliveCount - 1);
            if (allSpawned && aliveCount == 0 && GameManager.Instance != null)
                GameManager.Instance.WinGame();
        }

        IEnumerator Start()
        {
            yield return null; // lasă restul sistemului să se inițializeze

            for (int w = 0; w < waves.Count; w++)
            {
                CurrentWave = w + 1;

                // Numărătoare inversă până la următorul val.
                float t = timeBetweenWaves;
                while (t > 0f)
                {
                    if (IsDefeated()) yield break;
                    CountdownToNext = t;
                    t -= Time.deltaTime;
                    yield return null;
                }

                yield return StartCoroutine(SpawnWave(waves[w]));
            }

            allSpawned = true;
            if (aliveCount == 0 && GameManager.Instance != null)
                GameManager.Instance.WinGame();
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
            var enemy = go.AddComponent<Enemy>();
            enemy.Init(path, new EnemyStats
            {
                maxHealth = wave.health,
                speed = wave.speed,
                reward = wave.reward,
                baseDamage = 1,
                color = wave.color,
                scale = wave.scale
            });
            aliveCount++;
        }

        static bool IsDefeated()
        {
            return GameManager.Instance != null &&
                   GameManager.Instance.State == GameManager.GameState.Defeat;
        }
    }
}
