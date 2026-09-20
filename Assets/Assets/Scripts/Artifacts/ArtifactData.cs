using UnityEngine;

[System.Serializable]
public class ArtifactData
{
    public ArtifactType type;
    public string displayName;

    [TextArea(2, 4)]
    public string description;

    public ArtifactData(ArtifactType type, string displayName, string description)
    {
        this.type = type;
        this.displayName = displayName;
        this.description = description;
    }
}
