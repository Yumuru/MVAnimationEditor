using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableListSelectNode {
  public SerializableGraphNode graphNode;
  public string inputPortGuid;
  public string indexPortGuid;
  public string outputPortGuid;
  public SerializableListSelectNode() {
    inputPortGuid = Guid.NewGuid().ToString();
    indexPortGuid = Guid.NewGuid().ToString();
    outputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableListSelectNode(ListSelectNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.inputPortGuid = node.inputPortGuid;
    this.indexPortGuid = node.indexPortGuid;
    this.outputPortGuid = node.outputPortGuid;
  }
}
public class ListSelectNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string inputPortGuid { get; private set; }
  public string indexPortGuid { get; private set; }
  public string outputPortGuid { get; private set; }

  void Construct(SerializableListSelectNode serializable) {
    this.title = "List Select";

    var inputPort = CalculatePort.CreateInput<List<object>>();
    this.inputPortGuid = serializable.inputPortGuid;
    inputPort.portName = "List<Object>";
    graphNode.RegisterPort(inputPort, inputPortGuid);

    var indexPort = CalculatePort.CreateInput<int>();
    this.indexPortGuid = serializable.indexPortGuid;
    indexPort.portName = "Index";
    graphNode.RegisterPort(indexPort, indexPortGuid);

    this.inputContainer.Add(inputPort);
    this.inputContainer.Add(indexPort);

    var outputPort = CalculatePort.CreateOutput<object>();
    this.outputPortGuid = serializable.outputPortGuid;
    graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);
    
    outputPort.source = new PortObject<object>(() => {
      var list = (List<object>)CalculatePort.GetCalculatedValue(inputPort);
      var index = (int) CalculatePort.GetCalculatedValue(indexPort);
      if (list == null) return null;
      return list[index];
    });
  }

  void SaveAsset(GraphAsset asset) {
    asset.listSelectNodes.Add(new SerializableListSelectNode(this));
  }

  // New
  public ListSelectNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializableListSelectNode());
  }
  
  // Load
  public ListSelectNode(AnimationGraphView graphView, SerializableListSelectNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
