using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
using static CalculateNode;
[Serializable]
public class SerializableSequenceNode {
  public enum FieldType {
    Time,
    PropertyTransition,
  }
  [Serializable]
  public class Field {
    public FieldType type;
    public float time;
    public string timePortGuid;
    public string outputPortGuid;
    public string propertyCurvePortGuid;
    public Field(FieldType type) {
      this.type = type;
      this.timePortGuid = Guid.NewGuid().ToString();
      this.propertyCurvePortGuid = Guid.NewGuid().ToString();
      this.outputPortGuid = Guid.NewGuid().ToString();
    }
    public Field(SequenceNode.SequenceField.IField field) {
      if (field is SequenceNode.SequenceField.TimeField timeField) {
        this.type = FieldType.Time;
        this.time = timeField.time.value;
        this.timePortGuid = timeField.timePortGuid;
        this.outputPortGuid = timeField.outputPortGuid;
      } else if (field is SequenceNode.SequenceField.PropertyTransitionInput propertyCurve) {
        this.type = FieldType.PropertyTransition;
        this.propertyCurvePortGuid = propertyCurve.propertyCurvePortGuid;
      }
    }
  }
  public SerializableGraphNode graphNode;
  public string inputPortGuid;
  public List<Field> fields = new List<Field>();
  public SerializableSequenceNode() {
    inputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableSequenceNode(SequenceNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.inputPortGuid = node.inputPortGuid;
    foreach (var field in node.sequenceField.fields) {
      this.fields.Add(new Field(field));
    }
  }
}
public class SequenceNode : Node, IProcessNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string inputPortGuid;
  public Action<ProcessParameter> Proceed { get; private set; }
  public class SequenceField : VisualElement {
    public interface IField { }
    public class TimeField : VisualElement, IField {
      public FloatField time;
      public Port timePort;
      public string timePortGuid;
      public Port outputPort;
      public string outputPortGuid;
      public Action OnRemove { get; set; }
      public TimeField(SerializableSequenceNode.Field serializable) {
        this.style.flexDirection = FlexDirection.Row;
        time = new FloatField();
        time.label = "Time";
        time.labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
        time.labelElement.style.minWidth = 50;
        time.value = serializable.time;
        timePort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
        timePort.portName = "";
        timePortGuid = serializable.timePortGuid;
        this.RegisterCallback((GeometryChangedEvent evt) => {
          //time.SetEnabled(!timePort.connected);
        });
        outputPort = ProcessPort.CreateOutput();
        outputPortGuid = serializable.outputPortGuid;
        this.Add(timePort);
        this.Add(time);
        var deleteButton = new Button(() => OnRemove());
        deleteButton.text = "X";
        this.Add(deleteButton);
        this.Add(outputPort);
      }
    }
    public class PropertyTransitionInput : VisualElement, IField {
      public Port propertyCurvePort;
      public string propertyCurvePortGuid;
      public Action OnRemove { get; set; }
      public PropertyTransitionInput(SerializableSequenceNode.Field serializable) {
        this.style.flexDirection = FlexDirection.Row;
        propertyCurvePort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(PropertyTransition));
        propertyCurvePortGuid = serializable.propertyCurvePortGuid;
        var deleteButton = new Button(() => OnRemove());
        deleteButton.text = "X";
        this.Add(propertyCurvePort);
        this.Add(deleteButton);
      }
    }
    public SequenceNode node;
    public List<IField> fields = new List<IField>();
    public SequenceField(SequenceNode node) { this.node = node; }
    public void AddField(TimeField field) {
      node.graphNode.RegisterPort(field.timePort, field.timePortGuid);
      node.graphNode.RegisterPort(field.outputPort, field.outputPortGuid);
      fields.Add(field);
      this.Add(field);

      field.OnRemove = () => {
        fields.Remove(field);
        node.graphNode.UnregisterPort(field.timePort);
        node.graphNode.UnregisterPort(field.outputPort);
        field.RemoveFromHierarchy();
      };
    }

    public void AddField(PropertyTransitionInput field) {
      node.graphNode.RegisterPort(field.propertyCurvePort, field.propertyCurvePortGuid);
      fields.Add(field);
      this.Add(field);

      field.OnRemove = () => {
        fields.Remove(field);
        node.graphNode.UnregisterPort(field.propertyCurvePort);
        field.RemoveFromHierarchy();
      };
    }

    public void Proceed(ProcessParameter process) {
      foreach (var field in fields) {
        if (field is TimeField timeField) {
          if (timeField.timePort.connected) {
            process.time += GetCulculatedValue<float>(timeField.timePort);
          } else {
            process.time += timeField.time.value;
          }
          process.constructor.TSet(process.time);
          foreach (var edge in timeField.outputPort.connections) {
            var node = edge.input.node as IProcessNode;
            node.Proceed?.Invoke(process);
          }
        }
        if (field is PropertyTransitionInput propertyTransitionInput) {
          if (propertyTransitionInput.propertyCurvePort.connected) {
            var propertyTransition = GetCulculatedValue<PropertyTransition>(propertyTransitionInput.propertyCurvePort);
            var property = new Yumuru.AnimationConstructor.FloatPropertyInfo(
              propertyTransition.property.gameObject.transform,
              propertyTransition.property.type,
              propertyTransition.property.propertyName);
            process.constructor.Add(property, propertyTransition.targetValue, propertyTransition.curve);
          }
        }
      }
    }
  }
  public SequenceField sequenceField;

  void Construct(SerializableSequenceNode serializable) {
    this.title = "Sequence";
    this.outputContainer.RemoveFromHierarchy();
    var inputPort = ProcessPort.CreateInput();
    this.inputPortGuid = serializable.inputPortGuid;
    graphNode.RegisterPort(inputPort, serializable.inputPortGuid);
    this.inputContainer.Add(inputPort);

    sequenceField = new SequenceField(this);
    this.mainContainer.Add(sequenceField);

    foreach(var serializableSequenceField in serializable.fields) {
      if (serializableSequenceField.type == SerializableSequenceNode.FieldType.Time) {
        sequenceField.AddField(new SequenceField.TimeField(serializableSequenceField));
      } if (serializableSequenceField.type == SerializableSequenceNode.FieldType.PropertyTransition) {
        sequenceField.AddField(new SequenceField.PropertyTransitionInput(serializableSequenceField));
      }
    }

    var propertyCurveButton = new Button(() => {
      sequenceField.AddField(new SequenceField.PropertyTransitionInput(new SerializableSequenceNode.Field(SerializableSequenceNode.FieldType.PropertyTransition)));
    });
    propertyCurveButton.text = "Add PropertyCurve";
    var timeButton = new Button(() => {
      sequenceField.AddField(new SequenceField.TimeField(new SerializableSequenceNode.Field(SerializableSequenceNode.FieldType.Time)));
    });
    timeButton.text = "Add Time";
    this.mainContainer.Add(propertyCurveButton);
    this.mainContainer.Add(timeButton);

    this.Proceed = p => {
      sequenceField.Proceed(p);
    };
  }

  void SaveAsset(GraphAsset asset) {
    asset.sequenceNodes.Add(new SerializableSequenceNode(this));
  }

  // New
  public SequenceNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    var serializable = new SerializableSequenceNode();
    serializable.fields.Add(new SerializableSequenceNode.Field(SerializableSequenceNode.FieldType.Time));
    this.Construct(serializable);
  }

  // Load
  public SequenceNode(GraphView graphView, SerializableSequenceNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
