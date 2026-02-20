using UnityEngine;
using UnityEngine.UI;

namespace Polyperfect.LowPolyAnimatedFantasyCreatures
{
    public class SceneSwitchButton : MonoBehaviour
    {
        public Image previewIcon;
        public Image borderIcon;
        public Sprite activeBorder;
        public Sprite inactiveBorder;
        public float activeScale = 1.2f;
        public float inactiveScale = 1f;

        public void SetActive()
        {
            transform.localScale = Vector3.one * activeScale;
            borderIcon.sprite = activeBorder;
        }

        public void SetInactive()
        {
            transform.localScale = Vector3.one * inactiveScale;
            borderIcon.sprite = inactiveBorder;
        }
    }
}
