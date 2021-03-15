using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableAddNode {
  public SerializableGraphNode graphNode;
  public string outputPortGuid;
  public SerializableCalculateValueField calculateField;
  public SerializableAddNode() {
    this.outputPortGuid = Guid.NewGuid().ToString();
    this.calculateField = new SerializableCalculateValueField();
  }
  public SerializableAddNode(AddNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.calculateField = new SerializableCalculateValueField(node.calculateField);
    this.outputPortGuid = node.outputPortGuid;
  }
}
public class AddNode : Node, ICalculateNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public Dictionary<Port, Func<object>> Calculate { get; private set; } = new Dictionary<Port, Func<object>>();
  public string outputPortGuid { get; private set; }
  public CalculateValueField calculateField;

  void Construct(SerializableAddNode serializable) {
    this.title = "Add";

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
      var value = 1f;
      foreach (var field in calculateField.fields) {
        if (field.inputPort.connected) {
          var port = field.inputPort.connections.First().output;
          var node = port.node as ICalculateNode;
          var v = node.Calculate[port]();
          value += (float) v;
        } else {
          value += field.valueField.value;
        }
      }
      return value;
    };
  }
  
  void SaveAsset(GraphAsset asset) {
    asset.addNodes.Add(new SerializableAddNode(this));
  }

  // New
  public AddNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    var serializable = new SerializableAddNode();
    serializable.calculateField.fields.Add(new SerializableCalculateValueField.Field());
    serializable.calculateField.fields.Add(new SerializableCalculateValueField.Field());
    this.Construct(serializable);
  }
  
  // Load
  public AddNode(GraphView graphView, SerializableAddNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
