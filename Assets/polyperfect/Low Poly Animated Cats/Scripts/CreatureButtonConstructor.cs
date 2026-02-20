using System.Collections.Generic;
using UnityEngine;

namespace Polyperfect.LowPolyAnimatedFantasyCreatures
{
    public class CreatureButtonConstructor : MonoBehaviour
    {
        public PodiumPreview podiumPreview;
        
        public GameObject buttonPrefab;
        public Transform buttonParent;
        public List<Creature> creatures = new();

        private void Start()
        {
            ConstructButtons();
        }

        private void ConstructButtons()
        {
            bool first = true;

            creatures.ForEach(creature =>
            {
                CreatureButton btn = Instantiate(buttonPrefab, buttonParent).GetComponent<CreatureButton>();
                btn.SetCreature(creature);

                if (podiumPreview)
                {
                    btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => podiumPreview.PreviewCreature(creature));

                    if (first)
                    {
                        podiumPreview.PreviewCreature(creature);
                        first = false;
                    }
                }
            });
        }
    }
}