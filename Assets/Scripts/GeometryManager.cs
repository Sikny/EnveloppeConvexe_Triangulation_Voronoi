using System.Collections.Generic;
using System.Linq;
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

    public void Clear() {
        for (int i = points.Count - 1; i >= 0; --i) {
            Destroy(points[i].gameObject);
            points.RemoveAt(i);
        }
        if(_lineRenderer != null) Destroy(_lineRenderer.gameObject);
    }

    private void DrawLines(Vector3[] pts) {
        if (_lineRenderer != null) Destroy(_lineRenderer.gameObject);
        _lineRenderer = Instantiate(lineRendererPrefab);
        _lineRenderer.positionCount = pts.Length;
        _lineRenderer.SetPositions(pts.ToArray());
    }

    public void RunJarvisMarch() {
        var polygon = GeometryUtils.RunJarvisMarch(points.Select(t => t.position).ToArray());
        DrawLines(polygon);
    }

    public void RunGrahamScan() {
        var polygon = GeometryUtils.RunGrahamScan(points.Select(t => t.position).ToArray());
        DrawLines(polygon);
    }

    private void DrawTriangles(Vector3[] positions, int[] triangles) {
        if(_lineRenderer != null) Destroy(_lineRenderer.gameObject);
        _lineRenderer = Instantiate(lineRendererPrefab);
        int trianglesCount = triangles.Length / 3;
        _lineRenderer.positionCount = trianglesCount * 4;
        void SetPoint(int index) {
            _lineRenderer.SetPosition(index, positions[triangles[index]]);
        }
        for (int i = 0; i < trianglesCount; ++i) {
            SetPoint(i * 3 + 2);
            SetPoint(i * 3);
            SetPoint(i * 3 + 1);
            SetPoint(i * 3 + 2);
        }
    }

    public void RunIncrementalTriangulation() {
        var pointsCloud = points.Select(point => point.position).ToArray();
        var result = GeometryUtils.RunIncrementalTriangulation(pointsCloud);
        DrawTriangles(pointsCloud, result);
    }
}