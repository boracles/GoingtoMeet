using UnityEngine;

namespace Polyperfect.LowPolyAnimatedFantasyCreatures
{
    public class CameraControl : MonoBehaviour
    {
        public Camera cam;
        private float zoomLevel = 0;
        public float zoomSpeed = 8;
        public float zoomDistance = 0.5f;
        public float maxZoom = 6;
        public float minZoom = -2.5f;
        public bool zoomX = true;
        public bool zoomY = false;
        public bool zoomZ = true;

        private void Update()
        {
            Zoom();
        }

        private void Zoom()
        {
            transform.localPosition = new Vector3(
                Mathf.Lerp(transform.localPosition.x, zoomX ? zoomLevel : transform.localPosition.x, Time.deltaTime * zoomSpeed),
                Mathf.Lerp(transform.localPosition.y, zoomY ? zoomLevel : transform.localPosition.y, Time.deltaTime * zoomSpeed),
                Mathf.Lerp(transform.localPosition.z, zoomZ ? zoomLevel : transform.localPosition.z, Time.deltaTime * zoomSpeed));

            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                zoomLevel -= zoomDistance;
                if (zoomLevel < maxZoom)
                    zoomLevel = maxZoom;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                zoomLevel += zoomDistance;
                if (zoomLevel > minZoom)
                    zoomLevel = minZoom;
            }
        }
    }
}