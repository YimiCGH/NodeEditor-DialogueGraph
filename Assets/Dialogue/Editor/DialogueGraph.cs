using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;
    private NodeSearchWindow _searchWindow;
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
        GenerateMiniMap();
        GenerateBlackBoard();
        GenerateSearchWindow();
    }

    private void ConstructGraphView() {
        _graphView = new DialogueGraphView
        {
            name = "Dialogue Graph"
        };

        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar() {
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField("File Name:");
        fileNameTextField.SetValueWithoutNotify(_fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        toolbar.Add(fileNameTextField);

        toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save" });
        toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Load" });

        toolbar.Add(new Button(() => CleaGraph()) { text = "Clear Graph" });

        rootVisualElement.Add(toolbar);
    }
    private void GenerateMiniMap()
    {
        var miniMap = new MiniMap();
        miniMap.anchored = false;//可以随意拖拽移动

        var cords = _graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10,30));
        miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 150));
        _graphView.Add(miniMap);
    }
    private void GenerateSearchWindow()
    {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Init(_graphView, this);
        //侦听创建节点请求事件，空格键或右键
        _graphView.nodeCreationRequest = ctx => SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition), _searchWindow);
    }
    
    private void GenerateBlackBoard() {
        var blackboard = new Blackboard(_graphView);
        blackboard.Add(new BlackboardSection { title = "全局变量" });
        blackboard.addItemRequested = _blackboard => { _graphView.AddPropertyToBlackBoard(new ExposedProperty()); };

        blackboard.editTextRequested = (_blackboard, _element, _newValue) => {
            var oldPropertyName = ((BlackboardField)_element).text;
            if (_graphView.Exposedproperties.Any(x => x.PropertyName == _newValue))
            {
                EditorUtility.DisplayDialog("Error", "已存在同样的名称，请选择其他名称", "确定");
            }
            else {
                //应用新的名称
                ((BlackboardField)_element).text = _newValue;
                //把存储列表中的变量名称也改为新的名称
                var propertyIndex = _graphView.Exposedproperties.FindIndex(x => x.PropertyName == oldPropertyName);
                _graphView.Exposedproperties[propertyIndex].PropertyName = _newValue;
            }
        };

        blackboard.SetPosition(new Rect(10,30,200,300));
        _graphView.Blackboard = blackboard;
        _graphView.Add(blackboard);
        
    }
    private void RequestDataOperation(bool save) {

        if (string.IsNullOrEmpty(_fileName)) {
            EditorUtility.DisplayDialog("无效文件名", "请输入有效文件名", "确定");
            return;
        }
        
        if (save)
        {
            GraphSaveUtility.SaveGraph(_fileName, _graphView);
        }
        else {
            GraphSaveUtility.LoadGraph(_fileName, _graphView);
        }
    }

    private void CleaGraph() {
        GraphSaveUtility.ClearGraph(_graphView);
    }
}
