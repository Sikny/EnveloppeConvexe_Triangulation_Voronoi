using UnityEngine;

public class RandomPointsDrawer : MonoBehaviour {
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private Transform startPos;
    [SerializeField] private Transform endPos;
    [SerializeField] private int spawnCount;

    public void GeneratePoints() {
        GeometryManager.instance.Clear();

        var startPoint = endPos.position;
        var endPoint = startPos.position;
        for (int i = 0; i < spawnCount; ++i) {
            var pos = new Vector3(Random.Range(endPoint.x, startPoint.x), 0,
                Random.Range(endPoint.z, startPoint.z));
            var pointGo = Instantiate(pointPrefab, pos, Quaternion.identity);
            GeometryManager.instance.AddPoint(pointGo.transform);
        }
    }
}
