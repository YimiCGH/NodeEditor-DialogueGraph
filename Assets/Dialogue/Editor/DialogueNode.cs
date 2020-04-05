using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[System.Serializable]
public class DialogueNode : Node
{
    public string GUID;
    public string DialogueText;
    public bool EntryPoint = false;

    public static DialogueNode CreateNode(string nodeName,string dialogueText,bool entryPoint) {
        var dialogueNode = new DialogueNode
        {
            title = nodeName,
            DialogueText = dialogueText,
            GUID = Guid.NewGuid().ToString(),
            EntryPoint = entryPoint
        };
        return dialogueNode;
    }

    public DialogueNodeData Save() {
        return new DialogueNodeData {

            Guid = this.GUID,
            DialogueText = this.DialogueText,
            Position = this.GetPosition().position
        };
    }

    public void Load(DialogueNodeData data,bool isEntryPoint = false)
    {
        (GUID, DialogueText, EntryPoint) =
            (data.Guid, data.DialogueText, isEntryPoint);

        RefreshExpandedState();
        RefreshPorts();
    }
}
