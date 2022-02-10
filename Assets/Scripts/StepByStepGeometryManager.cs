using System.Linq;

namespace DefaultNamespace {
    public class StepByStepGeometryManager : GeometryManager{
        public override void RunJarvisMarch() {
            StartCoroutine(DelayedAlgorithms.RunJarvisMarch(points.Select(t => t.position).ToArray(), 0.25f, DrawLines));
        }

        public override void RunGrahamScan() {
            StartCoroutine(DelayedAlgorithms.RunGrahamScan(points.Select(t => t.position).ToArray(), 0.25f, DrawLines));
        }
    }
}