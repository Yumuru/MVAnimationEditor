using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializablePropertyTransitionNode {
  public SerializableGraphNode graphNode;
  public string propretyPortGuid;
  public string targetValuePortGuid;
  public string curvePortGuid;
  public string outputPortGuid;
  public float targetValue;
  public AnimationCurve curve;
  public SerializablePropertyTransitionNode() {
    this.propretyPortGuid = Guid.NewGuid().ToString();
    this.targetValuePortGuid = Guid.NewGuid().ToString();
    this.curvePortGuid = Guid.NewGuid().ToString();
    this.outputPortGuid = Guid.NewGuid().ToString();
    this.curve = new AnimationCurve();
    curve.AddKey(0f, 0f);
    curve.AddKey(1f, 1f);
  }
  public SerializablePropertyTransitionNode(PropertyTransitionNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.propretyPortGuid = node.propertyPortGuid;
    this.targetValuePortGuid = node.targetValuePortGuid;
    this.curvePortGuid = node.curvePortGuid;
    this.outputPortGuid = node.outputPortGuid;
    this.targetValue = node.targetValueField.value;
    this.curve = node.curveField.value;
  }
}

public struct PropertyTransition {
  public PropertyData property;
  public float targetValue;
  public AnimationCurve curve;
}

public class PropertyTransitionNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public FloatField targetValueField { get; private set; }
  public CurveField curveField { get; private set; }
  public string propertyPortGuid;
  public string curvePortGuid;
  public string targetValuePortGuid;
  public string outputPortGuid;

  void SaveAsset(GraphAsset asset) {
    asset.propertyTransitionNodes.Add(new SerializablePropertyTransitionNode(this));
  }

  void Construct(SerializablePropertyTransitionNode serializable) {
    this.title = "Property Transition";

    this.propertyPortGuid = serializable.propretyPortGuid;
    var propertyPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(PropertyData));
    this.graphNode.RegisterPort(propertyPort, propertyPortGuid);
    this.inputContainer.Add(propertyPort);

    var targetValueElement = new VisualElement();
    targetValueElement.style.flexDirection = FlexDirection.Row;
    this.targetValuePortGuid = serializable.targetValuePortGuid;
    var targetValuePort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
    targetValuePort.portName = "Traget Value";
    this.graphNode.RegisterPort(targetValuePort, targetValuePortGuid);
    targetValueField = new FloatField();
    //targetValueField.label = "Target Value";
    targetValueField.labelElement.style.minWidth = 50;
    targetValueField.value = serializable.targetValue;
    targetValueElement.Add(targetValuePort);
    targetValueElement.Add(targetValueField);
    this.mainContainer.Add(targetValueElement);

    var curveElement = new VisualElement();
    curveElement.style.flexDirection = FlexDirection.Row;
    this.curvePortGuid = serializable.curvePortGuid;
    var curvePort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(AnimationCurve));
    this.graphNode.RegisterPort(curvePort, curvePortGuid);
    this.curveField = new CurveField();
    curveField.value = serializable.curve;
    curveField.style.minWidth = 100;
    curveElement.Add(curvePort);
    curveElement.Add(curveField);
    this.mainContainer.Add(curveElement);

    this.outputPortGuid = serializable.outputPortGuid;
    var outputPort = new BasicCalculatedOutPort<PropertyTransition>();
    this.graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    outputPort.Calculate = () => {
      var property = CalculatePort.GetCalculatedValue<PropertyData>(propertyPort);
      float targetValue;
      if (targetValuePort.connected) {
        targetValue = CalculatePort.GetCalculatedValue<float>(targetValuePort);
      } else {
        targetValue = targetValueField.value;
      }
      AnimationCurve curve;
      if (curvePort.connected) {
        curve = CalculatePort.GetCalculatedValue<AnimationCurve>(curvePort);
      } else {
        curve = curveField.value;
      }
      return new PropertyTransition()
        { property = property, targetValue = targetValue, curve = curve };
    };
  }
  
  public PropertyTransitionNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializablePropertyTransitionNode());
  }
  public PropertyTransitionNode(GraphView graphView, SerializablePropertyTransitionNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
