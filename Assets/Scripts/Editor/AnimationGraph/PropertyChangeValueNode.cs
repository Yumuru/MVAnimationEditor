using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializablePropertyValueChangeNode {
  public SerializableGraphNode graphNode;
  public string propretyPortGuid;
  public string targetValuePortGuid;
  public string curvePortGuid;
  public string outputPortGuid;
  public AnimationCurve curve;
  public SerializablePropertyValueChangeNode() {
    this.propretyPortGuid = Guid.NewGuid().ToString();
    this.targetValuePortGuid = Guid.NewGuid().ToString();
    this.curvePortGuid = Guid.NewGuid().ToString();
    this.outputPortGuid = Guid.NewGuid().ToString();
    this.curve = new AnimationCurve();
    curve.AddKey(0f, 0f);
    curve.AddKey(1f, 1f);
  }
  public SerializablePropertyValueChangeNode(PropertyValueChangeNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.propretyPortGuid = node.propertyPortGuid;
    this.targetValuePortGuid = node.targetValuePortGuid;
    this.curvePortGuid = node.curvePortGuid;
    this.outputPortGuid = node.outputPortGuid;
  }
}

public struct PropertyChangeValue {
  public PropertyData property;
  public float targetValue;
}

public class PropertyValueChangeNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string propertyPortGuid;
  public string curvePortGuid;
  public string targetValuePortGuid;
  public string outputPortGuid;

  void SaveAsset(GraphAsset asset) {
    asset.propertyValueChangeNodes.Add(new SerializablePropertyValueChangeNode(this));
  }

  void Construct(SerializablePropertyValueChangeNode serializable) {
    this.title = "Property Change Value";

    this.propertyPortGuid = serializable.propretyPortGuid;
    var propertyPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(PropertyData));
    this.graphNode.RegisterPort(propertyPort, propertyPortGuid);
    this.inputContainer.Add(propertyPort);

    var targetValuePort = CalculatePort.CreateInput<float>();
    this.targetValuePortGuid = serializable.targetValuePortGuid;
    targetValuePort.portName = "Target Value";
    this.graphNode.RegisterPort(targetValuePort, targetValuePortGuid);
    this.inputContainer.Add(targetValuePort);

    var outputPort = CalculatePort.CreateOutput<Proceed>();
    this.outputPortGuid = serializable.outputPortGuid;
    this.graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    outputPort.source = new PortObject<Proceed>(() => {
      var propertyData = CalculatePort.GetCalculatedValue<PropertyData>(propertyPort);
      var property = new Yumuru.AnimationConstructor.FloatPropertyInfo(propertyData.gameObject.transform, propertyData.type, propertyData.propertyName);
      float targetValue;
      if (targetValuePort.connected) {
        targetValue = CalculatePort.GetCalculatedValue<float>(targetValuePort);
      } else return default;
      return p => {
        p.constructor.AddImmediate(property, targetValue);
        return p;
      };
    });
  }
  
  public PropertyValueChangeNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializablePropertyValueChangeNode());
  }
  public PropertyValueChangeNode(AnimationGraphView graphView, SerializablePropertyValueChangeNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
