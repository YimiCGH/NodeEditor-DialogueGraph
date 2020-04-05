using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;
    private string _fileName = "New Narrative";

    [MenuItem("Tool/Open DialogueGraph")]
    public static void Open() {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    private void ConstructGraphView() {
        _graphView = new DialogueGraphView
        {
            name = "Dialogue Graph"
        };

        _graphView.StretchToParentSize();

        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar() {
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField("File Name:");
        fileNameTextField.SetValueWithoutNotify(_fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        toolbar.Add(fileNameTextField);

        var nodeCreateButton = new Button(() => { _graphView.CreateNode("Dialogue Node"); })
        {
            text = "Create Node"
        };
        toolbar.Add(nodeCreateButton);

        toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save"});
        toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Load" });


        rootVisualElement.Add(toolbar);
    }

    void RequestDataOperation(bool save) {

        if (string.IsNullOrEmpty(_fileName)) {
            EditorUtility.DisplayDialog("无效文件名","请输入有效文件名","确定");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(_graphView);
        if (save)
        {
            saveUtility.SaveGraph(_fileName);
        }
        else {
            saveUtility.LoadGraph(_fileName);
        }
    }
}
