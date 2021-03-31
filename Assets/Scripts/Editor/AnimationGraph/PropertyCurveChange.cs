using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializablePropertyCurveChangeNode {
  public SerializableGraphNode graphNode;
  public string propretyPortGuid;
  public string timePortGuid;
  public string minValuePortGuid;
  public string maxValuePortGuid;
  public string curvePortGuid;
  public string outputPortGuid;
  public AnimationCurve curve;
  public SerializablePropertyCurveChangeNode() {
    this.propretyPortGuid = Guid.NewGuid().ToString();
    this.timePortGuid = Guid.NewGuid().ToString();
    this.minValuePortGuid = Guid.NewGuid().ToString();
    this.maxValuePortGuid = Guid.NewGuid().ToString();
    this.curvePortGuid = Guid.NewGuid().ToString();
    this.outputPortGuid = Guid.NewGuid().ToString();
    this.curve = new AnimationCurve();
    curve.AddKey(0f, 0f);
    curve.AddKey(1f, 1f);
  }
  public SerializablePropertyCurveChangeNode(PropertyCurveChangeNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.propretyPortGuid = node.propertyPortGuid;
    this.timePortGuid = node.timePortGuid;
    this.minValuePortGuid = node.minValuePortGuid;
    this.maxValuePortGuid = node.maxValuePortGuid;
    this.curvePortGuid = node.curvePortGuid;
    this.outputPortGuid = node.outputPortGuid;
    this.curve = node.curveField.value;
  }
}

public class PropertyCurveChangeNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public CurveField curveField { get; private set; }
  public string propertyPortGuid;
  public string timePortGuid;
  public string maxValuePortGuid;
  public string minValuePortGuid;
  public string curvePortGuid;
  public string outputPortGuid;

  void SaveAsset(GraphAsset asset) {
    asset.propertyCurveChangeNodes.Add(new SerializablePropertyCurveChangeNode(this));
  }

  void Construct(SerializablePropertyCurveChangeNode serializable) {
    this.title = "Property Curve Change";

    this.propertyPortGuid = serializable.propretyPortGuid;
    var propertyPort = CalculatePort.CreateInput<PropertyData>();
    this.graphNode.RegisterPort(propertyPort, propertyPortGuid);
    this.inputContainer.Add(propertyPort);

    var timePort = CalculatePort.CreateInput<float>();
    this.timePortGuid = serializable.timePortGuid;
    timePort.portName = "Time";
    this.graphNode.RegisterPort(timePort, timePortGuid);
    this.inputContainer.Add(timePort);

    var minValuePort = CalculatePort.CreateInput<float>();
    this.minValuePortGuid = serializable.minValuePortGuid;
    minValuePort.portName = "Min Value";
    this.graphNode.RegisterPort(minValuePort, minValuePortGuid);
    this.inputContainer.Add(minValuePort);

    var maxValuePort = CalculatePort.CreateInput<float>();
    this.maxValuePortGuid = serializable.maxValuePortGuid;
    maxValuePort.portName = "Max Value";
    this.graphNode.RegisterPort(maxValuePort, maxValuePortGuid);
    this.inputContainer.Add(maxValuePort);

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
    var outputPort = CalculatePort.CreateOutput<Process>();
    this.graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    outputPort.source = new SequenceActionParameter(false, () => {
      var propertyData = (PropertyData) CalculatePort.GetCalculatedValue(propertyPort);
      var property = new Yumuru.AnimationConstructor.FloatPropertyInfo(propertyData.gameObject.transform, propertyData.type, propertyData.propertyName);
      var time = (float) CalculatePort.GetCalculatedValue(timePort);
      var minValue = 0f;
      var maxValue = 1f;
      float targetValue;
      if (minValuePort.connected) {
        minValue = (float) CalculatePort.GetCalculatedValue(minValuePort);
      }
      if (maxValuePort.connected) {
        maxValue = (float) CalculatePort.GetCalculatedValue(maxValuePort);
      }
      AnimationCurve curve;
      if (curvePort.connected) {
        curve = (AnimationCurve) CalculatePort.GetCalculatedValue(curvePort);
      } else {
        curve = curveField.value;
      }
      return p => {
        p.constructor.AddImmediate(property, minValue);
        p.constructor.Add(property, maxValue, curve);
        p.constructor.Next(time);
        return p;
      };
    });
  }
  
  public PropertyCurveChangeNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializablePropertyCurveChangeNode());
  }
  public PropertyCurveChangeNode(AnimationGraphView graphView, SerializablePropertyCurveChangeNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
