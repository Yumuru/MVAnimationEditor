using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public class SequenceActionParameter : IPortObject<Proceed> {
  public bool canUseProcessOutput;
  public Func<Proceed> getter { get; private set; }
  public SequenceActionParameter(bool canUseProcessOutput, Func<Proceed> getter) {
    this.canUseProcessOutput = canUseProcessOutput;
    this.getter = getter;
  }
}

[Serializable]
public class SerializableSequenceNode {
  public SerializableGraphNode graphNode;
  public string inputPortGuid;

  [Serializable]
  public class Field {
    public string actionPortGuid;
    public string outputPortGuid;
    public Field() {
      this.actionPortGuid = Guid.NewGuid().ToString();
      this.outputPortGuid = Guid.NewGuid().ToString();
    }
    public Field(SequenceNode.Field field) {
      this.actionPortGuid = field.actionPortGuid;
      this.outputPortGuid = field.outputPortGuid;
    }
  }

  public List<Field> fields = new List<Field>();
  public SerializableSequenceNode() {
    inputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableSequenceNode(SequenceNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.inputPortGuid = node.inputPortGuid;
    foreach (var field in node.fields.fields) {
      this.fields.Add(new Field(field));
    }
  }
}
public class SequenceNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string inputPortGuid;

  public class Field : VisualElement {
    public Port actionPort;
    public string actionPortGuid;
    public Port outputPort;
    public string outputPortGuid;
    public Action OnRemove;
    public Field(SequenceNode node, SerializableSequenceNode.Field serializable) {
      this.style.flexDirection = FlexDirection.Row;

      actionPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Proceed));
      this.actionPortGuid = serializable.actionPortGuid;
      node.graphNode.RegisterPort(actionPort, actionPortGuid);

      outputPort = ProcessPort.CreateOutput();
      this.outputPortGuid = serializable.outputPortGuid;
      node.graphNode.RegisterPort(outputPort, outputPortGuid);

      OnRemove += () => {
        actionPort.RemoveFromHierarchy();
        outputPort.RemoveFromHierarchy();
        node.graphNode.UnregisterPort(actionPort);
        node.graphNode.UnregisterPort(outputPort);
      };

      var deleteButton = new Button(() => OnRemove());
      deleteButton.text = "X";

      this.Add(actionPort);
      this.Add(deleteButton);
      this.Add(outputPort);
    }

    public ProcessParameter Proceed(ProcessParameter p) {
      var action = CalculatePort.GetCalculatedValue<Proceed>(actionPort);
      if (action != null) p = action(p);
      ProcessPort.Proceed(p, outputPort);
      return p;
    }
  }

  public class Fields : VisualElement {
    SequenceNode node;
    public List<Field> fields = new List<Field>();
    public Fields(SequenceNode node) {
      this.node = node;
    }

    public void AddField(SerializableSequenceNode.Field serializable) {
      var field = new Field(node, serializable);
      fields.Add(field);
      this.Add(field);

      field.OnRemove = () => {
        field.RemoveFromHierarchy();
        fields.Remove(field);
      };
    }
    
    public ProcessParameter Proceed(ProcessParameter p) {
      foreach (var field in fields) {
        p = field.Proceed(p);
      }
      return p;
    }
  }

  public Fields fields;

  void Construct(SerializableSequenceNode serializable) {
    this.title = "Sequence";
    this.outputContainer.RemoveFromHierarchy();
    var inputPort = ProcessPort.CreateInput();
    this.inputPortGuid = serializable.inputPortGuid;
    graphNode.RegisterPort(inputPort, serializable.inputPortGuid);
    this.inputContainer.Add(inputPort);

    this.fields = new Fields(this);
    this.mainContainer.Add(this.fields);

    foreach (var sField in serializable.fields) {
      this.fields.AddField(sField);
    }

    var addFieldButton = new Button(() => {
      this.fields.AddField(new SerializableSequenceNode.Field());
    });

    this.mainContainer.Add(addFieldButton);

    inputPort.SetProceed(p => {
      return fields.Proceed(p);
    });
  }

  void SaveAsset(GraphAsset asset) {
    asset.sequenceNodes.Add(new SerializableSequenceNode(this));
  }

  // New
  public SequenceNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    var serializable = new SerializableSequenceNode();
    serializable.fields.Add(new SerializableSequenceNode.Field());
    this.Construct(serializable);
  }

  // Load
  public SequenceNode(AnimationGraphView graphView, SerializableSequenceNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
