using System.Collections;
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

    private void DrawPoints(Vector3[] pts) {
        if (_lineRenderer != null) Destroy(_lineRenderer.gameObject);
        _lineRenderer = Instantiate(lineRendererPrefab);
        _lineRenderer.positionCount = pts.Length;
        _lineRenderer.SetPositions(pts.ToArray());
    }

    public void RunJarvisMarch() {
        var polygon = GeometryUtils.RunJarvisMarch(points.Select(t => t.position).ToArray());
        DrawPoints(polygon);
    }

    public void RunGrahamScan() {
        var polygon = GeometryUtils.RunGrahamScan(points.Select(t => t.position).ToArray());
        DrawPoints(polygon);
    }

    public void RunIncrementalTriangulation() {
        StartCoroutine(RunIncrementalTriangulationCoroutine());
    }
    public IEnumerator RunIncrementalTriangulationCoroutine() {
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
        else {
            result.Add(2);
            result.Add(0);
            result.Add(1);
            result.Add(2);
            currentIndex = 3;
        }

        /*if (_lineRenderer != null) Destroy(_lineRenderer.gameObject);
        _lineRenderer = Instantiate(lineRendererPrefab);
        _lineRenderer.positionCount = result.Count;
        for (int i = result.Count - 1; i >= 0; --i)
        {
            _lineRenderer.SetPosition(i, pointsCloud[result[i]]);
        }*/
        
        // 3 - iterer sur les points restants et trianguler avec les aretes vues par chaque point
        for (int i = currentIndex; i < pointsCount; ++i) {
            // a - recherche des arretes vues par le point i
            var currentPolygon = GeometryUtils.RunJarvisMarch(pointsCloud.Take(i).ToArray());
            for (int j = currentPolygon.Length - 1; j > 0; --j) {
                Vector3 p1 = currentPolygon[j], p2 = currentPolygon[j - 1];
                Vector3 n = Vector3.Cross((p2 - p1).normalized, Vector3.down);
                Vector3 point = pointsCloud[i];
                float dot = Vector3.Dot((point - p1).normalized, n);
                if(dot > 0) {
                    // b - pour toute arrete vue, ajouter au resultat le triangle associe
                    if(result[result.Count - 1] != i)
                        result.Add(i);
                    result.Add(pointsCloud.ToList().IndexOf(currentPolygon[j]));
                    result.Add(pointsCloud.ToList().IndexOf(currentPolygon[j-1]));
                    result.Add(i);
                }
            }

            DrawPoints(result.Select(index => pointsCloud[index]).ToArray());
            //yield return null;
            //break;
            continue;
        }

        yield return null;
    }
}