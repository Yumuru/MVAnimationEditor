using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableFloatValueNode {
  public SerializableGraphNode graphNode;
  public float value;
  public string outputPortGuid;
  public SerializableFloatValueNode() {
    outputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableFloatValueNode(FloatValueNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.value = node.valueField.value;
    this.outputPortGuid = node.outputPortGuid;
  }
}
public class FloatValueNode : Node, ICalculateNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public Dictionary<Port, Func<object>> Calculate { get; private set; } = new Dictionary<Port, Func<object>>();
  public FloatField valueField { get; private set; }
  public string outputPortGuid { get; private set; }

  void Construct(SerializableFloatValueNode serializable) {
    this.title = "Float Value";
    this.topContainer.Insert(1, this.titleContainer);
    this.titleButtonContainer.Clear();

    this.inputContainer.RemoveFromHierarchy();
    var outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
    this.outputPortGuid = serializable.outputPortGuid;
    graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    this.valueField = new FloatField("Value");
    valueField.value = serializable.value;
    this.mainContainer.Add(valueField);
    
    this.Calculate[outputPort] = () => valueField.value;
  }
  
  void SaveAsset(GraphAsset asset) {
    asset.floatValueNodes.Add(new SerializableFloatValueNode(this));
  }

  // New
  public FloatValueNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    var serializable = new SerializableConstuntNode();
    this.Construct(new SerializableFloatValueNode());
  }
  
  // Load
  public FloatValueNode(GraphView graphView, SerializableFloatValueNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}