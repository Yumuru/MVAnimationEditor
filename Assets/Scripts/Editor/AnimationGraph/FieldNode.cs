using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public class SerializableFieldNode<T> {
  public SerializableGraphNode graphNode;
  public string outputPortGuid;
  public SerializableFieldNode() {
    outputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableFieldNode(FieldNode<T> node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
  }
}
public abstract class FieldNode<T> : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string outputPortGuid;

  public VisualElement fieldElement = new VisualElement();

  protected abstract void SaveAsset(GraphAsset asset);

  void Construct(SerializableFieldNode<T> serializable) {
    this.titleContainer.RemoveFromHierarchy();
    this.inputContainer.RemoveFromHierarchy();

    var outputPort = CalculatePort.CreateOutput<float>();
    this.outputPortGuid = serializable.outputPortGuid;
    graphNode.RegisterPort(outputPort, outputPortGuid);

    this.outputContainer.style.flexDirection = FlexDirection.Row;
    this.outputContainer.Add(fieldElement);
    this.outputContainer.Add(outputPort);
  }

  public FieldNode(AnimationGraphView graphView, Port inputPort, SerializableFieldNode<T> serializable) {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(new SerializableFieldNode<T>());
  }
}
}