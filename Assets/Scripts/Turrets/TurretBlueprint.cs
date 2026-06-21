using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Definește un tip de tură: numele, costul, statisticile de luptă, aspectul,
    /// nivelul la care se deblochează și eventuale efecte speciale (splash AoE,
    /// lovire de ținte multiple). Folosit de magazin pentru a ști ce poate construi jucătorul.
    /// </summary>
    [System.Serializable]
    public class TurretBlueprint
    {
        public string name;
        public int cost;
        public float range;        // raza de acțiune (unități world)
        public float fireRate;     // proiectile pe secundă
        public float damage;       // daune pe proiectil
        public Color color;
        public Color bulletColor;

        public int unlockLevel;    // nivelul de la care e disponibilă în magazin
        public float splashRadius; // > 0 = daune de zonă (AoE) la impact
        public int maxTargets;     // câte ținte lovește simultan (1 = normal)

        public TurretBlueprint(string name, int cost, float range, float fireRate, float damage,
                               Color color, Color bulletColor, int unlockLevel = 1,
                               float splashRadius = 0f, int maxTargets = 1)
        {
            this.name = name;
            this.cost = cost;
            this.range = range;
            this.fireRate = fireRate;
            this.damage = damage;
            this.color = color;
            this.bulletColor = bulletColor;
            this.unlockLevel = unlockLevel;
            this.splashRadius = splashRadius;
            this.maxTargets = Mathf.Max(1, maxTargets);
        }
    }
}
