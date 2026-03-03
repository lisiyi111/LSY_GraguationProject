using UnityEngine;


public class Lamp : MonoBehaviour
{
    public Light lampLight;
    public Material lampMat;

    [Header("Group ID (1-31)")]
    public int groupId;
    [Header("Lamp Index In Group (0-15)")]
    public int lampIndex;   // 该灯在本组中的编号，从 0 开始

    [Header("Camera Config")]
    public bool hasCamera = false;   // ← 是否有摄像头（在 Inspector 里勾选）
    public bool cameraOn = false;    // ← 当前摄像头状态
    
    [Header("Highlight")]
    public GameObject highlight;

    // ===== 第一组：外部实时灯 =====
    [Header("Group 1 (Display)")]
    [Range(0, 100)] public float R;
    [Range(0, 100)] public float G;
    [Range(0, 100)] public float B;
    [Range(0, 100)] public float W;

    // ===== 第二组：内部灯 / 编码参数（不显示）=====
    [Header("Group 2 (Internal / Hidden)")]
    [Range(0, 100)] public float R2;
    [Range(0, 100)] public float G2;
    [Range(0, 100)] public float B2;
    [Range(0, 100)] public float W2;

    void Awake()
    {
        
        if (lampLight == null)
            lampLight = GetComponent<Light>();

        lampMat = GetComponent<Renderer>().material;

        ApplyLight();
        SetHighlight(false);
    }

    // ===== 第一组设置（实时显示）=====
    public void SetR(float v) { R = Mathf.Clamp(v, 0, 100); ApplyLight(); }
    public void SetG(float v) { G = Mathf.Clamp(v, 0, 100); ApplyLight(); }
    public void SetB(float v) { B = Mathf.Clamp(v, 0, 100); ApplyLight(); }
    public void SetW(float v) { W = Mathf.Clamp(v, 0, 100); ApplyLight(); }

    // ===== 第二组设置（只存数值，不显示）=====
    public void SetR2(float v) { R2 = Mathf.Clamp(v, 0, 100); }
    public void SetG2(float v) { G2 = Mathf.Clamp(v, 0, 100); }
    public void SetB2(float v) { B2 = Mathf.Clamp(v, 0, 100); }
    public void SetW2(float v) { W2 = Mathf.Clamp(v, 0, 100); }

    // ===== 混光只使用第一组 =====
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
    
    
    public void SetCameraState(bool on)
    {
        if (!hasCamera)
        {
            cameraOn = false;   // 没有摄像头，永远关闭
            return;
        }

        cameraOn = on;
    }
    
    public void ResetAllData()
    {
        // 第一组
        R = 0;
        G = 0;
        B = 0;
        W = 0;

        // 第二组
        R2 = 0;
        G2 = 0;
        B2 = 0;
        W2 = 0;

        // 摄像头
        cameraOn = false;

        // 更新显示
        ApplyLight();
    }

}




