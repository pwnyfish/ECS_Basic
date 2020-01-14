//using NUnit.Framework.Internal;
using UnityEngine.XR.WSA;
using UnityEngine;

public class RTSCamControl : MonoBehaviour
{


    public Vector2 panLimit;

    private float panSpeed = 20f;
    private float panBorderThickness = 10f;
    private float zoomSpeed = 20f;
    private float scrollSpeed = 2f;
    private float minY = 0f;
    private float maxY = 300f;
    private Vector3 camPos = new Vector3(0f, 20f, -20f);
    private Quaternion camRot;

    Vector3 startPos;
    Vector3 endPos;
    // Use this for initialization
    void Start()
    {
        camRot = Quaternion.Euler(40f, 0f, 0f);
        transform.position = new Vector3(0f, 20f, -20f);
        transform.rotation = camRot;
        Camera.main.fieldOfView = 60;
    }


    // Update is called once per frame
    void Update()
    {

        Vector3 pos = transform.position;
        //Moving around the map with keyboard input
        //if (Input.GetKey("w") /*|| Input.mousePosition.y >= Screen.height - panBorderThickness*/)
        //{
        //    pos.z += panSpeed * Time.deltaTime;
        //}

        //if (Input.GetKey("s")/* || Input.mousePosition.y <= panBorderThickness*/)
        //{
        //    pos.z -= panSpeed * Time.deltaTime;
        //}


        //if (Input.GetKey("d") /*|| Input.mousePosition.x >= Screen.width - panBorderThickness*/)
        //{
        //    pos.x += panSpeed * Time.deltaTime;
        //}


        //if (Input.GetKey("a") /*|| Input.mousePosition.x <= panBorderThickness*/)
        //{
        //    pos.x -= panSpeed * Time.deltaTime;
        //}

        if (Input.GetMouseButtonDown(2))
        {
            startPos = Input.mousePosition;
        }

        //scroll across the map with holding the mousewheel
        if (Input.GetMouseButton(2))
        {
            pos.x += (Time.deltaTime * ((Input.mousePosition.x - startPos.x) * scrollSpeed));
            pos.z += (Time.deltaTime * ((Input.mousePosition.y - startPos.y) * scrollSpeed));
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * zoomSpeed * 100f * Time.deltaTime;

        //pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        //pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);


        transform.position = pos;
    }
}
