using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

    private readonly Vector3 originalPos = new Vector3(9.5f, 9.7f, -10);
    private readonly float originalZoom = 11;



    private bool isMiddleMouseDown;
    private float zoom
    {
        get { return this.GetComponent<Camera>().orthographicSize; }
        set { this.GetComponent<Camera>().orthographicSize = value; }
    }


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(2))
            isMiddleMouseDown = true;
        if (Input.GetMouseButtonUp(2))
            isMiddleMouseDown = false;

        if (isMiddleMouseDown)
        {
            //bigger the zoom, smaller the movement
            float x = -Input.GetAxis("Mouse X") * zoom / 10;
            float y = -Input.GetAxis("Mouse Y") * zoom / 10;
            this.transform.position += new Vector3(x, y, 0);
        }


        var d = Input.GetAxis("Mouse ScrollWheel");
        this.zoom = Mathf.Max(zoom - 2f * d, 1);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.transform.position = originalPos;
            this.zoom = originalZoom;
        }

    }
}
