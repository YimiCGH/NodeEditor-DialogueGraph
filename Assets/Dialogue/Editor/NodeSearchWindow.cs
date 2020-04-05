using UnityEngine;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DialogueGraphView _graphView;
    private EditorWindow _window;

    private Texture2D _indentationIcon;

    public void Init(DialogueGraphView graphView,EditorWindow window) {
        _graphView = graphView;
        _window = window;

        _indentationIcon = new Texture2D(1,1);
        _indentationIcon.SetPixel(0,0,new Color(0,0,0,0));
        _indentationIcon.Apply();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>{ 
            new  SearchTreeGroupEntry(new GUIContent("Create Elemets"),0),
            new  SearchTreeGroupEntry(new GUIContent("Dialogue Node"),1),
            new SearchTreeEntry(new GUIContent("Diaogue Node",_indentationIcon)){ 
                userData = new DialogueNode(),level = 2
            }

        };
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var worldMousePos = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
            context.screenMousePosition - _window.position.position);

        var localMousePos = _graphView.contentContainer.WorldToLocal(worldMousePos);

        switch (SearchTreeEntry.userData) {
            case DialogueNode dialogueNode:
                Debug.Log("创建节点");
                _graphView.CreateNode("Diaogue Node", localMousePos);                
                return true;
            default:
                return false;
        }
    }
}
