using System;
using UnityEngine;

namespace Geometry {
    [Serializable]
    public class Circle {
        public Vector3 center;
        public float radius;

        public bool Contains(Vector3 point) {
            return Vector3.Distance(center, point) < radius;
        }

        public static Circle Circumcircle(Vector3 a, Vector3 b, Vector3 c) {
            float d = 2 * (a.x * (b.z - c.z) + b.x * (c.z - a.z) + c.x * (a.z - b.z));
            var result = new Circle {
                center = new Vector3(1 / d * ((a.x.Sqr() + a.z.Sqr()) * (b.z - c.z) + (b.x.Sqr() + b.z.Sqr()) * (c.z - a.z) + (c.x.Sqr() + c.z.Sqr()) * (a.z - b.z)),
                    0, 1 / d * ((a.x.Sqr() + a.z.Sqr()) * (c.x - b.x) + (b.x.Sqr() + b.z.Sqr()) * (a.x - c.x) + (c.x.Sqr() + c.z.Sqr()) * (b.x - a.x))),
            };
            result.radius = Vector3.Distance(result.center, a);
            return result;
        }

        public static Circle Circumcircle(Vector3[] vertices, Triangle triangleData) {
            var pts = triangleData.Points;
            return Circumcircle(vertices[pts[0]], vertices[pts[1]], vertices[pts[2]]);
        }
    }
}