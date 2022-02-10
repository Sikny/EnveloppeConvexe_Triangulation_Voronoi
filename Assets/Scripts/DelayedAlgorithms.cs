using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DelayedAlgorithms {
    public static IEnumerator RunJarvisMarch(Vector3[] points, float delay, Action<Vector3[]> resultCallback) {
        var wait = new WaitForSeconds(delay);  
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
        
        do
        {
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

            for (j = iNew + 1; j < n; ++j)
            {
                if (j != i)
                {
                    PiPj = points[j] - points[i];
                    alpha = Vector3.Angle(direction, PiPj);
                    if (alphaMin > alpha || Math.Abs(alphaMin - alpha) < 0.001f && lMax < PiPj.magnitude)
                    {
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

            resultCallback(polygon.ToArray());
            yield return wait;
        } while (i != i0);

        polygon.Add(polygon[0]);

        resultCallback(polygon.ToArray());
        yield return null;
    }
    
    public static IEnumerator RunGrahamScan(Vector3[] points, float delay, Action<Vector3[]> resultCallback) {
        var wait = new WaitForSeconds(delay);
        List<Vector2> polygon = new List<Vector2>();
        foreach (var point in points)
        {
            Vector3 pointPos = point;
            polygon.Add(new Vector2(pointPos.x, pointPos.z));
        }
        resultCallback.Invoke(polygon.Select(vector2 => new Vector3(vector2.x, 0, vector2.y)).ToArray());
        yield return wait;

        // 1 - Calcul du barycentre
        Vector2 center = Vector2.zero;
        foreach (var point in polygon)
        {
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
        
        do
        {
            int previous = pivot - 1 < 0 ? polygon.Count - 1 : pivot - 1;
            int next = pivot + 1 > polygon.Count - 1 ? 0 : pivot + 1;
            float angle = Vector2.SignedAngle(polygon[next] - polygon[pivot], polygon[previous] - polygon[pivot]);
            if (angle > 180 || angle < 0)  // si pivot convexe
            {
                pivot = pivot + 1;
                if (pivot > polygon.Count - 1) pivot = 0;
                avance = true;
            }
            else
            {
                sommetInit = pivot - 1;
                polygon.RemoveAt(pivot);
                if (sommetInit < 0) sommetInit = polygon.Count - 1;
                pivot = sommetInit;
                avance = false;
                resultCallback.Invoke(polygon.Select(vector2 => new Vector3(vector2.x, 0, vector2.y)).ToArray());
                yield return wait;
            }
        } while (pivot != sommetInit || avance == false);


        polygon.Add(polygon[0]);

        resultCallback.Invoke(polygon.Select(vector2 => new Vector3(vector2.x, 0, vector2.y)).ToArray());
        yield return null;
    }
}