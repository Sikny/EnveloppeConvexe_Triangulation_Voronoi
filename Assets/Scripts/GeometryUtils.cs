using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GeometryUtils {
    private const float Tolerance = 0.0001f;

    [Serializable]
    public class Circle {
        public Vector3 center;
        public float radius;

        public bool Contains(Vector3 point) {
            return Vector3.Distance(center, point) < radius;
        }
    }

    #region CONVEX_HULL

    public static Vector3[] RunJarvisMarch(Vector3[] points) {
        // polygone resultat
        List<Vector3> polygon = new List<Vector3>();

        int n = points.Length;

        /*
         * 0 - initialisation : Premier sommet de l'enveloppe convexe déterminé en considérant
         * une droite verticale s'appuyant sur le point le plus à gauche
         */
        var firstPoint = points[0];
        int i0 = 0;
        for (var index = 0; index < n; index++) {
            var point = points[index];
            if (point.x < firstPoint.x) {
                firstPoint = point;
                i0 = index;
            }
        }

        /*
         * 1 - Faire tourner la droite autour du point dans le sens trigo jusqu'à ce qu'elle
         * contienne un autre point
         */
        Vector3 direction = new Vector3(0, 0, 1);
        int i = i0;
        int j;
        float alpha, alphaMin;
        float lMax;
        int iNew;

        int crashHandler = 1000;
        do {
            // ajout du point pivot au polygone
            polygon.Add(points[i]);

            // recherche du point suivant
            // initialisation de alphaMin et lMax avec le premier point d'indice différent de i
            if (i == 0) j = 1;
            else j = 0;
            Vector3 PiPj = points[j] - points[i];
            alphaMin = Vector3.Angle(direction, PiPj); // vecteur PiPj
            lMax = PiPj.magnitude;
            iNew = j;

            // recherche du point le plus proche (en angle) de la droite

            for (j = iNew + 1; j < n; ++j) {
                if (j != i) {
                    PiPj = points[j] - points[i];
                    alpha = Vector3.Angle(direction, PiPj);
                    if (alphaMin > alpha || Math.Abs(alphaMin - alpha) < 0.001f && lMax < PiPj.magnitude) {
                        alphaMin = alpha;
                        lMax = PiPj.magnitude;
                        iNew = j;
                    }
                }
            }

            // mise à jour du pivot et du vecteur directeur
            direction = points[iNew] - points[i];
            direction.y = 0;
            i = iNew;


            --crashHandler;
        } while (i != i0 && crashHandler >= 0);

        polygon.Add(polygon[0]);

        return polygon.ToArray();
    }

    public static Vector3[] RunGrahamScan(Vector3[] points) {
        List<Vector2> polygon = new List<Vector2>();
        foreach (var point in points) {
            Vector3 pointPos = point;
            polygon.Add(new Vector2(pointPos.x, pointPos.z));
        }

        // 1 - Calcul du barycentre
        Vector2 center = Vector2.zero;
        foreach (var point in polygon) {
            center += point;
        }

        center /= points.Length;

        // 2 - Tri des points Pi de points suivant l'angle orienté center, Pi
        polygon.Sort((p1, p2) =>
            Math.Sign(Vector2.SignedAngle(center, p2) - Vector2.SignedAngle(center, p1)));

        // 3 - Suppression des points non convexes du polygone
        int sommetInit = 0;
        int pivot = sommetInit;
        bool avance;

        int crashHandler = 1000;
        do {
            int previous = pivot - 1 < 0 ? polygon.Count - 1 : pivot - 1;
            int next = pivot + 1 > polygon.Count - 1 ? 0 : pivot + 1;
            float angle = Vector2.SignedAngle(polygon[next] - polygon[pivot], polygon[previous] - polygon[pivot]);
            if (angle > 180 || angle < 0) // si pivot convexe
            {
                pivot = pivot + 1;
                if (pivot > polygon.Count - 1) pivot = 0;
                avance = true;
            }
            else {
                sommetInit = pivot - 1;
                polygon.RemoveAt(pivot);
                if (sommetInit < 0) sommetInit = polygon.Count - 1;
                pivot = sommetInit;
                avance = false;
            }

            --crashHandler;
        } while ((pivot != sommetInit || avance == false) && crashHandler >= 0);


        polygon.Add(polygon[0]);

        return polygon.Select(vector2 => new Vector3(vector2.x, 0, vector2.y)).ToArray();
    }

    #endregion

    #region TRIANGULATION

    public static int[] RunIncrementalTriangulation(Vector3[] points) {
        // 1 - tri par abscisse croissante
        var sorted = false;
        var pointsCount = points.Length;
        while (!sorted) {
            sorted = true;
            for (int i = 0; i < pointsCount - 1; ++i) {
                if (points[i].x > points[i + 1].x
                    || points[i].x > points[i + 1].x + Tolerance && points[i].y > points[i + 1].y) {
                    sorted = false;
                    // swap
                    (points[i], points[i + 1]) = (points[i + 1], points[i]);
                }
            }
        }

        // resultat
        var result = new List<int>();
        int currentIndex = 0;

        // 2 - initialisation
        // a - on construit une suite de k - 1 aretes colineaires avec les k points alignés
        var alignedPoints = new List<Vector3>();
        float firstX = points[0].x;
        alignedPoints.Add(points[0]);
        for (int i = 1; i < pointsCount; ++i) {
            if (points[i].x - firstX < Tolerance) {
                alignedPoints.Add(points[i]);
            }
            else break; // we stop on first too far point
        }

        // b - avec le premier point suivant à droite, trianguler
        if (alignedPoints.Count >= 2) {
            currentIndex = alignedPoints.Count;
            for (int i = 0; i < currentIndex; ++i) {
                /*if(result.Count == 0 || result[result.Count - 1] != currentIndex)
                    result.Add(currentIndex);*/
                result.Add(i);
                result.Add(i + 1);
                result.Add(currentIndex);
            }
        }
        else {
            //result.Add(2);
            result.Add(0);
            result.Add(1);
            result.Add(2);
            currentIndex = 3;
        }

        // 3 - iterer sur les points restants et trianguler avec les aretes vues par chaque point
        for (int i = currentIndex; i < pointsCount; ++i) {
            // a - recherche des arretes vues par le point i
            var currentPolygon = GeometryUtils.RunJarvisMarch(points.Take(i).ToArray());
            for (int j = currentPolygon.Length - 1; j > 0; --j) {
                Vector3 p1 = currentPolygon[j], p2 = currentPolygon[j - 1];
                Vector3 n = Vector3.Cross((p2 - p1).normalized, Vector3.down);
                Vector3 point = points[i];
                float dot = Vector3.Dot((point - p1).normalized, n);
                if (dot > 0) {
                    // b - pour toute arrete vue, ajouter au resultat le triangle associe
                    /*if(result[result.Count - 1] != i)
                        result.Add(i);*/
                    result.Add(points.ToList().IndexOf(currentPolygon[j]));
                    result.Add(points.ToList().IndexOf(currentPolygon[j - 1]));
                    result.Add(i);
                }
            }
        }

        return result.ToArray();
    }

    public static Circle GetCircumcircle(Vector3 a, Vector3 b, Vector3 c) {
        float d = 2 * (a.x * (b.z - c.z) + b.x * (c.z - a.z) + c.x * (a.z - b.z));
        var result = new Circle {
            center = new Vector3(1 / d * ((a.x.Sqr() + a.z.Sqr()) * (b.z - c.z) + (b.x.Sqr() + b.z.Sqr()) * (c.z - a.z) + (c.x.Sqr() + c.z.Sqr()) * (a.z - b.z)),
                0, 1 / d * ((a.x.Sqr() + a.z.Sqr()) * (c.x - b.x) + (b.x.Sqr() + b.z.Sqr()) * (a.x - c.x) + (c.x.Sqr() + c.z.Sqr()) * (b.x - a.x))),
        };
        result.radius = Vector3.Distance(result.center, a);
        return result;
    }

    private static void EdgeFlipping(Vector3[] points, int[] trianglesCouple) {
        var triangle1 = new[] {
            trianglesCouple[0], trianglesCouple[1], trianglesCouple[2]
        };
        var triangle2 = new[] {
            trianglesCouple[3], trianglesCouple[4], trianglesCouple[5],
        };
        var pts = trianglesCouple.GroupBy(v => v).Select(obj => obj.Key).ToArray(); // should be size 4
        Debug.Log("Pts size : " + pts.Length);
        
        // edges
        var a1 = new[] { pts[0], pts[3] };
        var a2 = new[] { pts[0], pts[2] };
        var a3 = new[] { pts[2], pts[1] };
        var a4 = new[] { pts[1], pts[3] };

        var edges = new List<int[]> (new []{ a1, a2, a3, a4 });
        
        while (edges.Count > 0) {
            var currentEdge = edges.First();
            edges.Remove(currentEdge);
            
            // si currentEdge ne vérifie pas le critère de Delaunay, flip
            // 1 - get triangle concerned by edge
            int[] triangle, otherTriangle;
            if (triangle1.Contains(currentEdge[0]) && triangle1.Contains(currentEdge[1])) {
                triangle = triangle1;
                otherTriangle = triangle2;
            }
            else {
                triangle = triangle2;
                otherTriangle = triangle1;
            }
            Circle circumcircle = GetCircumcircle(points[triangle[0]], points[triangle[1]], points[triangle[2]]);
            if (circumcircle.Contains(points[triangle2[0]]) && circumcircle.Contains(points[triangle2[1]]) &&
                circumcircle.Contains(points[triangle2[2]])) {
                // flip
                Debug.Log("Flip");
            }
        }
    }

    public static int[] RunDelaunayTriangulation(Vector3[] points) {
        var triangles = RunIncrementalTriangulation(points);
        
        EdgeFlipping(points, triangles.Take(6).ToArray());
        
        return triangles;
    }
    #endregion
}