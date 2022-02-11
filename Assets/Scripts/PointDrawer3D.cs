using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointDrawer3D : MonoBehaviour
{
    [SerializeField] private GameObject pointPrefab;
    private Camera _mainCam;
    
    GeometryManager3D geometry;

    // Start is called before the first frame update
    void Start()
    {
        _mainCam = Camera.main;
        geometry = GeometryManager3D.instance;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray;
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.transform.gameObject.GetComponent<MeshCollider>())
                    {
                        var pointGo = Instantiate(pointPrefab, hit.point, Quaternion.identity);
                    }
                }
            }
        }

        foreach(var obj in FindObjectsOfType<GameObject>())
        {
            if(obj.name.Contains("Point") && !geometry.points.Contains(obj.transform)) 
                geometry.points.Add(obj.transform);
        }

        if (geometry.points.Count >= 4)
        {
            geometry.InitConvexHull();
        }
    }
}
