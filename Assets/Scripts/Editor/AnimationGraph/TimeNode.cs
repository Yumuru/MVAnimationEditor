using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
using static CalculateNode;
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
public class TimeNode : Node, ICalculateNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public Dictionary<Port, Func<object>> Calculate { get; } = new Dictionary<Port, Func<object>>();
  public string timePortGuid;
  public FloatField timeField;
  public string outputPortGuid;

  void Construct(SerializableTimeNode serializable) {
    this.title = "Time";
    
    this.timeField = new FloatField();
    this.timeField.label = "Time";
    this.timeField.labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
    this.timeField.labelElement.style.minWidth = 50;
    this.timeField.value = serializable.time;


  }
}
}
