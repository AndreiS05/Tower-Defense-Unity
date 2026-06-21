using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Construiește vizual un nivel: traseele (Waypoints), dalele de drum, celulele
    /// pe care se poate construi, portalurile de apariție și baza (punctul vital).
    /// Toate obiectele sunt puse sub un „root" pentru a fi șterse ușor la schimbarea nivelului.
    /// </summary>
    public static class LevelBuilder
    {
        public const int Cols = 12;
        public const int Rows = 7;
        public const float Cell = 1.4f;

        public static Vector3 CellToWorld(Vector2Int c)
        {
            float x = (c.x - (Cols - 1) / 2f) * Cell;
            float y = (c.y - (Rows - 1) / 2f) * Cell;
            return new Vector3(x, y, 0);
        }

        /// <summary>Construiește nivelul și întoarce lista de rute (pentru WaveSpawner).</summary>
        public static List<WaypointPath> Build(LevelDefinition def, BuildManager build, Transform root)
        {
            var pathCells = new HashSet<Vector2Int>();
            var paths = new List<WaypointPath>();

            // Creează câte un WaypointPath pentru fiecare rută și marchează celulele traseului.
            foreach (var route in def.routes)
            {
                var go = new GameObject("Route");
                go.transform.SetParent(root);
                var wp = go.AddComponent<WaypointPath>();
                var pts = new List<Vector3>();
                for (int i = 0; i < route.Length; i++)
                {
                    pts.Add(CellToWorld(route[i]));
                    if (i < route.Length - 1) AddSegmentCells(route[i], route[i + 1], pathCells);
                }
                wp.SetPoints(pts);
                paths.Add(wp);
            }

            // Grila: dale de drum pe traseu, celule construibile în rest.
            for (int c = 0; c < Cols; c++)
            {
                for (int r = 0; r < Rows; r++)
                {
                    var cell = new Vector2Int(c, r);
                    Vector3 pos = CellToWorld(cell);

                    if (pathCells.Contains(cell))
                    {
                        var tile = new GameObject($"Path_{c}_{r}");
                        tile.transform.SetParent(root);
                        tile.transform.position = pos;
                        tile.transform.localScale = Vector3.one * (Cell * 0.98f);
                        var sr = tile.AddComponent<SpriteRenderer>();
                        sr.sprite = TextureFactory.CreateSquare(Color.white);
                        sr.color = new Color(0.42f, 0.30f, 0.18f);
                        sr.sortingOrder = 0;
                    }
                    else
                    {
                        var nodeGo = new GameObject($"Node_{c}_{r}");
                        nodeGo.transform.SetParent(root);
                        nodeGo.transform.position = pos;
                        nodeGo.transform.localScale = Vector3.one * (Cell * 0.9f);
                        var node = nodeGo.AddComponent<Node>();
                        node.Init(cell, new Color(0.2f, 0.4f, 0.25f));
                        build.Register(node, Cell);
                    }
                }
            }

            // Portaluri de apariție (start) pentru fiecare rută.
            foreach (var route in def.routes)
                BuildStartPortal(CellToWorld(route[0]), root);

            // Baza (punctul vital) — la finalul comun al rutelor.
            var baseCell = def.routes[0][def.routes[0].Length - 1];
            BuildBase(CellToWorld(baseCell), root);

            return paths;
        }

        /// <summary>Portal de apariție: inel rotativ + miez pulsatil (cyan).</summary>
        static void BuildStartPortal(Vector3 pos, Transform root)
        {
            var portal = new GameObject("StartPortal");
            portal.transform.SetParent(root);
            portal.transform.position = pos + new Vector3(0, 0, -0.1f);
            portal.transform.localScale = Vector3.one * (Cell * 0.85f);

            var ring = new GameObject("Ring");
            ring.transform.SetParent(portal.transform);
            ring.transform.localPosition = Vector3.zero;
            var ringSr = ring.AddComponent<SpriteRenderer>();
            ringSr.sprite = TextureFactory.CreateRing(new Color(0.3f, 0.9f, 1f), 0.55f);
            ringSr.sortingOrder = 2;
            ring.AddComponent<Rotate>().speed = 80f;

            var core = new GameObject("Core");
            core.transform.SetParent(portal.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.55f;
            var coreSr = core.AddComponent<SpriteRenderer>();
            coreSr.sprite = TextureFactory.CreateCircle(new Color(0.1f, 0.4f, 0.7f, 0.9f));
            coreSr.sortingOrder = 3;
            var pulse = core.AddComponent<Pulse>();
            pulse.amplitude = 0.15f;
            pulse.frequency = 3f;
        }

        /// <summary>Baza (punctul vital): inel exterior + romb + miez pulsatil (roșu).</summary>
        static void BuildBase(Vector3 pos, Transform root)
        {
            var baseGo = new GameObject("Base");
            baseGo.transform.SetParent(root);
            baseGo.transform.position = pos + new Vector3(0, 0, -0.1f);
            baseGo.transform.localScale = Vector3.one * (Cell * 0.95f);

            var ring = new GameObject("Ring");
            ring.transform.SetParent(baseGo.transform);
            ring.transform.localPosition = Vector3.zero;
            var ringSr = ring.AddComponent<SpriteRenderer>();
            ringSr.sprite = TextureFactory.CreateRing(new Color(1f, 0.85f, 0.2f), 0.78f);
            ringSr.sortingOrder = 2;
            ring.AddComponent<Rotate>().speed = -35f;

            var diamond = new GameObject("Diamond");
            diamond.transform.SetParent(baseGo.transform);
            diamond.transform.localPosition = Vector3.zero;
            diamond.transform.localRotation = Quaternion.Euler(0, 0, 45f);
            diamond.transform.localScale = Vector3.one * 0.62f;
            var dSr = diamond.AddComponent<SpriteRenderer>();
            dSr.sprite = TextureFactory.CreateSquare(Color.white);
            dSr.color = new Color(0.6f, 0.12f, 0.12f);
            dSr.sortingOrder = 2;

            var core = new GameObject("Core");
            core.transform.SetParent(baseGo.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.35f;
            var coreSr = core.AddComponent<SpriteRenderer>();
            coreSr.sprite = TextureFactory.CreateCircle(new Color(1f, 0.4f, 0.25f));
            coreSr.sortingOrder = 3;
            var pulse = core.AddComponent<Pulse>();
            pulse.amplitude = 0.2f;
            pulse.frequency = 2.5f;
        }

        static void AddSegmentCells(Vector2Int a, Vector2Int b, HashSet<Vector2Int> set)
        {
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
    }
}
