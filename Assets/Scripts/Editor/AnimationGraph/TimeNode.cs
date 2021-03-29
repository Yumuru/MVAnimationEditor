using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableTimeNode {
  public SerializableGraphNode graphNode;
  public string timePortGuid;
  public float time;
  public string outputPortGuid;
  public SerializableTimeNode() {
    this.timePortGuid = Guid.NewGuid().ToString();
    this.outputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableTimeNode(TimeNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.timePortGuid = node.timePortGuid;
    this.time = node.timeField.value;
    this.outputPortGuid = node.outputPortGuid;
  }
}
public class TimeNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string timePortGuid;
  public FloatField timeField;
  public string outputPortGuid;

  public void SaveAsset(GraphAsset graphAsset) {
    graphAsset.timeNodes.Add(new SerializableTimeNode(this));
  }

  void Construct(SerializableTimeNode serializable) {
    this.title = "Time";
    
    this.timeField = new FloatField();
    this.timeField.label = "Time";
    this.timeField.labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
    this.timeField.labelElement.style.minWidth = 50;
    this.timeField.value = serializable.time;
    this.mainContainer.Add(timeField);

    var setValuePort = CalculatePort.CreateCalculateInPort<float>();

    var outputPort = new SequenceActionPort(true);
    this.outputPortGuid = serializable.outputPortGuid;
    graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    outputPort.Calculate = () => {
      return p => {
        p.time += timeField.value;
        p.constructor.TSet(p.time);
        return p;
      };
    };
  }

  public TimeNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializableTimeNode());
  }
  public TimeNode(GraphView graphView, SerializableTimeNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
