using System.Collections.Generic;
using System.Linq;
using Geometry;
using UnityEngine;

public class GeometryManager : MonoBehaviour
{
    public static GeometryManager instance;

    public List<Transform> points;
    [SerializeField] private LineRenderer lineRendererPrefab;
    private LineRenderer[] _lineRenderers;
    
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
        ClearLines();
    }

    private void ClearLines() {
        _lineRenderers = FindObjectsOfType<LineRenderer>();
        for (int i = _lineRenderers.Length - 1; i >= 0; --i) {
            Destroy(_lineRenderers[i].gameObject);
        }
    }

    protected void DrawLines(Vector3[] pts) {
        ClearLines();
        _lineRenderers = new[] {
            Instantiate(lineRendererPrefab)
        };
        _lineRenderers[0].positionCount = pts.Length;
        _lineRenderers[0].SetPositions(pts.ToArray());
    }

    protected void DrawTriangles(Vector3[] vertices, int[] triangles) {
        ClearLines();
        
        int triCount = triangles.Length / 3;
        _lineRenderers = new LineRenderer[triCount];
        int index = 0;
        for (int i = 0; i < triCount; ++i) {
            var lineRenderer = Instantiate(lineRendererPrefab);
            lineRenderer.positionCount = 4;
            lineRenderer.SetPosition(0, vertices[triangles[i * 3]]);
            lineRenderer.SetPosition(1, vertices[triangles[i * 3 + 1]]);
            lineRenderer.SetPosition(2, vertices[triangles[i * 3 + 2]]);
            lineRenderer.SetPosition(3, vertices[triangles[i * 3]]);
            _lineRenderers[index++] = lineRenderer;
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

    
    public Triangle[] trianglesInfo;
    public virtual void RunDelaunayTriangulation()
    {
        var positions = points.Select(point => point.position).ToArray();
        var result = GeometryUtils.RunDelaunayTriangulation(positions, out trianglesInfo);
        DrawTriangles(positions, result);
    }
}