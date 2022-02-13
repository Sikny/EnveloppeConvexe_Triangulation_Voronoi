using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GeometryManager : MonoBehaviour
{
    public static GeometryManager instance;

    public List<Transform> points;
    [SerializeField] private LineRenderer lineRendererPrefab;
    [SerializeField] private Toggle clearScreenOnDrawToggle;
    [SerializeField] private Color currentLineColor;
    [SerializeField] private Image currentLineColorImage;
    private LineRenderer[] _lineRenderers;
    
    private const float Tolerance = 0.0001f;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        instance = this;
        points = new List<Transform>();
    }

    public void OpenColorPicker() {
        ColorPicker.Create(currentLineColor, "Choose line color", SetColor, null);
    }

    private void SetColor(Color currentColor) {
        currentLineColor = currentColor;
        currentLineColorImage.color = currentColor;
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

    protected void DrawPolyLine(Vector3[] pts) {
        if(clearScreenOnDrawToggle.isOn)
            ClearLines();
        _lineRenderers = new[] {
            Instantiate(lineRendererPrefab)
        };
        _lineRenderers[0].positionCount = pts.Length;
        _lineRenderers[0].SetPositions(pts.ToArray());
        _lineRenderers[0].material.color = currentLineColor;
    }

    protected void DrawLines(List<Vector3[]> lines) {
        if(clearScreenOnDrawToggle.isOn)
            ClearLines();
        int lineCount = lines.Count;
        _lineRenderers = new LineRenderer[lineCount];
        for (int i = 0; i < lineCount; ++i) {
            var lineRenderer = Instantiate(lineRendererPrefab);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, lines[i][0]);
            lineRenderer.SetPosition(1, lines[i][1]);
            lineRenderer.material.color = currentLineColor;
            _lineRenderers[i] = lineRenderer;
        }
    }

    protected void DrawTriangles(Vector3[] vertices, int[] triangles) {
        if(clearScreenOnDrawToggle.isOn)
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
            lineRenderer.material.color = currentLineColor;
            _lineRenderers[index++] = lineRenderer;
        }
    }

    public virtual void RunJarvisMarch() {
        var polygon = GeometryUtils.RunJarvisMarch(points.Select(t => t.position).ToArray());
        DrawPolyLine(polygon);
    }

    public virtual void RunGrahamScan() {
        var polygon = GeometryUtils.RunGrahamScan(points.Select(t => t.position).ToArray());
        DrawPolyLine(polygon);
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
        var result = GeometryUtils.RunDelaunayTriangulation(positions, out _);
        DrawTriangles(positions, result);
    }

    public virtual void RunVoronoi() {
        var positions = points.Select(point => point.position).ToArray();
        var indices = GeometryUtils.RunDelaunayTriangulation(positions, out var triangles);
        var result = GeometryUtils.RunVoronoi(triangles, indices, positions);
        DrawLines(result);
    }
}