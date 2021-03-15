using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableTestNode {
  public SerializableGraphNode graphNode;
  public string inputPortGuid;
  public SerializableTestNode() {
    this.inputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableTestNode(TestNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.inputPortGuid = node.inputPortGuid;
  }
}
public class TestNode : Node, IProcessNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string inputPortGuid;
  public Action<ProcessParameter> Proceed { get; private set; }

  void SaveAsset(GraphAsset asset) {
    asset.testNodes.Add(new SerializableTestNode(this));
  }

  void Construct(SerializableTestNode serializable) {
    this.title = "Test Node (-ω-)";
    var inputPort = ProcessPort.CreateInput();
    graphNode.RegisterPort(inputPort, serializable.inputPortGuid);
    this.inputPortGuid = serializable.inputPortGuid;
    this.inputContainer.Add(inputPort);
    this.outputContainer.RemoveFromHierarchy();

    this.Proceed = p => {
      Debug.Log(p.time);
    };
  }

  public TestNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    Construct(new SerializableTestNode());
  }

  public TestNode(GraphView graphView, SerializableTestNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(graphNode as GraphNodeLogic);
    Construct(serializable);
  }
}
}
