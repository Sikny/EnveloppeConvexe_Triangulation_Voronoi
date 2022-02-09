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
        var positions = points.Select(point => point.position).ToArray();
        var result = GeometryUtils.RunIncrementalTriangulation(positions);
        DrawPoints(result.Select(index => positions[index]).ToArray());
    }
}