using UnityEngine;
using UnityEngine.UI;

namespace Polyperfect.LowPolyAnimatedFantasyCreatures
{
    public class CreatureButton : MonoBehaviour
    {
        public Text creatureName;
        public Image creatureIcon;
        private Creature creature;

        public void SetCreature(Creature creature)
        {
            this.creature = creature;
            creatureName.text = creature.DisplayName;
            creatureIcon.sprite = creature.previewImage;
        }
    }
}