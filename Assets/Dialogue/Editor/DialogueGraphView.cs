using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.Linq;


public class DialogueGraphView : GraphView
{
    public static readonly Vector2 DefaultNodeSize = new Vector2(150,200);
    public DialogueNode EntryNode;
    //public Blackboard Blackboard;

    //private List<ExposedProperty> _properties = new List<ExposedProperty>();

    private StyleSheet _graphUss;
    private StyleSheet _nodeUss;

    public DialogueGraphView() 
    {
        LoadUss();
        AddManipulators();
        InitializeView();
    }

    /// <summary>
    /// 添加鼠标操作到窗口
    /// </summary>
    private void AddManipulators() {
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ClickSelector());
    }

    private void LoadUss() {
        _graphUss = Resources.Load<StyleSheet>("DialogueGraph");
        _nodeUss = Resources.Load<StyleSheet>("Node");
    }
   

    private void InitializeView() {
        //加载USS文件
        styleSheets.Add(_graphUss);
        //绘制网格背景        
        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        //设置缩放范围
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        //填充整个窗口
        this.StretchToParentSize();

        //创建起始节点
        AddElement(GenerateEntryPointNode());
    }

    private DialogueNode GenerateEntryPointNode() {
        var node = DialogueNode.CreateNode("Start", "ENTRYPOINT",true);

        node.SetPosition(new Rect(100,200,100,100));

        var generatedPort = GeneratePort(node,Direction.Output);
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);


        node.capabilities &= ~Capabilities.Movable;//不可移动
        node.capabilities &= ~Capabilities.Deletable;//不可删除

        node.RefreshExpandedState();
        node.RefreshPorts();

        EntryNode = node;

        return node;
    }

    #region Public
    public void CreateNode(string nodeName,Vector2 screenPos)
    {
        var node = CreateDialogueNode(nodeName, screenPos);
        AddElement(node);
    }
    public DialogueNode CreateDialogueNode(string nodeName, Vector2 screenPos)
    {
        var dialogueNode = DialogueNode.CreateNode(nodeName, nodeName, false);
        dialogueNode.styleSheets.Add(_nodeUss);

        var inputPort = GeneratePort(dialogueNode,Direction.Input,Port.Capacity.Multi);
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);


        var button = new Button(() => { AddChoicePort(dialogueNode); })
        {
            text = "New Choice"
        };
        dialogueNode.titleButtonContainer.Add(button);

        var textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt => {
            dialogueNode.DialogueText = evt.newValue;
            dialogueNode.title = evt.newValue;
        });
        textField.SetValueWithoutNotify(dialogueNode.title);
        dialogueNode.mainContainer.Add(textField);

        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(screenPos, DefaultNodeSize));

        return dialogueNode;
    }


    public void AddChoicePort(DialogueNode dialogueNode,string overriddenPortName = "") {
        var generatedPort = GeneratePort(dialogueNode,Direction.Output);

        var oldLable = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(oldLable);

        var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;
        

        var choicePortName = string.IsNullOrEmpty(overriddenPortName) 
            ? $"Choice {outputPortCount + 1}" : overriddenPortName;
        generatedPort.portName = choicePortName;


        var texField = new TextField
        {
            name = string.Empty,
            value = choicePortName
        };
   
        texField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
        generatedPort.contentContainer.Add(new Label("  "));
        generatedPort.contentContainer.Add(texField);

        var deletButton = new Button(() => RemoveOutputPort(dialogueNode, generatedPort)) { text = "X" };
        generatedPort.contentContainer.Add(deletButton);


        dialogueNode.outputContainer.Add(generatedPort);
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }
    #endregion

    private void RemoveOutputPort(DialogueNode dialogueNode, Port outputPort)
    {
        //从GraphView 的edges中，找到需要删除的端口
        // 因为一个端口可能存在多条边连接的情况，所以，返回的是一个迭代器
        var targetEdge = edges.ToList()
            .Where(x => x.output.portName == outputPort.portName && x.output.node == outputPort.node);

        //如果没有找到该边
        if (!targetEdge.Any()) {
            return;
        }

        //删除所有链接该端口的边
        using (var alledges = targetEdge.GetEnumerator()) {
            while (alledges.MoveNext())
            {

                var edge = alledges.Current;//targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            } 
        }
        
        //删除端口
        dialogueNode.outputContainer.Remove(outputPort);
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }

    private Port GeneratePort(DialogueNode node,Direction portDirection,Port.Capacity capacity = Port.Capacity.Single) {
        return node.InstantiatePort(Orientation.Horizontal,portDirection, capacity, typeof(float));
    }

    #region Overridden
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach(
            port => {
                //不会连接到自身，或者同一个节点的其他端口
                if (startPort != port && startPort.node != port.node) {
                    compatiblePorts.Add(port);
                }

            });

        return compatiblePorts;
    }

    #endregion

    #region Blackboard
    /*
    public void AddPropertyToBlackBoard(ExposedProperty exposedProperty) {
        var property = new ExposedProperty();
        property.PropertyName = exposedProperty.PropertyName;
        property.PropertyValue = exposedProperty.PropertyValue;

        //_properties.Add(property);


        var container = AddStringType(property);

        //Blackboard.Add(container);
    }


    VisualElement AddStringType(ExposedProperty property) {
        var container = new VisualElement();
        var blackboardField = new BlackboardField
        {
            text = property.PropertyName,
            typeText = "string"
        };
        container.Add(blackboardField);

        var propertyValueTextFeld = new TextField("value") { 
            value = property.PropertyValue
        };
        propertyValueTextFeld.RegisterValueChangedCallback(
            evt => {
                var changeIndex = _properties.FindIndex(x => x.PropertyName == property.PropertyName);
                _properties[changeIndex].PropertyValue = evt.newValue;
            }
            );

        var blackboardValueRow = new BlackboardRow(blackboardField,propertyValueTextFeld);
        container.Add(blackboardValueRow);
  

        return container;
    }
*/
    #endregion
}
