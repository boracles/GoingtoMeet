using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace Polyperfect.LowPolyAnimatedFantasyCreatures
{
    public class AnimationButtonConstructor : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public Transform buttonParent;
        public float animationTransitionTime = 0.3f;
        private Animator creatureAnimator;
        private List<Transform> constructed = new();
        private List<string> creatureAnimatorStates = new();

#if UNITY_EDITOR
        private void ExtractNonEmptyStatesFromStateMachine(AnimatorStateMachine stateMachine, Dictionary<AnimationClip, AnimationClip> overrides, List<string> stateNames)
        {
            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                AnimationClip clip = childState.state.motion as AnimationClip;
                if (clip != null)
                {
                    if (overrides.TryGetValue(clip, out AnimationClip overrideClip))
                        clip = overrideClip;

                    if (clip != null && !string.IsNullOrEmpty(clip.name))
                        stateNames.Add(childState.state.name);
                }
            }

            foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
                ExtractNonEmptyStatesFromStateMachine(childStateMachine.stateMachine, overrides, stateNames);
           
        } 

        private void ExtractStatesFromStateMachine(AnimatorStateMachine stateMachine, List<string> stateList)
        {
            foreach (ChildAnimatorState childState in stateMachine.states)
                stateList.Add(childState.state.name);

            foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
                ExtractStatesFromStateMachine(childStateMachine.stateMachine, stateList);
        }
#endif

        public void LoadCreatureAnimations(Animator animator)
        {
#if UNITY_EDITOR
            if (animator == null)
            {
                creatureAnimatorStates.Clear();
                return;
            }

            if (animator.runtimeAnimatorController is AnimatorController controller)
            {
                List<string> allStates = new();

                foreach (AnimatorControllerLayer layer in controller.layers)
                    ExtractStatesFromStateMachine(layer.stateMachine, allStates);

                creatureAnimatorStates = allStates;
            }
            else if (animator.runtimeAnimatorController is AnimatorOverrideController overrideController)
            {
                RuntimeAnimatorController baseController = overrideController.runtimeAnimatorController;
                if (baseController == null)
                {
                    Debug.LogError("The override controller doesn't have a valid base controller.");
                    return;
                }

                AnimatorController animatorController = baseController as AnimatorController;
                if (animatorController == null)
                {
                    Debug.LogError("The base controller is not an AnimatorController.");
                    return;
                }

                List<KeyValuePair<AnimationClip, AnimationClip>> overrideList = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                overrideController.GetOverrides(overrideList);
                Dictionary<AnimationClip, AnimationClip> overrides = overrideList.ToDictionary(pair => pair.Key, pair => pair.Value);
                List<string> nonEmptyStateNames = new();

                foreach (AnimatorControllerLayer layer in animatorController.layers)
                    ExtractNonEmptyStatesFromStateMachine(layer.stateMachine, overrides, nonEmptyStateNames);

                creatureAnimatorStates = nonEmptyStateNames;
            }
            
            creatureAnimator = animator;
#endif
        }

        public void ConstructButtons()
        {
#if UNITY_EDITOR
            constructed.ForEach(obj => { Destroy(obj.gameObject); });
            constructed.Clear();

            foreach (var state in creatureAnimatorStates)
            {
                AnimationButton btn = Instantiate(buttonPrefab, buttonParent).GetComponent<AnimationButton>();
                btn.SetAnimation(state);
                btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
                    creatureAnimator.CrossFadeInFixedTime(state, animationTransitionTime);
                });
                constructed.Add(btn.transform);
            }
            
#endif
        }
    }
}
