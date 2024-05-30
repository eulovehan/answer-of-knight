using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class text : MonoBehaviour
{
    public Image imageUI;  // 점점 흐려질 이미지
    public TextMeshProUGUI textUI;  // 점점 흐려질 텍스트

    private float defaultAlpha = 174f;  // 이미지의 기본 알파값

    // 텍스트 실행
    public void TextView(string text = "", float delayDuration = 6f, float fadeDuration = 1.0f)
    {
        textUI.text = text;
        StartCoroutine(StartFadeAfterDelay(delayDuration, fadeDuration));
    }
        
    // 지연시간
    IEnumerator StartFadeAfterDelay(float delayDuration = 6f, float fadeDuration = 1.0f)
    {
        StartCoroutine(FadeIn(fadeDuration / 2f));

        yield return new WaitForSeconds(delayDuration + fadeDuration);  // 지연 시간 대기
        StartCoroutine(FadeOut(fadeDuration));
    }

    // 페이드인
    IEnumerator FadeIn(float fadeDuration = 0.5f)
    {
        Color originalColor = imageUI.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, defaultAlpha / 255f, elapsedTime / fadeDuration);
            imageUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            textUI.color = new Color(textUI.color.r, textUI.color.g, textUI.color.b, alpha);

            yield return null;
        }

        imageUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, defaultAlpha / 255f);  // 완전히 불투명하게 설정
    }
    
    // 페이드아웃
    IEnumerator FadeOut(float fadeDuration = 1.0f)
    {
        Color originalColor = imageUI.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(defaultAlpha / 255f, 0f, elapsedTime / fadeDuration);
            imageUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            textUI.color = new Color(textUI.color.r, textUI.color.g, textUI.color.b, alpha);

            yield return null;
        }

        imageUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);  // 완전히 투명하게 설정
    }
}