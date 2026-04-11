using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class FaceDropdown : MonoBehaviour
{
    public TMP_Dropdown faceDropdown;

    int lastFaceDropdownIndex;

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
            list.Add("Face " + (i + 1));
        }

        faceDropdown.AddOptions(list);
        faceDropdown.SetValueWithoutNotify(0);
        faceDropdown.RefreshShownValue();
        lastFaceDropdownIndex = 0;

        faceDropdown.onValueChanged.AddListener(OnFaceChanged);
    }

    void OnFaceChanged(int index)
    {
        if (index <= 0)
        {
            lastFaceDropdownIndex = index;
            return;
        }

        var faceCtrl = FindObjectOfType<FaceUIController>();
        if (faceCtrl == null) return;

        faceCtrl.SelectFace(index - 1);
        lastFaceDropdownIndex = index;
    }
}
