using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DialogueContainer : ScriptableObject
{
    public List<EdgeData> NodeLinks = new List<EdgeData>();
    public List<DialogueNodeData> DialogueNodeData = new List<DialogueNodeData>();
    public List<ExposedProperty> Exposedproperties = new List<ExposedProperty>();
    public DialogueNodeData EntryNodeData;
}
