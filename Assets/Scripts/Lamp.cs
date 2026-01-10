// using UnityEngine;
//
// public class Lamp : MonoBehaviour
// {
//     public Light lampLight;
//     public Material lampMat;
//
//     private float intensity = 0f;
//
//     void Awake()
//     {
//         if (lampLight == null)
//             lampLight = GetComponent<Light>();
//
//         lampMat = GetComponent<Renderer>().material;
//         SetIntensity(intensity);
//     }
//
//     // 设置灯光亮度（Intensity）并更新材质发光
//     public void SetIntensity(float value)
//     {
//         intensity = value;
//         lampLight.intensity = intensity * 20f; // 5f是放大倍率，可调
//         lampMat.SetColor("_EmissionColor", Color.blue * intensity);
//     }
//
//     void OnMouseDown()
//     {
//         // 点击时通知 LampManager 弹出右侧面板
//         LampManager.Instance.SelectLamp(this);
//     }
// }

using UnityEngine;

public class Lamp : MonoBehaviour
{
    public Light lampLight;
    public Material lampMat;

    [Header("Highlight")]
    public GameObject highlight;   // 红色高亮物体

    private float intensity = 0f;

    void Awake()
    {
        if (lampLight == null)
            lampLight = GetComponent<Light>();

        lampMat = GetComponent<Renderer>().material;

        SetIntensity(intensity);
        SetHighlight(false); // 初始不选中
    }

    // ===== 设置亮度 =====
    public void SetIntensity(float value)
    {
        intensity = value;
        lampLight.intensity = intensity * 20f;
        lampMat.SetColor("_EmissionColor", Color.blue * intensity);
    }

    // ===== 控制高亮 =====
    public void SetHighlight(bool show)
    {
        if (highlight != null)
            highlight.SetActive(show);
    }

    // ===== 点击灯 =====
    void OnMouseDown()
    {
        LampManager.Instance.SelectLamp(this);
    }
}
