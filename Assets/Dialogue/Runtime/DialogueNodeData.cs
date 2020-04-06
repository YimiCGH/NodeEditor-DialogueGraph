using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNodeData
{
    public string Guid;
    public string DialogueText;
    public Vector2 Position;

    public override string ToString()
    {
        return $"Guid:{Guid} ,DialogueText :{DialogueText} ,Position :{Position}";
    }
}
