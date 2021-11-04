using System;
using System.Collections.Generic;
using UnityEngine;

public class GeometryManager : MonoBehaviour
{
    public static GeometryManager instance;

    public List<Transform> points;
    [SerializeField] private LineRenderer lineRendererPrefab;
    private LineRenderer _lineRenderer;

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

        if(_lineRenderer != null) Destroy(_lineRenderer.gameObject);
        _lineRenderer = Instantiate(lineRendererPrefab);
        _lineRenderer.positionCount = polygon.Count;
        _lineRenderer.SetPositions(polygon.ToArray());
    }

    public void RunGrahamScan()
    {
        // 1 - Calcul du barycentre
        Vector3 center = Vector3.zero;
        foreach (var point in points)
        {
            center += point.position;
        }

        center /= points.Count;
        
        // 2 - Tri des points Pi de points suivant l'angle orienté center, Pi
        points.Sort((t1, t2) => (int) Mathf.Sign(Vector3.SignedAngle(center, t2.position, Vector3.up) - Vector3.SignedAngle(center, t1.position, Vector3.up)));

        List<Vector3> polygon = new List<Vector3>();
        foreach (var point in points)
        {
            polygon.Add(point.position);
        }
        polygon.Add(points[0].position);
        
        if(_lineRenderer != null) Destroy(_lineRenderer.gameObject);
        _lineRenderer = Instantiate(lineRendererPrefab);
        _lineRenderer.positionCount = polygon.Count;
        _lineRenderer.SetPositions(polygon.ToArray());
    }
}
