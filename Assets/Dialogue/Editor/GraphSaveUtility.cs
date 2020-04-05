using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using UnityEditor;
using System;
using UnityEngine.UIElements;

public class GraphSaveUtility 
{
    public static void SaveGraph(string _fileName, DialogueGraphView targetGraphView) {
        var edges = targetGraphView.edges.ToList();

        if (!edges.Any()) {
            return;
        }

        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        var connectedPorts = edges.Where(x => x.input.node != null).ToArray();

        for (int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;

            dialogueContainer.NodeLinks.Add(new EdgeData { 
                FromNodeGuid = outputNode.GUID,
                PortName = connectedPorts[i].output.portName,
                ToNodeGuid = inputNode.GUID
            });
        }

        var nodes = targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

        foreach (var dialogueNode in nodes.Where(node => !node.EntryPoint)){

            if (dialogueNode.EntryPoint){
                dialogueContainer.EntryNodeData = dialogueNode.Save();
            }
            else {
                dialogueContainer.DialogueNodeData.Add(dialogueNode.Save());
            }
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources")) {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        AssetDatabase.CreateAsset(dialogueContainer,$"Assets/Resources/{_fileName}.asset");
        AssetDatabase.SaveAssets();
    }

    public static void LoadGraph(string fileName, DialogueGraphView graphView) {
        var graphDataCache = Resources.Load<DialogueContainer>(fileName);
        if (graphDataCache == null) {
            EditorUtility.DisplayDialog("找不到文件","目标 dialogue graph 不存在","确定");
            return;
        }

        ClearGraph(graphView);
        CreateNodes(graphView, graphDataCache);
        ConnectNodes(graphView, graphDataCache);
    }
    public static void ClearGraph(DialogueGraphView graph) {
        var Nodes = graph.nodes.ToList().Cast<DialogueNode>().ToList();
        //将起始节点 guid 设置为 缓存中的起始节点
        //Nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGuid;
        var edges = graph.edges.ToList();
        foreach (var node in Nodes)
        {
            //保留起始点
            if (node.EntryPoint) {
                
                continue;             
            }
            //找到所有链接当前节点的边，删除该边
            edges.Where(x => x.input.node == node)
                .ToList()
                .ForEach(edge => graph.RemoveElement(edge));

            //删除完边后，删除节点
            graph.RemoveElement(node);
        }

        var entryNode = Nodes.Find(x => x.EntryPoint);
        using (var outputs = entryNode.outputContainer.Children().GetEnumerator()) {
            while (outputs.MoveNext())
            {
                var port = outputs.Current.Q<Port>();
                port.DisconnectAll();
            }
            entryNode.RefreshExpandedState();
            entryNode.RefreshPorts();
        }
    }
    private static void CreateNodes(DialogueGraphView graphView,DialogueContainer dialogueContainer)
    {
        foreach (var nodeData in dialogueContainer.DialogueNodeData)
        {
            var tempNode = graphView.CreateDialogueNode(nodeData.DialogueText, nodeData.Position);
            tempNode.Load(nodeData);
            graphView.AddElement(tempNode);

            //添加端口
            var nodePorts = dialogueContainer.NodeLinks.Where(x => x.FromNodeGuid == nodeData.Guid).ToList();
            nodePorts.ForEach(x => graphView.AddChoicePort(tempNode,x.PortName));
        }

        graphView.EntryNode.Load(dialogueContainer.EntryNodeData,true);
    }
    private static void ConnectNodes(DialogueGraphView graphView, DialogueContainer dialogueContainer)
    {
        var nodes = graphView.nodes.ToList().Cast<DialogueNode>().ToList() ;

        for (int i = 0; i < nodes.Count; i++)
        {
            var cuNode = nodes[i];

            //找到所有从此节点开始的连线
            var connections = dialogueContainer.NodeLinks
                .Where(x=>x.FromNodeGuid == cuNode.GUID)
                .ToList();

            for (int j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].ToNodeGuid;
                //找到这条连线的目标节点
                var targetNode = nodes.First( x => x.GUID == targetNodeGuid);

                LinkNodes(graphView,
                    cuNode.outputContainer[j].Q<Port>(),
                    (Port) targetNode.inputContainer[0]);
                //获取端口的两种表示方法
            }
        }
    }

    private static void LinkNodes(DialogueGraphView graphView, Port output,Port input) {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);

        graphView.Add(tempEdge);
    }
  

    
}
