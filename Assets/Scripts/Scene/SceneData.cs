using System.Collections.Generic;

[System.Serializable]
public class SceneData
{
    public Dictionary<int, List<LampData>> groups = new Dictionary<int, List<LampData>>();
}