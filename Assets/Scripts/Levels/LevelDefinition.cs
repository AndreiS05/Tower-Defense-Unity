using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Descrie un nivel: rutele (traseele) pe care le pot urma inamicii, dificultatea
    /// (multiplicator de viață) și paleta de culori. Fiecare rută este o listă de
    /// colțuri (celule din grilă); toate rutele unui nivel se termină în aceeași bază.
    /// </summary>
    public class LevelDefinition
    {
        public int index;
        public List<Vector2Int[]> routes;
        public float healthMultiplier;
        public Color enemyColorA;
        public Color enemyColorB;
        public Color bossColor;

        /// <summary>Construiește campania de 3 niveluri (1, 2 și 3 rute).</summary>
        public static List<LevelDefinition> CreateCampaign()
        {
            return new List<LevelDefinition>
            {
                // Nivel 1 — o singură rută (traseu în formă de S).
                new LevelDefinition
                {
                    index = 1,
                    healthMultiplier = 1.0f,
                    enemyColorA = new Color(1f, 0.65f, 0.2f),
                    enemyColorB = new Color(0.85f, 0.1f, 0.1f),
                    bossColor = new Color(0.45f, 0.08f, 0.12f),
                    routes = new List<Vector2Int[]>
                    {
                        new[]
                        {
                            new Vector2Int(0, 5), new Vector2Int(3, 5), new Vector2Int(3, 1),
                            new Vector2Int(8, 1), new Vector2Int(8, 5), new Vector2Int(11, 5)
                        }
                    }
                },

                // Nivel 2 — două rute care converg spre aceeași bază (11,3).
                new LevelDefinition
                {
                    index = 2,
                    healthMultiplier = 1.4f,
                    enemyColorA = new Color(0.3f, 0.8f, 0.95f),
                    enemyColorB = new Color(0.1f, 0.2f, 0.85f),
                    bossColor = new Color(0.1f, 0.15f, 0.5f),
                    routes = new List<Vector2Int[]>
                    {
                        new[] { new Vector2Int(0, 5), new Vector2Int(8, 5), new Vector2Int(8, 3), new Vector2Int(11, 3) },
                        new[] { new Vector2Int(0, 1), new Vector2Int(8, 1), new Vector2Int(8, 3), new Vector2Int(11, 3) }
                    }
                },

                // Nivel 3 — trei rute care converg spre baza (11,3).
                new LevelDefinition
                {
                    index = 3,
                    healthMultiplier = 1.9f,
                    enemyColorA = new Color(0.6f, 1f, 0.4f),
                    enemyColorB = new Color(0.1f, 0.5f, 0.15f),
                    bossColor = new Color(0.1f, 0.35f, 0.1f),
                    routes = new List<Vector2Int[]>
                    {
                        new[] { new Vector2Int(0, 6), new Vector2Int(6, 6), new Vector2Int(6, 3), new Vector2Int(11, 3) },
                        new[] { new Vector2Int(0, 3), new Vector2Int(11, 3) },
                        new[] { new Vector2Int(0, 0), new Vector2Int(6, 0), new Vector2Int(6, 3), new Vector2Int(11, 3) }
                    }
                }
            };
        }
    }
}
