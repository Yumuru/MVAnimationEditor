using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableForloopNode {
  public SerializableGraphNode graphNode;
  public string inputPortGuid;
  public string bodyPortGuid;
  public string outputPortGuid;
  public string iterateCountPortGuid;
  public int loopnum;

  public SerializableForloopNode() {
    inputPortGuid = Guid.NewGuid().ToString();
    bodyPortGuid = Guid.NewGuid().ToString();
    outputPortGuid = Guid.NewGuid().ToString();
    iterateCountPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableForloopNode(ForloopNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.inputPortGuid = node.inputPortGuid;
    this.bodyPortGuid = node.bodyPortGuid;
    this.outputPortGuid = node.outputPortGuid;
    this.iterateCountPortGuid = node.iterateCountPortGuid;
    this.loopnum = node.loopNum.value;
  }
}
public class ForloopNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string inputPortGuid;
  public string bodyPortGuid;
  public string outputPortGuid;
  public string iterateCountPortGuid;
  public IntegerField loopNum;

  void Construct(SerializableForloopNode serializable) {
    this.title = "For loop";

    var inputPort = EventPort.CreateInput();
    this.inputPortGuid = serializable.inputPortGuid;
    graphNode.RegisterPort(inputPort, inputPortGuid);

    var outputPort = EventPort.CreateOutput();
    outputPort.portName = "Exit";
    this.outputPortGuid = serializable.outputPortGuid;
    graphNode.RegisterPort(outputPort, outputPortGuid);

    var bodyPort = EventPort.CreateOutput();
    bodyPort.portName = "Body";
    this.bodyPortGuid = serializable.bodyPortGuid;
    graphNode.RegisterPort(bodyPort, bodyPortGuid);

    this.inputContainer.Add(inputPort);
    this.outputContainer.Add(outputPort);
    this.outputContainer.Add(bodyPort);

    var iterateCountPort = CalculatePort.CreateOutput<int>();
    this.iterateCountPortGuid = serializable.iterateCountPortGuid;
    graphNode.RegisterPort(iterateCountPort, iterateCountPortGuid);
    iterateCountPort.portName = "Count";
    this.outputContainer.Add(iterateCountPort);
    var iterateCount = 0;
    iterateCountPort.source = new PortObject<int>(() => iterateCount);

    loopNum = new IntegerField();
    loopNum.label = "Loop Num";
    loopNum.value = serializable.loopnum;
    this.mainContainer.Add(loopNum);

    inputPort.NewEvent(p => {
      for (int i = 0; i < loopNum.value; i++) {
        iterateCount = i;
        p = EventPort.Proceed(p, bodyPort);
      }
      EventPort.Proceed(p, outputPort);
      return p;
    });
  }

  void SaveAsset(GraphAsset asset) {
    asset.forloopNodes.Add(new SerializableForloopNode(this));
  }

  // New
  public ForloopNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    var serializable = new SerializableForloopNode();

    this.Construct(serializable);
  }

  // Load
  public ForloopNode(AnimationGraphView graphView, SerializableForloopNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
