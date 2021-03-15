using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializablePropertyNode {
  public SerializableGraphNode graphNode;
  public string outputNodeGuid;
  public SerializablePropertyNode() {
    outputNodeGuid = Guid.NewGuid().ToString();
  }
  public SerializablePropertyNode(PropertyNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.outputNodeGuid = node.outputPortGuid;
  }
}

public interface IPropertyData {
  Type type { get; set; }
}
public struct PropertyData {
  public string pathOfGameObject;
  public Type type;
  public string propertyName;
}

public class PropertyNode : Node, ICalculateNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public ObjectField gameObjectField { get; private set; }
  public Dictionary<Port, Func<object>> Calculate { get; } = new Dictionary<Port, Func<object>>();
  public string outputPortGuid;

  void SaveAsset(GraphAsset asset) {
    asset.propertyNodes.Add(new SerializablePropertyNode(this));
  }

  void Construct(SerializablePropertyNode serializable) {
    this.title = "Property";

    this.gameObjectField = new ObjectField();
    this.gameObjectField.objectType = typeof(GameObject);
    this.gameObjectField.label = "Game Object";

    this.outputPortGuid = serializable.outputNodeGuid;
    var outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(PropertyData));
    this.graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    this.Calculate[outputPort] = () => {
      return new PropertyData() {
      };
    };

    this.mainContainer.Add(gameObjectField);
  }
  
  public PropertyNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializablePropertyNode());
  }
  public PropertyNode(GraphView graphView, SerializablePropertyNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
