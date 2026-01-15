// using UnityEngine;
//
// public class Lamp : MonoBehaviour
// {
//     public Light lampLight;
//     public Material lampMat;
//
//     [Header("Highlight")]
//     public GameObject highlight;   // 红色高亮物体
//
//     private float intensity = 0f;
//
//     void Awake()
//     {
//         if (lampLight == null)
//             lampLight = GetComponent<Light>();
//
//         lampMat = GetComponent<Renderer>().material;
//
//         SetIntensity(intensity);
//         SetHighlight(false); // 初始不选中
//     }
//
//     // ===== 设置亮度 =====
//     public void SetIntensity(float value)
//     {
//         intensity = value;
//         lampLight.intensity = intensity * 20f;
//         lampMat.SetColor("_EmissionColor", Color.blue * intensity);
//     }
//
//     // ===== 控制高亮 =====
//     public void SetHighlight(bool show)
//     {
//         if (highlight != null)
//             highlight.SetActive(show);
//     }
//
//     // ===== 点击灯 =====
//     void OnMouseDown()
//     {
//         LampManager.Instance.SelectLamp(this);
//     }
// }

using UnityEngine;

public class Lamp : MonoBehaviour
{
    public Light lampLight;
    public Material lampMat;

    [Header("Highlight")]
    public GameObject highlight;

    [Header("RGBW (0–100)")]
    [Range(0, 100)] public float R;
    [Range(0, 100)] public float G;
    [Range(0, 100)] public float B;
    [Range(0, 100)] public float W;

    void Awake()
    {
        if (lampLight == null)
            lampLight = GetComponent<Light>();

        lampMat = GetComponent<Renderer>().material;

        ApplyLight();
        SetHighlight(false);
    }

    // ===== RGBW 设置 =====
    public void SetR(float v) { R = Mathf.Clamp(v, 0, 100); ApplyLight(); }
    public void SetG(float v) { G = Mathf.Clamp(v, 0, 100); ApplyLight(); }
    public void SetB(float v) { B = Mathf.Clamp(v, 0, 100); ApplyLight(); }
    public void SetW(float v) { W = Mathf.Clamp(v, 0, 100); ApplyLight(); }

    // ===== 混光核心 =====
    void ApplyLight()
    {
        Color rgb = new Color(R / 100f, G / 100f, B / 100f);
        float white = W / 100f;

        Color finalColor = rgb + Color.white * white;
        finalColor.a = 1f;

        lampLight.color = finalColor;
        lampLight.intensity = Mathf.Max(R, G, B, W) * 0.2f;

        lampMat.SetColor("_EmissionColor", finalColor);
    }

    // ===== 高亮 =====
    public void SetHighlight(bool show)
    {
        if (highlight != null)
            highlight.SetActive(show);
    }

    // ===== 点击 =====
    void OnMouseDown()
    {
        LampManager.Instance.SelectLamp(this);
    }
}



