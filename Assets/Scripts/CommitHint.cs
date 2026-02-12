using UnityEngine;
using TMPro;
using System.Collections;

public class CommitHint : MonoBehaviour
{
    public static CommitHint Instance;

    public CanvasGroup canvasGroup;
    public float showTime = 0.2f;

    void Awake()
    {
        Instance = this;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    public void Show()
    {
        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        // 显示
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(showTime);

        // 立即消失（不搞动画，干净）
        canvasGroup.alpha = 0f;
    }
}