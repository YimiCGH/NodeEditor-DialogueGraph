using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.Linq;

public class DialogueGraphView : GraphView
{
    public readonly Vector2 DefaultNodeSize = new Vector2(150,200);
    public DialogueGraphView() {

        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        AddElement(GenerateEntryPointNode());
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
    }

    private DialogueNode GenerateEntryPointNode() {
        var node = new DialogueNode
        {
            title = "Start",
            GUID = Guid.NewGuid().ToString(),
            DialogueText = "ENTRYPOINT",
            EntryPoint = true
        };

        node.SetPosition(new Rect(100,200,100,100));

        var generatedPort = GeneratePort(node,Direction.Output);
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);

        node.RefreshExpandedState();
        node.RefreshPorts();

        return node;
    }

    public DialogueNode CreateDialogueNode(string nodeName)
    {
        var dialogueNode = new DialogueNode { 
            title = nodeName,
            DialogueText = nodeName,
            GUID = Guid.NewGuid().ToString()
        };

        var inputPort = GeneratePort(dialogueNode,Direction.Input,Port.Capacity.Multi);
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);


        var button = new Button(()=> { AddChoicePort(dialogueNode); });
        button.text = "New Choice";
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
        dialogueNode.SetPosition(new Rect(Vector2.zero, DefaultNodeSize));

        return dialogueNode;
    }

    public void CreateNode(string nodeName) {
        AddElement(CreateDialogueNode(nodeName));
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

        var deletButton = new Button(() => RemovePort(dialogueNode, generatedPort)) { text = "X" };
        generatedPort.contentContainer.Add(deletButton);


        dialogueNode.outputContainer.Add(generatedPort);
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }

    private void RemovePort(DialogueNode dialogueNode, Port generatedPort)
    {
        var targetEdge = edges.ToList()
            .Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

        if (!targetEdge.Any()) {
            return;
        }

        var edge = targetEdge.First();
        edge.input.Disconnect(edge);
        RemoveElement(targetEdge.First());

        dialogueNode.outputContainer.Remove(generatedPort);
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }

    private Port GeneratePort(DialogueNode node,Direction portDirection,Port.Capacity capacity = Port.Capacity.Single) {
        return node.InstantiatePort(Orientation.Horizontal,portDirection, capacity, typeof(float));
    }

    
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


}
