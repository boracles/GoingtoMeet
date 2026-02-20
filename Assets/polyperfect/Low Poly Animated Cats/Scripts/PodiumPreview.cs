using UnityEngine;

namespace Polyperfect.LowPolyAnimatedFantasyCreatures
{
    public class PodiumPreview : MonoBehaviour
    {
        public Transform spawnTransform;
        public AnimationButtonConstructor animationButtonConstructor;
        public float animalRotationOffset = 0;

        public void PreviewCreature(Creature creature)
        {
            if (spawnTransform.childCount > 0)
            {
                Destroy(spawnTransform.GetChild(0).gameObject);
            }

            GameObject creatureObject = Instantiate(creature.modelPrefab, spawnTransform);
            creatureObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, animalRotationOffset, 0f));
            creatureObject.transform.localScale = new Vector3(creature.scale, creature.scale, creature.scale);

            if (animationButtonConstructor)
            {
                if (creatureObject.TryGetComponent(out Animator anim))
                {
                    if (creature.overrideAnimator)
                        anim.runtimeAnimatorController = creature.overrideAnimator;

                    animationButtonConstructor.LoadCreatureAnimations(anim);
                }
                else
                {
                    animationButtonConstructor.LoadCreatureAnimations(null);
                }
                animationButtonConstructor.ConstructButtons();
            }
        }
    }
}