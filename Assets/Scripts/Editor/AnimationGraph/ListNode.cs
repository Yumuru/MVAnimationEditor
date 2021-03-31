using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public class SerializableListNode<T> {
  public SerializableGraphNode graphNode;
  public List<string> inputPortGuids = new List<string>();
  public string outputPortGuid;
  public SerializableListNode() {
    inputPortGuids.Add(Guid.NewGuid().ToString());
    outputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableListNode(ListNode<T> node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    foreach (var field in node.fields) {
      this.inputPortGuids.Add(field.inputPortGuid); }
    this.outputPortGuid = node.outputPortGuid;
  }
}
public abstract class ListNode<T> : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public TextField constantNameField { get; private set; }
  public FloatField valueField { get; private set; }
  public string outputPortGuid { get; private set; }

  public class Field : VisualElement {
    public Action OnRemove { get; set; }
    public Port inputPort;
    public string inputPortGuid;
    public Field(ListNode<T> node, string inputPortGuid) {
      this.inputPortGuid = inputPortGuid;
      this.style.flexDirection = FlexDirection.Row;
      inputPort = CalculatePort.CreateInput<T>();
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

  void Construct(SerializableListNode<T> serializable) {
    this.title = String.Format("List of {0}", typeof(T).Name);

    foreach (var inputPortGuid in serializable.inputPortGuids) {
      NewField(inputPortGuid);
    }

    var button = new Button(() => {
      NewField(Guid.NewGuid().ToString());
    });
    button.text = "Add";
    this.mainContainer.Add(button);

    var outputPort = CalculatePort.CreateOutput<List<object>>();
    outputPort.portName = String.Format("List<{0}>", typeof(T).Name);
    this.outputPortGuid = serializable.outputPortGuid;
    graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);
    
    outputPort.source = new PortObject<List<object>>(() => {
      return fields
        .Where(f => f.inputPort.connected)
        .Select(field => CalculatePort.GetCalculatedValue(field.inputPort))
        .ToList();
    });
  }

  protected abstract void SaveAsset(GraphAsset asset);

  // New
  public ListNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializableListNode<T>());
  }
  
  // Load
  public ListNode(AnimationGraphView graphView, SerializableListNode<T> serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
[Serializable]
public class SerializableFloatListNode : SerializableListNode<float> {
  public SerializableFloatListNode() : base() { }
  public SerializableFloatListNode(FloatListNode node) : base(node) { }
}
public class FloatListNode : ListNode<float>, IGraphNode {
  protected override void SaveAsset(GraphAsset asset) {
    asset.floatListNodes.Add(new SerializableFloatListNode(this));
  }

  public FloatListNode() : base() {}
  public FloatListNode(AnimationGraphView graphView, SerializableFloatListNode serializable) : base(graphView, serializable) {}
}
[Serializable]
public class SerializableGameObjectListNode : SerializableListNode<GameObject> {
  public SerializableGameObjectListNode() : base() { }
  public SerializableGameObjectListNode(GameObjectListNode node) : base(node) { }
}
public class GameObjectListNode : ListNode<GameObject>, IGraphNode {
  protected override void SaveAsset(GraphAsset asset) {
    asset.gameObjectListNodes.Add(new SerializableGameObjectListNode(this));
  }

  public GameObjectListNode() : base() {}
  public GameObjectListNode(AnimationGraphView graphView, SerializableGameObjectListNode serializable) : base(graphView, serializable) {}
}
}
