using System.Collections.Generic;
using UnityEngine;

namespace Polyperfect.LowPolyAnimatedFantasyCreatures
{
    [System.Serializable]
    public struct SceneSwitcherButton
    {
        public Sprite iconSprite;
        public GameObject currentScene;
    }

    public class SceneSwitcher : MonoBehaviour
    {
        public List<GameObject> availableScenes;
        public List<SceneSwitcherButton> backgrounds = new List<SceneSwitcherButton>();
        public GameObject buttonPrefab;
        public Transform buttonParent;

        private List<SceneSwitchButton> constructed = new();

        private void Start()
        {
            bool first = true;

            backgrounds.ForEach(background =>
            {
                SceneSwitchButton button = Instantiate(buttonPrefab, buttonParent).GetComponent<SceneSwitchButton>();
                button.previewIcon.sprite = background.iconSprite;
                constructed.Add(button);

                button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
                {
                    constructed.ForEach(b => b.SetInactive());
                    button.SetActive();
                    
                    availableScenes.ForEach(scene => scene.SetActive(false));
                    background.currentScene.SetActive(true);
                });

                if (first)
                {
                    button.SetActive();
                    first = false;
                    availableScenes.ForEach(scene => scene.SetActive(false));
                    background.currentScene.SetActive(true);
                }
            });
        }
    }
}