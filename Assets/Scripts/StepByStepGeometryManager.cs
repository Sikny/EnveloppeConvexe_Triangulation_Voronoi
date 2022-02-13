using System.Linq;

public class StepByStepGeometryManager : GeometryManager {
    public override void RunJarvisMarch() {
        StartCoroutine(DelayedAlgorithms.RunJarvisMarch(points.Select(t => t.position).ToArray(), 0.25f, DrawPolyLine));
    }

    public override void RunGrahamScan() {
        StartCoroutine(DelayedAlgorithms.RunGrahamScan(points.Select(t => t.position).ToArray(), 0.25f, DrawPolyLine));
    }
}