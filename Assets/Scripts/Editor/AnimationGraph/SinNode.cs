using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableSinNode {
  public SerializableGraphNode graphNode;
  public string outputPortGuid;
  public SerializableCalculateValueField calculateField;
  public SerializableSinNode() {
    this.outputPortGuid = Guid.NewGuid().ToString();
    this.calculateField = new SerializableCalculateValueField();
  }
  public SerializableSinNode(SinNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.calculateField = new SerializableCalculateValueField(node.calculateField);
    this.outputPortGuid = node.outputPortGuid;
  }
}
public class SinNode : Node, ICalculateNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public Dictionary<Port, Func<object>> Calculate { get; private set; } = new Dictionary<Port, Func<object>>();
  public string outputPortGuid { get; private set; }
  public CalculateValueField calculateField;

  void Construct(SerializableSinNode serializable) {
    this.title = "Sin";

    this.calculateField = new CalculateValueField(this, serializable.calculateField);
    this.inputContainer.Add(calculateField);

    var outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
    this.outputPortGuid = serializable.outputPortGuid;
    graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    this.graphNode.isCompatible = (input, output) => {
      if (calculateField.fields.Any()) {
        var existType = calculateField.fields.First().inputPort.portType;
        return input.portType == existType;
      }
      return input.portType == typeof(float) || calculateField.isVectorOrMatrix(input.portType);
    };

    this.Calculate[outputPort] = () => {
      var field = calculateField.fields[0];
      float value;
      if (field.inputPort.connected) {
        var port = field.inputPort.connections.First().output;
        var node = port.node as ICalculateNode;
        value = (float) node.Calculate[port]();
      } else {
        value = (float) field.valueField.value;
      }
      return Mathf.Sin(value);
    };
  }
  
  void SaveAsset(GraphAsset asset) {
    asset.sinNodes.Add(new SerializableSinNode(this));
  }

  // New
  public SinNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    var serializable = new SerializableSinNode();
    serializable.calculateField.fields.Add(new SerializableCalculateValueField.Field());
    this.Construct(serializable);
  }
  
  // Load
  public SinNode(GraphView graphView, SerializableSinNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
