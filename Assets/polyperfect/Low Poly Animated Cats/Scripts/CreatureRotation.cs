using UnityEngine;

namespace Polyperfect.LowPolyAnimatedFantasyCreatures
{
    public class CreatureRotation : MonoBehaviour
    {
        public float rotationSpeed = 20;
        private bool rotation = false;
        private float cursorPos;

        private void Update()
        {
            Rotate();
        }

        private void Rotate()
        {
            if (Input.GetMouseButtonUp(0))
            {
                rotation = false;
            }

            if (Input.GetMouseButtonDown(0))
            {
                cursorPos = Input.mousePosition.x;
                rotation = true;
            }

            if (rotation)
            {
                float newCursorPos = Input.mousePosition.x;
                float cursorDifference = newCursorPos - cursorPos;
                cursorPos = newCursorPos;

                transform.Rotate(Vector3.up, -cursorDifference * rotationSpeed * Time.deltaTime);
            }
        }
    }
}