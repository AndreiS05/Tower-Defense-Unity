using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefense
{
    /// <summary>
    /// Punctul de pornire al jocului. Creează managerii (joc, construcție, nivel) și
    /// interfața, apoi lasă <see cref="LevelManager"/> să construiască nivelele din cod.
    /// Astfel proiectul este complet jucabil doar apăsând Play, fără configurări manuale.
    ///
    /// Rulează automat la intrarea în Play Mode și la fiecare reîncărcare a scenei
    /// (de ex. butonul „Reia jocul" de pe ecranul final).
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            SceneManager.sceneLoaded += (scene, mode) => Boot();
            Boot(); // scena inițială este deja încărcată în acest moment
        }

        static void Boot()
        {
            SetupCamera();

            // Manager de joc (resurse, vieți, stare).
            new GameObject("GameManager").AddComponent<GameManager>();

            // Manager de construcție (selecție tură + plasare).
            var build = new GameObject("BuildManager").AddComponent<BuildManager>();

            // Tipurile de ture din magazin (unele se deblochează pe niveluri).
            var blueprints = new List<TurretBlueprint>
            {
                // Disponibile de la nivelul 1.
                new TurretBlueprint("Mitraliera", 50, 2.6f, 3.5f, 8f,
                    new Color(0.3f, 0.6f, 1f), Color.cyan, unlockLevel: 1),
                new TurretBlueprint("Tun", 100, 2.2f, 1.0f, 40f,
                    new Color(1f, 0.5f, 0.2f), Color.yellow, unlockLevel: 1),
                new TurretBlueprint("Lunetist", 150, 4.5f, 0.8f, 70f,
                    new Color(0.7f, 0.3f, 0.9f), Color.magenta, unlockLevel: 1),

                // Nivel 2: Mortieră — daune de zonă (AoE) pe mai multe tile-uri.
                new TurretBlueprint("Mortiera", 175, 2.8f, 0.8f, 28f,
                    new Color(0.4f, 0.8f, 0.3f), new Color(0.7f, 1f, 0.35f),
                    unlockLevel: 2, splashRadius: 1.8f),

                // Nivel 3: Inferno — lovește 3 inamici simultan, rază mare, cel mai scump.
                new TurretBlueprint("Inferno", 275, 5.0f, 1.2f, 34f,
                    new Color(1f, 0.35f, 0.1f), new Color(1f, 0.5f, 0.15f),
                    unlockLevel: 3, maxTargets: 3)
            };
            build.Select(blueprints[0]);

            // Interfața (trebuie să existe înainte de a porni nivelele, pentru bannere/evenimente).
            var ui = new GameObject("GameUI").AddComponent<GameUI>();
            ui.Init(blueprints);

            // Managerul de nivel construiește campania (3 niveluri) începând cu nivelul 1.
            var lm = new GameObject("LevelManager").AddComponent<LevelManager>();
            lm.Init(build, blueprints, ui);
        }

        static void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            // Ridicăm puțin terenul (cameră mai jos) ca tile-urile de jos să nu fie acoperite de magazin.
            cam.transform.position = new Vector3(0, -0.4f, -10);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.15f, 0.2f);
        }
    }
}
