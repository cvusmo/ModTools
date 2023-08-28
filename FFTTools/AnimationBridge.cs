using UnityEngine;

namespace FFTTools
{ 
    [CreateAssetMenu(menuName = "AnimationBridge")]
    public class AnimationBridge : ScriptableObject
    {
        public Material targetMaterial;
        public float animationValue;

        private void OnValidate()
        {
            if (targetMaterial != null)
            {
                if (targetMaterial.HasProperty("_AnimationTime"))
                {
                    targetMaterial.SetFloat("_AnimationTime", animationValue);
                }
                else
                {
                    Debug.LogWarning("The target material does not have an _AnimationTime property.");
                }
            }
        }
    }
}