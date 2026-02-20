using UnityEngine;

namespace Polyperfect.LowPolyAnimatedFantasyCreatures
{
    [CreateAssetMenu(fileName = "Creature", menuName = "Polyperfect/New Creature", order = 1)]
    public class Creature : ScriptableObject
    {
        public string DisplayName;
        public GameObject modelPrefab;
        public Sprite previewImage;
        public RuntimeAnimatorController overrideAnimator;
        public float scale;
    }
}