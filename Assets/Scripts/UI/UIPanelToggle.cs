using UnityEngine;

public class UIPanelToggle : MonoBehaviour
{
    public GameObject panel;

    private bool isOpen = false;

    void Start()
    {
        panel.SetActive(isOpen);

    }
    public void TogglePanel()
    {
        isOpen = !isOpen;
        panel.SetActive(isOpen);
    }
}
