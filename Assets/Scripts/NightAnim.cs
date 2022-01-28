using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightAnim : MonoBehaviour
{
    public System.Action blackScreenDone;

    public float fadeDuration = 2f;
    public SpriteRenderer nightSprite;


    private bool isFading = false;
    public void FadeToBlack()
    {
        if (isFading)
        {
            StopAllCoroutines();
        }
        Color color = nightSprite.color;
        color.a = 1;
        StartCoroutine(FadeTo(color));
    }
    public void FadeFromBlack()
    {
        if (isFading)
        {
            StopAllCoroutines();
        }
        Color color = nightSprite.color;
        color.a = 0;
        StartCoroutine(FadeTo(color));
    }
    IEnumerator FadeTo(Color end)
    {
        isFading = true;
        float timer = 0f;
        Color start = nightSprite.color;
        while (timer < fadeDuration)
        {
            nightSprite.color = Color.Lerp(start, end, timer / fadeDuration);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        nightSprite.color = end;
        isFading = false;
        blackScreenDone?.Invoke();
    }
}
