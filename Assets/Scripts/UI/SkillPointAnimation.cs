using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillPointAnimation : MonoBehaviour
{
    private float bounceDuration = 0.3f;
    private AnimationCurve bounceAnimationCurve;
    private bool isInCoroutine;

    public void setCurve(AnimationCurve curve)
    {
        bounceAnimationCurve = curve;
    }

    void Start()
    {
        isInCoroutine = false;
    }

    void Update()
    {
        if (!isInCoroutine)
        {
            StartCoroutine(BounceAnimation(this.transform));
        }
    }

    private IEnumerator BounceAnimation(Transform target)
    {
        isInCoroutine = true; 
        Vector3 originalScale = target.localScale; 
        float elapsedTime = 0f;

        while (elapsedTime < bounceDuration)
        {
            float progress = elapsedTime / bounceDuration;
            float scaleMultiplier = bounceAnimationCurve.Evaluate(progress) * 0.1f; 
            target.localScale = originalScale * (1f + scaleMultiplier); 

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.localScale = originalScale;
        isInCoroutine = false;
    }

}
