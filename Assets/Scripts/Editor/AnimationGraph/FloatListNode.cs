using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableFloatListNode {
  public SerializableGraphNode graphNode;
  public List<string> inputPortGuids = new List<string>();
  public string outputPortGuid;
  public SerializableFloatListNode() {
    inputPortGuids.Add(Guid.NewGuid().ToString());
    outputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableFloatListNode(FloatListNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    foreach (var field in node.fields) {
      this.inputPortGuids.Add(field.inputPortGuid); }
    this.outputPortGuid = node.outputPortGuid;
  }
}
public class FloatListNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public TextField constantNameField { get; private set; }
  public FloatField valueField { get; private set; }
  public string outputPortGuid { get; private set; }

  public class Field : VisualElement {
    public Action OnRemove { get; set; }
    public Port inputPort;
    public string inputPortGuid;
    public Field(FloatListNode node, string inputPortGuid) {
      this.inputPortGuid = inputPortGuid;
      this.style.flexDirection = FlexDirection.Row;
      inputPort = CalculatePort.CreateCalculateInPort<float>();
      node.graphNode.RegisterPort(inputPort, inputPortGuid);
      this.Add(inputPort);
      var deleteButton = new Button(() => OnRemove());
      deleteButton.text = "X";
      this.Add(deleteButton);
    }
  }
  public List<Field> fields = new List<Field>();

  void NewField(string inputPortGuid) {
    var field = new Field(this, inputPortGuid);
    field.OnRemove = () => {
        this.fields.Remove(field);
        this.graphNode.UnregisterPort(field.inputPort);
        field.RemoveFromHierarchy();
    };
    this.fields.Add(field);
    this.inputContainer.Add(field);
  }

  void Construct(SerializableFloatListNode serializable) {
    this.title = "Float List";

    foreach (var inputPortGuid in serializable.inputPortGuids) {
      NewField(inputPortGuid);
    }

    var button = new Button(() => {
      NewField(Guid.NewGuid().ToString());
    });
    button.text = "Add";
    this.mainContainer.Add(button);

    var outputPort = new BasicCalculatedOutPort<List<float>>();
    outputPort.portName = "List<float>";
    this.outputPortGuid = serializable.outputPortGuid;
    graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);
    
    outputPort.Calculate = () => {
      return fields
        .Where(f => f.inputPort.connected)
        .Select(field => CalculatePort.GetCalculatedValue<float>(field.inputPort))
        .ToList();
    };
  }
  
  void SaveAsset(GraphAsset asset) {
    asset.floatListNodes.Add(new SerializableFloatListNode(this));
  }

  // New
  public FloatListNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializableFloatListNode());
  }
  
  // Load
  public FloatListNode(GraphView graphView, SerializableFloatListNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
