using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeometryManager : MonoBehaviour
{
    public static GeometryManager instance;

    public List<Transform> points;
    [SerializeField] private LineRenderer lineRendererPrefab;
    private LineRenderer _lineRenderer;
    
    private const float Tolerance = 0.0001f;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        instance = this;
        points = new List<Transform>();
    }

    public void AddPoint(Transform p)
    {
        points.Add(p);
    }

    public void Clear() {
        for (int i = points.Count - 1; i >= 0; --i) {
            Destroy(points[i].gameObject);
            points.RemoveAt(i);
        }
        if(_lineRenderer != null) Destroy(_lineRenderer.gameObject);
    }

    public void RunJarvisMarch()
    {
        // polygone resultat
        List<Vector3> polygon = new List<Vector3>();

        /*
         * 0 - initialisation : Premier sommet de l'enveloppe convexe déterminé en considérant
         * une droite verticale s'appuyant sur le point le plus à gauche
         */
        Transform firstPoint = points[0];
        foreach (var point in points)
        {
            if (point.position.x < firstPoint.position.x)
            {
                firstPoint = point;
            }
        }

        /*
         * 1 - Faire tourner la droite autour du point dans le sens trigo jusqu'à ce qu'elle
         * contienne un autre point
         */
        int i0 = points.IndexOf(firstPoint);
        Vector3 direction = new Vector3(0, 0, 1);
        int i = i0;
        int j;
        float alpha, alphaMin;
        float lMax;
        int iNew;
        int n = points.Count;

        int crashHandler = 1000;
        do
        {
            // ajout du point pivot au polygone
            polygon.Add(points[i].position);

            // recherche du point suivant
            // initialisation de alphaMin et lMax avec le premier point d'indice différent de i
            if (i == 0) j = 1;
            else j = 0;
            Vector3 PiPj = points[j].position - points[i].position;
            alphaMin = Vector3.Angle(direction, PiPj); // vecteur PiPj
            lMax = PiPj.magnitude;
            iNew = j;

            // recherche du point le plus proche (en angle) de la droite

            for (j = iNew + 1; j < n; ++j)
            {
                if (j != i)
                {
                    PiPj = points[j].position - points[i].position;
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
            direction = points[iNew].position - points[i].position;
            direction.y = 0;
            i = iNew;


            --crashHandler;
        } while (i != i0 && crashHandler >= 0);

        polygon.Add(polygon[0]);

        if (_lineRenderer != null) Destroy(_lineRenderer.gameObject);
        _lineRenderer = Instantiate(lineRendererPrefab);
        _lineRenderer.positionCount = polygon.Count;
        _lineRenderer.SetPositions(polygon.ToArray());
    }

    public void RunGrahamScan()
    {
        List<Vector2> polygon = new List<Vector2>();
        foreach (var point in points)
        {
            Vector3 pointPos = point.position;
            polygon.Add(new Vector2(pointPos.x, pointPos.z));
        }

        // 1 - Calcul du barycentre
        Vector2 center = Vector2.zero;
        foreach (var point in polygon)
        {
            center += point;
        }
        center /= points.Count;

        // 2 - Tri des points Pi de points suivant l'angle orienté center, Pi
        polygon.Sort((p1, p2) =>
            Math.Sign(Vector2.SignedAngle(center, p2) - Vector2.SignedAngle(center, p1)));

        // 3 - Suppression des points non convexes du polygone
        int sommetInit = 0;
        int pivot = sommetInit;
        bool avance;
        
        int crashHandler = 1000;
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
            }

            --crashHandler;
        } while ((pivot != sommetInit || avance == false) && crashHandler >= 0);


        polygon.Add(polygon[0]);

        if (_lineRenderer != null) Destroy(_lineRenderer.gameObject);
        _lineRenderer = Instantiate(lineRendererPrefab);
        _lineRenderer.positionCount = polygon.Count;
        for (int i = polygon.Count - 1; i >= 0; --i)
        {
            _lineRenderer.SetPosition(i, new Vector3(polygon[i].x, 0, polygon[i].y));
        }
    }

    public void RunIncrementalTriangulation() {
        var pointsCloud = points.Select(point => point.position).ToArray();
        // 1 - tri par abscisse croissante
        var sorted = false;
        var pointsCount = pointsCloud.Length;
        while (!sorted) {
            sorted = true;
            for (int i = 0; i < pointsCount - 1; ++i) {
                if (pointsCloud[i].x > pointsCloud[i + 1].x
                    || pointsCloud[i].x > pointsCloud[i + 1].x + Tolerance && pointsCloud[i].y > pointsCloud[i + 1].y) {
                    sorted = false;
                    // swap
                    (pointsCloud[i], pointsCloud[i + 1]) = (pointsCloud[i + 1], pointsCloud[i]);
                }
            }
        }

        // resultat
        var result = new List<int>();
        int currentIndex = 0;
        
        // 2 - initialisation
        // a - on construit une suite de k - 1 aretes colineaires avec les k points alignés
        var alignedPoints = new List<Vector3>();
        float firstX = pointsCloud[0].x;
        alignedPoints.Add(pointsCloud[0]);
        for (int i = 1; i < pointsCount; ++i) {
            if (pointsCloud[i].x - firstX < Tolerance) {
                alignedPoints.Add(pointsCloud[i]);
            }
            else break; // we stop on first too far point
        }
        Debug.Log("Aligned points : " + alignedPoints.Count);
        // b - avec le premier point suivant à droite, trianguler
        if (alignedPoints.Count >= 2) {
            currentIndex = alignedPoints.Count;
            for (int i = 0; i < currentIndex; ++i) {
                if(result.Count == 0 || result[result.Count - 1] != currentIndex)
                    result.Add(currentIndex);
                result.Add(i);
                result.Add(i+1);
                result.Add(currentIndex);
            }
        }
        
        if (_lineRenderer != null) Destroy(_lineRenderer.gameObject);
        _lineRenderer = Instantiate(lineRendererPrefab);
        _lineRenderer.positionCount = result.Count;
        for (int i = result.Count - 1; i >= 0; --i)
        {
            _lineRenderer.SetPosition(i, pointsCloud[result[i]]);
        }
        

        // 3 - iterer sur les points restants et trianguler avec les aretes vues par chaque point
        for (int i = currentIndex; i < pointsCount; ++i) {
            // a - recherche des arretes vues par le point i
            //Vector3 
            
            // b - pour toute arrete vue, ajouter au resultat le triangle associe
        }
    }
}