using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Proiectil teleghidat: urmărește inamicul țintă și îi aplică daune la impact.
    /// Dacă ținta dispare înainte de impact, proiectilul se autodistruge.
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        Enemy target;
        float damage;
        float speed = 9f;

        public void Init(Enemy target, float damage, Color color)
        {
            this.target = target;
            this.damage = damage;

            var sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = TextureFactory.CreateCircle(color);
            sr.sortingOrder = 7;
            transform.localScale = Vector3.one * 0.25f;
        }

        void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 dir = target.transform.position - transform.position;
            float step = speed * Time.deltaTime;

            if (dir.magnitude <= step + 0.1f)
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            transform.position += dir.normalized * step;
        }
    }
}
