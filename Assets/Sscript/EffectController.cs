using System.Collections;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Awake()
    {
        // 尝试获取 SpriteRenderer 组件
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 如果找不到 SpriteRenderer，打印错误日志
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on " + gameObject.name);
        }
    }

    public void StartEffect(Sprite image, float brightnessDuration, float fadeDuration)
    {
        StartCoroutine(ChangeImage(image, brightnessDuration, fadeDuration));
    }

    private IEnumerator ChangeImage(Sprite image, float brightnessDuration, float fadeDuration)
    {
        // 确保 spriteRenderer 已经被正确初始化
        if (spriteRenderer == null)
        {
            yield break; // 结束协程
        }

        Color originalColor = spriteRenderer.color;
        spriteRenderer.sprite = image;
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1);
        
        Vector3 originalScale = spriteRenderer.transform.localScale;
        Vector3 targetScale = originalScale * 2;

        // Gradually increase the brightness
        for (float t = 0; t < brightnessDuration; t += Time.deltaTime)
        {
            float factor = Mathf.Lerp(1, 3f, t / brightnessDuration); // Adjust brightness factor
            float scale = Mathf.Lerp(1, 2, t / brightnessDuration);
            spriteRenderer.color = new Color(originalColor.r * factor, originalColor.g * factor, originalColor.b * factor, 1);
            spriteRenderer.transform.localScale = originalScale * scale;
            yield return null;
        }

        // Gradually decrease the alpha to make it transparent
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(1, 0, t / fadeDuration); // Adjust alpha factor
            float scale = Mathf.Lerp(2, 0, t / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r * 1.5f, originalColor.g * 1.5f, originalColor.b * 1.5f, alpha);
            spriteRenderer.transform.localScale = originalScale * scale;
            yield return null;
        }

        // Ensure the final state is fully transparent
        spriteRenderer.color = new Color(originalColor.r * 1.5f, originalColor.g * 1.5f, originalColor.b * 1.5f, 0);
    }
}

