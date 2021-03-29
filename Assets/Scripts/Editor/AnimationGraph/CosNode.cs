using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableCosNode {
  public SerializableGraphNode graphNode;
  public string outputPortGuid;
  public SerializableCalculateValueField calculateField;
  public SerializableCosNode() {
    this.outputPortGuid = Guid.NewGuid().ToString();
    this.calculateField = new SerializableCalculateValueField();
  }
  public SerializableCosNode(CosNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.calculateField = new SerializableCalculateValueField(node.calculateField);
    this.outputPortGuid = node.outputPortGuid;
  }
}
public class CosNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string outputPortGuid { get; private set; }
  public CalculateValueField calculateField;

  void Construct(SerializableCosNode serializable) {
    this.title = "Cos";

    this.calculateField = new CalculateValueField(this, serializable.calculateField);
    this.inputContainer.Add(calculateField);

    var outputPort = new BasicCalculatedOutPort<float>();
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

    
    outputPort.Calculate = () => {
      var field = calculateField.fields[0];
      float value;
      if (field.inputPort.connected) {
        return CalculatePort.GetCalculatedValue<float>(field.inputPort);
      } else {
        value = (float) field.valueField.value;
      }
      return Mathf.Cos(value);
    };
  }
  
  void SaveAsset(GraphAsset asset) {
    asset.cosNodes.Add(new SerializableCosNode(this));
  }

  // New
  public CosNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    var serializable = new SerializableCosNode();
    serializable.calculateField.fields.Add(new SerializableCalculateValueField.Field());
    this.Construct(serializable);
  }
  
  // Load
  public CosNode(GraphView graphView, SerializableCosNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
