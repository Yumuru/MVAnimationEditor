using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializablePropertyChangeValueNode {
  public SerializableGraphNode graphNode;
  public string propretyPortGuid;
  public string targetValuePortGuid;
  public string curvePortGuid;
  public string outputPortGuid;
  public AnimationCurve curve;
  public SerializablePropertyChangeValueNode() {
    this.propretyPortGuid = Guid.NewGuid().ToString();
    this.targetValuePortGuid = Guid.NewGuid().ToString();
    this.curvePortGuid = Guid.NewGuid().ToString();
    this.outputPortGuid = Guid.NewGuid().ToString();
    this.curve = new AnimationCurve();
    curve.AddKey(0f, 0f);
    curve.AddKey(1f, 1f);
  }
  public SerializablePropertyChangeValueNode(PropertyChangeValueNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.propretyPortGuid = node.propertyPortGuid;
    this.targetValuePortGuid = node.targetValuePortGuid;
    this.curvePortGuid = node.curvePortGuid;
    this.outputPortGuid = node.outputPortGuid;
    this.curve = node.curveField.value;
  }
}

public struct PropertyChangeValue {
  public PropertyData property;
  public float targetValue;
}

public class PropertyChangeValueNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public CurveField curveField { get; private set; }
  public string propertyPortGuid;
  public string curvePortGuid;
  public string targetValuePortGuid;
  public string outputPortGuid;

  void SaveAsset(GraphAsset asset) {
    asset.propertyChangeValueNodes.Add(new SerializablePropertyChangeValueNode(this));
  }

  void Construct(SerializablePropertyChangeValueNode serializable) {
    this.title = "Property Value Change";

    this.propertyPortGuid = serializable.propretyPortGuid;
    var propertyPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(PropertyData));
    this.graphNode.RegisterPort(propertyPort, propertyPortGuid);
    this.inputContainer.Add(propertyPort);

    var targetValuePort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
    this.targetValuePortGuid = serializable.targetValuePortGuid;
    targetValuePort.portName = "Target Value";
    this.graphNode.RegisterPort(targetValuePort, targetValuePortGuid);
    this.inputContainer.Add(targetValuePort);

    var outputPort = new BasicCalculatedOutPort<PropertyChangeValue>();
    this.outputPortGuid = serializable.outputPortGuid;
    this.graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    outputPort.Calculate = () => {
      var property = CalculatePort.GetCalculatedValue<PropertyData>(propertyPort);
      float targetValue;
      if (targetValuePort.connected) {
        targetValue = CalculatePort.GetCalculatedValue<float>(targetValuePort);
      } else return default;
      return new PropertyChangeValue()
        { property = property, targetValue = targetValue };
    };
  }
  
  public PropertyChangeValueNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializablePropertyChangeValueNode());
  }
  public PropertyChangeValueNode(GraphView graphView, SerializablePropertyChangeValueNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
