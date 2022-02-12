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

    protected void DrawLines(Vector3[] pts) {
        if (_lineRenderer != null) Destroy(_lineRenderer.gameObject);
        _lineRenderer = Instantiate(lineRendererPrefab);
        _lineRenderer.positionCount = pts.Length;
        _lineRenderer.SetPositions(pts.ToArray());
    }

    protected void DrawTriangles(Vector3[] vertices, int[] triangles) {
        if(_lineRenderer != null) Destroy(_lineRenderer.gameObject);
        _lineRenderer = Instantiate(lineRendererPrefab);
        int triCount = triangles.Length / 3;
        _lineRenderer.positionCount = triCount * 4;
        for (int i = 0; i < triCount; ++i) {
            _lineRenderer.SetPosition(i * 4, vertices[triangles[i * 3 + 2]]);
            _lineRenderer.SetPosition(i * 4 + 1, vertices[triangles[i * 3]]);
            _lineRenderer.SetPosition(i * 4 + 2, vertices[triangles[i * 3 + 1]]);
            _lineRenderer.SetPosition(i * 4 + 3, vertices[triangles[i * 3 + 2]]);
        }
    }

    public virtual void RunJarvisMarch() {
        var polygon = GeometryUtils.RunJarvisMarch(points.Select(t => t.position).ToArray());
        DrawLines(polygon);
    }

    public virtual void RunGrahamScan() {
        var polygon = GeometryUtils.RunGrahamScan(points.Select(t => t.position).ToArray());
        DrawLines(polygon);
    }

    public virtual void RunIncrementalTriangulation() {
        var positions = points.Select(point => point.position).ToArray();
        var result = GeometryUtils.RunIncrementalTriangulation(positions);
        //DrawLines(result.Select(index => positions[index]).ToArray());
        DrawTriangles(positions, result);
    }

    public virtual void RunDelaunayTriangulation()
    {
        var positions = points.Select(point => point.position).ToArray();
        var result = GeometryUtils.RunDelaunayTriangulation(positions);
        DrawTriangles(positions, result);
    }
}