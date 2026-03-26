using UnityEngine;

public class UI_SidePanel : MonoBehaviour
{
    public RectTransform panel;
    public float speed = 10f;

    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private bool isOpen = false;

    void Start()
    {
        shownPos = new Vector2(0, panel.anchoredPosition.y);
        hiddenPos = new Vector2(-panel.rect.width, panel.anchoredPosition.y);

        panel.anchoredPosition = hiddenPos;
    }

    void Update()
    {
        Vector2 target = isOpen ? shownPos : hiddenPos;

        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            target,
            Time.deltaTime * speed
        );
    }

    public void TogglePanel()
    {
        isOpen = !isOpen;
    }
}
