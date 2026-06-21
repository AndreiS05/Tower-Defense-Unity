using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefense
{
    /// <summary>
    /// Punctul de pornire al jocului. Construiește întreaga scenă din cod (camera,
    /// traseul, grila de celule, valurile, turele și interfața), astfel încât
    /// proiectul să fie complet jucabil doar apăsând Play, fără configurări manuale.
    ///
    /// Rulează automat la intrarea în Play Mode și la fiecare reîncărcare a scenei
    /// (de ex. butonul „Reincearca").
    /// </summary>
    public static class GameBootstrap
    {
        // Configurarea nivelului.
        const int Cols = 12;
        const int Rows = 7;
        const float Cell = 1.4f;
        const int WaveCount = 8;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            // Reconstruiește nivelul la fiecare încărcare de scenă (inclusiv restart).
            SceneManager.sceneLoaded += (scene, mode) => BuildLevel();
            BuildLevel(); // scena inițială este deja încărcată în acest moment
        }

        static void BuildLevel()
        {
            SetupCamera();

            // Manager de joc (resurse, vieți, stare).
            new GameObject("GameManager").AddComponent<GameManager>();

            // Traseul predefinit (colțurile prin care trec inamicii).
            Vector2Int[] corners =
            {
                new Vector2Int(0, 5), new Vector2Int(3, 5), new Vector2Int(3, 1),
                new Vector2Int(8, 1), new Vector2Int(8, 5), new Vector2Int(11, 5)
            };

            var cornerWorld = new List<Vector3>();
            var pathCells = new HashSet<Vector2Int>();
            for (int i = 0; i < corners.Length; i++)
            {
                cornerWorld.Add(CellToWorld(corners[i]));
                if (i < corners.Length - 1)
                    AddSegmentCells(corners[i], corners[i + 1], pathCells);
            }

            var path = new GameObject("Path").AddComponent<WaypointPath>();
            path.SetPoints(cornerWorld);

            // Manager de construcție.
            var build = new GameObject("BuildManager").AddComponent<BuildManager>();

            BuildGrid(pathCells, build);
            MarkPoint(corners[0], new Color(0.2f, 0.8f, 0.3f));                 // apariție
            MarkPoint(corners[corners.Length - 1], new Color(0.9f, 0.2f, 0.2f)); // bază (punct vital)

            // Valuri de inamici.
            var spawner = new GameObject("WaveSpawner").AddComponent<WaveSpawner>();
            spawner.Init(path, WaveCount);

            // Tipuri de ture disponibile în magazin.
            var blueprints = new List<TurretBlueprint>
            {
                new TurretBlueprint("Mitraliera", 50, 2.6f, 3.5f, 8f,
                    new Color(0.3f, 0.6f, 1f), Color.cyan),
                new TurretBlueprint("Tun", 100, 2.2f, 1.0f, 40f,
                    new Color(1f, 0.5f, 0.2f), Color.yellow),
                new TurretBlueprint("Lunetist", 150, 4.5f, 0.8f, 70f,
                    new Color(0.7f, 0.3f, 0.9f), Color.magenta)
            };
            build.Select(blueprints[0]);

            // Interfața.
            var ui = new GameObject("GameUI").AddComponent<GameUI>();
            ui.Init(blueprints, spawner);
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
            cam.transform.position = new Vector3(0, 0, -10);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.15f, 0.2f);
        }

        static void BuildGrid(HashSet<Vector2Int> pathCells, BuildManager build)
        {
            for (int c = 0; c < Cols; c++)
            {
                for (int r = 0; r < Rows; r++)
                {
                    var cell = new Vector2Int(c, r);
                    Vector3 pos = CellToWorld(cell);

                    if (pathCells.Contains(cell))
                    {
                        // Dală de traseu (drumul inamicilor).
                        var tile = new GameObject($"Path_{c}_{r}");
                        tile.transform.position = pos;
                        tile.transform.localScale = Vector3.one * (Cell * 0.98f);
                        var sr = tile.AddComponent<SpriteRenderer>();
                        sr.sprite = TextureFactory.CreateSquare(Color.white);
                        sr.color = new Color(0.45f, 0.32f, 0.2f);
                        sr.sortingOrder = 0;
                    }
                    else
                    {
                        // Celulă pe care se poate construi.
                        var nodeGo = new GameObject($"Node_{c}_{r}");
                        nodeGo.transform.position = pos;
                        nodeGo.transform.localScale = Vector3.one * (Cell * 0.9f);
                        var node = nodeGo.AddComponent<Node>();
                        node.Init(cell, new Color(0.2f, 0.4f, 0.25f));
                        build.Register(node, Cell);
                    }
                }
            }
        }

        static void MarkPoint(Vector2Int cell, Color color)
        {
            var go = new GameObject("Marker");
            go.transform.position = CellToWorld(cell) + new Vector3(0, 0, -0.1f);
            go.transform.localScale = Vector3.one * (Cell * 0.45f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = TextureFactory.CreateCircle(color);
            sr.sortingOrder = 2;
        }

        static void AddSegmentCells(Vector2Int a, Vector2Int b, HashSet<Vector2Int> set)
        {
            // Pas de -1/0/+1 pe fiecare axă (Mathf.Sign întoarce 1 pentru 0, deci îl evităm).
            int dx = b.x == a.x ? 0 : (b.x > a.x ? 1 : -1);
            int dy = b.y == a.y ? 0 : (b.y > a.y ? 1 : -1);
            var cur = a;
            set.Add(cur);
            while (cur != b)
            {
                cur = new Vector2Int(cur.x + dx, cur.y + dy);
                set.Add(cur);
            }
        }

        static Vector3 CellToWorld(Vector2Int cell)
        {
            float x = (cell.x - (Cols - 1) / 2f) * Cell;
            float y = (cell.y - (Rows - 1) / 2f) * Cell;
            return new Vector3(x, y, 0);
        }
    }
}
