using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Traiectoria predefinită (lista de Waypoints) pe care o urmează inamicii,
    /// de la punctul de apariție până la punctul vital (baza).
    /// </summary>
    public class WaypointPath : MonoBehaviour
    {
        readonly List<Vector3> points = new List<Vector3>();

        public int Count => points.Count;

        /// <summary>Primul punct = locul de apariție al inamicilor.</summary>
        public Vector3 Start => points.Count > 0 ? points[0] : Vector3.zero;

        /// <summary>Ultimul punct = baza (punctul vital de protejat).</summary>
        public Vector3 End => points.Count > 0 ? points[points.Count - 1] : Vector3.zero;

        public void SetPoints(IEnumerable<Vector3> pts)
        {
            points.Clear();
            points.AddRange(pts);
        }

        public Vector3 GetPoint(int index)
        {
            return points[Mathf.Clamp(index, 0, points.Count - 1)];
        }

        public bool IsLast(int index) => index >= points.Count - 1;
    }
}
