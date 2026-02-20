using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Polyperfect.LowPolyAnimatedFantasyCreatures
{
    public class AnimationButton : MonoBehaviour
    {
        public Text animationName;

        private static string FormatAnimationName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string[] words = System.Text.RegularExpressions.Regex.Split(input, @"(?<!^)(?=[A-Z])|_");

            string result = string.Join(" ", words.Select(word =>
                char.ToUpper(word[0]) + word.Substring(1).ToLower()));

            return result;
        }

        public void SetAnimation(string stateName)
        {
            animationName.text = FormatAnimationName(stateName);
        }
    }
}