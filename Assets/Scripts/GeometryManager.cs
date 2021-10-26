using System;
using System.Collections.Generic;
using UnityEngine;

public class GeometryManager : MonoBehaviour
{
    public static GeometryManager instance;

    public List<Transform> points;
    private List<LineRenderer> _lineRenderers;

    private void Awake()
    {
        if (instance != null) 
            Destroy(gameObject);
        instance = this;
        points = new List<Transform>();
        _lineRenderers = new List<LineRenderer>();
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
        
        GameObject lineRendererGo = new GameObject("LineRendererJarvis");
        LineRenderer lineRenderer = lineRendererGo.AddComponent<LineRenderer>();
        lineRenderer.positionCount = polygon.Count;
        lineRenderer.SetPositions(polygon.ToArray());
        lineRenderer.startWidth = lineRenderer.endWidth = 0.1f;
    }
}
