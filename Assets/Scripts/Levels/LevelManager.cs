using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Coordonează campania: construiește nivelul curent, gestionează tranziția la
    /// nivelul următor după ce toate valurile (inclusiv boss-ul) au fost eliminate,
    /// și declanșează victoria finală după ultimul nivel.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        BuildManager build;
        List<TurretBlueprint> blueprints;
        GameUI ui;
        List<LevelDefinition> levels;
        bool transitioning;

        public int CurrentLevel { get; private set; }
        public int TotalLevels => levels != null ? levels.Count : 0;
        public WaveSpawner CurrentSpawner { get; private set; }
        public Transform CurrentRoot { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Init(BuildManager build, List<TurretBlueprint> blueprints, GameUI ui)
        {
            this.build = build;
            this.blueprints = blueprints;
            this.ui = ui;
            levels = LevelDefinition.CreateCampaign();
            CurrentLevel = 1;
            BuildLevel();
        }

        void BuildLevel()
        {
            if (CurrentRoot != null) Destroy(CurrentRoot.gameObject);
            build.ClearNodes();

            var def = levels[CurrentLevel - 1];
            CurrentRoot = new GameObject($"Level_{CurrentLevel}").transform;

            var routes = LevelBuilder.Build(def, build, CurrentRoot);

            var spawnerGo = new GameObject("WaveSpawner");
            spawnerGo.transform.SetParent(CurrentRoot);
            CurrentSpawner = spawnerGo.AddComponent<WaveSpawner>();
            CurrentSpawner.Init(routes, def, CurrentLevel);

            if (ui != null) ui.ShowBanner($"NIVELUL {CurrentLevel}", 2f);
        }

        /// <summary>Apelat de WaveSpawner când nivelul curent a fost curățat complet.</summary>
        public void NotifyLevelCleared()
        {
            if (transitioning) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing) return;
            transitioning = true;
            StartCoroutine(ClearRoutine());
        }

        IEnumerator ClearRoutine()
        {
            if (CurrentLevel < levels.Count)
            {
                if (ui != null) ui.ShowBanner($"NIVELUL {CurrentLevel} COMPLET!", 2.5f);
                yield return new WaitForSeconds(2.6f);

                if (GameManager.Instance != null) GameManager.Instance.AddMoney(75); // bonus de nivel
                CurrentLevel++;
                BuildLevel();
                transitioning = false;
            }
            else
            {
                // Ultimul nivel terminat — victorie de campanie.
                if (GameManager.Instance != null) GameManager.Instance.WinGame();
            }
        }
    }
}
