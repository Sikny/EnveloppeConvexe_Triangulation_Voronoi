using UnityEngine;
using UnityEngine.EventSystems;

public class PointDrawer : MonoBehaviour
{
    private enum DrawMode
    {
        TwoDimensions, ThreeDimensions
    }
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private DrawMode mode;
    private Camera _mainCam;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (mode == DrawMode.TwoDimensions)
            {
                Vector2 mousePos = Input.mousePosition;

                Vector3 point = _mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _mainCam.nearClipPlane));
                point.y = 0;
                var pointGo = Instantiate(pointPrefab, point, Quaternion.identity);
                GeometryManager.instance.AddPoint(pointGo.transform);
            }
            else
            {

            }
        }
    }
}
