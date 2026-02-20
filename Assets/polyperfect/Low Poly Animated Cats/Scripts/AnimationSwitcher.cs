using UnityEngine;

namespace Polyperfect.LowPolyAnimatedFantasyCreatures
{
    public class AnimationSwitcher : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private string currentAnimation;
        public void SwitchTo(string animation) {
            if(currentAnimation != null)
                animator.SetBool(currentAnimation, false);
            if(currentAnimation == animation)
                animator.SetBool(animation, false);
            else
                animator.SetBool(animation, true);
            currentAnimation = animation;
        
        }
        public void Trigger(string animation){
            animator.SetTrigger(animation);
        }
        public void Reset() {
            if(currentAnimation != null){
                animator.SetBool(currentAnimation, false);
                currentAnimation = null;
            }
            animator.Play("idle");
        }

        public void SetAnimator(Animator _animator){
            animator = _animator;
        }
    }
}
