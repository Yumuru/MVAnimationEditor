using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableMultiplyNode {
  public SerializableGraphNode graphNode;
  public string outputPortGuid;
  public SerializableCalculateValueField calculateField;
  public SerializableMultiplyNode() {
    this.outputPortGuid = Guid.NewGuid().ToString();
    this.calculateField = new SerializableCalculateValueField();
  }
  public SerializableMultiplyNode(MultiplyNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.calculateField = new SerializableCalculateValueField(node.calculateField);
    this.outputPortGuid = node.outputPortGuid;
  }
}
public class MultiplyNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string outputPortGuid { get; private set; }
  public CalculateValueField calculateField;

  void Construct(SerializableMultiplyNode serializable) {
    this.title = "Multiply";

    this.calculateField = new CalculateValueField(this, serializable.calculateField);
    this.inputContainer.Add(calculateField);

    var outputPort = new BasicCalculatedOutPort<float>();
    this.outputPortGuid = serializable.outputPortGuid;
    graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    this.graphNode.isCompatible = (input, output) => {
      if (calculateField.isVectorOrMatrix(input.portType)) return !calculateField.isContainVectorOrMatrixInput;
      return input.portType == typeof(float);
    };

    outputPort.Calculate = () => {
      var value = 1f;
      foreach (var field in calculateField.fields) {
        if (field.inputPort.connected) {
          value *= CalculatePort.GetCalculatedValue<float>(field.inputPort);
        } else {
          value *= field.valueField.value;
        }
      }
      return value;
    };
  }
  
  void SaveAsset(GraphAsset asset) {
    asset.multiplyNodes.Add(new SerializableMultiplyNode(this));
  }

  // New
  public MultiplyNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    var serializable = new SerializableMultiplyNode();
    serializable.calculateField.fields.Add(new SerializableCalculateValueField.Field());
    serializable.calculateField.fields.Add(new SerializableCalculateValueField.Field());
    this.Construct(serializable);
  }
  
  // Load
  public MultiplyNode(GraphView graphView, SerializableMultiplyNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
