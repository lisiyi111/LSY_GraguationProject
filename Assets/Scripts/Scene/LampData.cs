[System.Serializable]
public class LampData
{
    public int groupId;
    public int lampIndex;

    public byte R, G, B, W;
    public byte R2, G2, B2, W2;

    public bool hasCamera;
    public bool cameraOn;
}