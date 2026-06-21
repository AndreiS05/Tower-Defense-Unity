using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Definește un tip de tură: numele, costul, statisticile de luptă și aspectul.
    /// Folosit de magazin pentru a ști ce poate construi jucătorul.
    /// </summary>
    [System.Serializable]
    public class TurretBlueprint
    {
        public string name;
        public int cost;
        public float range;       // raza de acțiune (unități world)
        public float fireRate;    // proiectile pe secundă
        public float damage;      // daune pe proiectil
        public Color color;
        public Color bulletColor;

        public TurretBlueprint(string name, int cost, float range, float fireRate,
                               float damage, Color color, Color bulletColor)
        {
            this.name = name;
            this.cost = cost;
            this.range = range;
            this.fireRate = fireRate;
            this.damage = damage;
            this.color = color;
            this.bulletColor = bulletColor;
        }
    }
}
