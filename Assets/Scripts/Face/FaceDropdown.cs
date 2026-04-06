using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class FaceDropdown : MonoBehaviour
{
    public TMP_Dropdown faceDropdown;

    void Start()
    {
        InitFaceDropdown();
    }

    void InitFaceDropdown()
    {
        faceDropdown.ClearOptions();

        var list = new List<string>();
        list.Add("");
        for (int i = 0; i < 31; i++)
        {
                list.Add("Face " + ( i + 1 ) );
        }

        faceDropdown.AddOptions(list);
        faceDropdown.SetValueWithoutNotify(0);
        faceDropdown.RefreshShownValue();

        faceDropdown.onValueChanged.AddListener(OnFaceChanged);
    }

    void OnFaceChanged(int index)
    {
        // 0 是占位项，不执行任何选择
        if (index <= 0) return;

        FindObjectOfType<FaceUIController>().SelectFace(index - 1);
    }
}
