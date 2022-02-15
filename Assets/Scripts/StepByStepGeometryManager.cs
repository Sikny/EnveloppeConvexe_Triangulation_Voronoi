using System.Linq;

public class StepByStepGeometryManager : GeometryManager {
    public override void RunJarvisMarch() {
        StartCoroutine(DelayedAlgorithms.RunJarvisMarch(points.Select(t => t.position).ToArray(), 0.25f, DrawPolyLine));
    }

    public override void RunGrahamScan() {
        StartCoroutine(DelayedAlgorithms.RunGrahamScan(points.Select(t => t.position).ToArray(), 0.25f, DrawPolyLine));
    }

    public override void RunIncrementalTriangulation() {
        var positions = points.Select(t => t.position).ToArray();
        StartCoroutine(DelayedAlgorithms.RunIncrementalTriangulation(positions, 0.25f,
            result => DrawTriangles(positions, result)));
    }

    public override void RunDelaunayTriangulation() {
        var positions = points.Select(t => t.position).ToArray();
        StartCoroutine(DelayedAlgorithms.RunDelaunayTriangulation(positions, 0.25f, (indices, triangles) => {
            DrawTriangles(positions, indices);
        }));
    }

    public override void RunVoronoi() {
        var positions = points.Select(point => point.position).ToArray();
        var indices = GeometryUtils.RunDelaunayTriangulation(positions, out var triangles);
        StartCoroutine(DelayedAlgorithms.RunVoronoi(triangles, indices, positions, 0.25f, list => {
            ClearLines();
            DrawTriangles(positions, indices);
            DrawLines(list);
        }));
    }
}