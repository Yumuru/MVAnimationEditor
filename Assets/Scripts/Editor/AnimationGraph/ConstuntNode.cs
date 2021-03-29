using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableConstuntNode {
  public SerializableGraphNode graphNode;
  public string name;
  public float value;
  public string inputPortGuid;
  public string outputPortGuid;
  public SerializableConstuntNode() {
    inputPortGuid = Guid.NewGuid().ToString();
    outputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableConstuntNode(ConstuntNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.name = node.constantNameField.value;
    this.value = node.valueField.value;
    this.inputPortGuid = node.inputPortGuid;
    this.outputPortGuid = node.outputPortGuid;
  }
}
public class ConstuntNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public TextField constantNameField { get; private set; }
  public FloatField valueField { get; private set; }
  public string inputPortGuid { get; private set; }
  public string outputPortGuid { get; private set; }

  void Construct(SerializableConstuntNode serializable) {
    this.title = "Constunt";
    var inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
    var outputPort = new BasicCalculatedOutPort<float>();
    this.inputPortGuid = serializable.inputPortGuid;
    this.outputPortGuid = serializable.outputPortGuid;
    graphNode.RegisterPort(inputPort, inputPortGuid);
    graphNode.RegisterPort(outputPort, outputPortGuid);
    this.inputContainer.Add(inputPort);
    this.outputContainer.Add(outputPort);

    this.constantNameField = new TextField("Name");
    this.valueField = new FloatField("Value");
    constantNameField.value = serializable.name;
    valueField.value = serializable.value;
    this.mainContainer.Add(constantNameField);
    this.mainContainer.Add(valueField);
    
    outputPort.Calculate = () => {
      if (inputPort.connected) {
        return CalculatePort.GetCalculatedValue<float>(inputPort);
      } else {
        return valueField.value;
      }
    };
  }
  
  void SaveAsset(GraphAsset asset) {
    asset.constuntNodes.Add(new SerializableConstuntNode(this));
  }

  // New
  public ConstuntNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    var serializable = new SerializableConstuntNode();
    this.Construct(new SerializableConstuntNode());
  }
  
  // Load
  public ConstuntNode(GraphView graphView, SerializableConstuntNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
