using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public delegate ProcessParameter SequenceAction(ProcessParameter processParameter);
public class SequenceActionParameter {
  public bool canUseProcessOutput;
  public Calculate<SequenceAction> Calculate; 
  public SequenceActionParameter(bool canUseProcessOutput, Calculate<SequenceAction> calculate) {
    this.canUseProcessOutput = canUseProcessOutput;
    this.Calculate = calculate;
  }
}

[Serializable]
public class SerializableNewSequenceNode {
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
    public Field(NewSequenceNode.Field field) {
      this.actionPortGuid = field.actionPortGuid;
      this.outputPortGuid = field.outputPortGuid;
    }
  }

  public List<Field> fields = new List<Field>();
  public SerializableNewSequenceNode() {
    inputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableNewSequenceNode(NewSequenceNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.inputPortGuid = node.inputPortGuid;
    foreach (var field in node.fields.fields) {
      this.fields.Add(new Field(field));
    }
  }
}
public class NewSequenceNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string inputPortGuid;

  public class Field : VisualElement {
    public Port actionPort;
    public string actionPortGuid;
    public Port outputPort;
    public string outputPortGuid;
    public Action OnRemove;
    public Field(NewSequenceNode node, SerializableNewSequenceNode.Field serializable) {
      this.style.flexDirection = FlexDirection.Row;

      actionPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(SequenceAction));
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
      var action = CalculatePort.GetCalculatedValue(actionPort, s => (s as SequenceActionParameter).Calculate);
      if (action != null) p = action(p);
      ProcessPort.Proceed(p, outputPort);
      return p;
    }
  }

  public class Fields : VisualElement {
    NewSequenceNode node;
    public List<Field> fields = new List<Field>();
    public Fields(NewSequenceNode node) {
      this.node = node;
    }

    public void AddField(SerializableNewSequenceNode.Field serializable) {
      var field = new Field(node, serializable);
      fields.Add(field);
      this.Add(field);

      field.OnRemove = () => {
        field.RemoveFromHierarchy();
        fields.Remove(field);
      };
    }
    
    public void Proceed(ProcessParameter p) {
      foreach (var field in fields) {
        p = field.Proceed(p);
      }
    }
  }

  public Fields fields;

  void Construct(SerializableNewSequenceNode serializable) {
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
      this.fields.AddField(new SerializableNewSequenceNode.Field());
    });

    this.mainContainer.Add(addFieldButton);

    inputPort.SetProceed(p => {
      fields.Proceed(p);
    });
  }

  void SaveAsset(GraphAsset asset) {
    asset.newSequenceNodes.Add(new SerializableNewSequenceNode(this));
  }

  // New
  public NewSequenceNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    var serializable = new SerializableNewSequenceNode();
    serializable.fields.Add(new SerializableNewSequenceNode.Field());
    this.Construct(serializable);
  }

  // Load
  public NewSequenceNode(GraphView graphView, SerializableNewSequenceNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
