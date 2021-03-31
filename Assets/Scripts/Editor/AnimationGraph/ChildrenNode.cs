using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
using static UnityUtil;
[Serializable]
public class SerializableChildrenNode {
  public SerializableGraphNode graphNode;
  public string gameObjectPortGuid;
  public string outputNodeGuid;
  public SerializableChildrenNode() {
    gameObjectPortGuid = Guid.NewGuid().ToString();
    outputNodeGuid = Guid.NewGuid().ToString();
  }
  public SerializableChildrenNode(ChildrenNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.gameObjectPortGuid = node.gameObjectPortGuid;
    this.outputNodeGuid = node.outputPortGuid;
  }
}

public class ChildrenNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string gameObjectPortGuid;
  public string outputPortGuid;

  void SaveAsset(GraphAsset asset) {
    asset.childrenNodes.Add(new SerializableChildrenNode(this));
  }

  Type GetType(string typeName) {
    foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())) {
      if (type.Namespace == "UnityEngine" && type.Name == typeName) {
        return type;
      }
    }
    return null;
  }

  void Construct(SerializableChildrenNode serializable) {
    this.title = "Get Children";

    this.gameObjectPortGuid = serializable.gameObjectPortGuid;
    var gameObjectPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(GameObject));
    this.graphNode.RegisterPort(gameObjectPort, gameObjectPortGuid);
    this.inputContainer.Add(gameObjectPort);

    this.outputPortGuid = serializable.outputNodeGuid;
    var outputPort = CalculatePort.CreateOutput<List<GameObject>>();
    outputPort.portName = "List<GameObject>";
    this.graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    outputPort.source = new PortObject<List<object>>(() => {
      var gameObject = CalculatePort.GetCalculatedValue(gameObjectPort) as GameObject;
      return gameObject.GetComponentsInChildren<Transform>()
        .Where(t => t != gameObject.transform)
        .Select(t => t.gameObject)
        .Cast<object>()
        .ToList();
    });
  }
  
  public ChildrenNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializableChildrenNode());
  }
  public ChildrenNode(AnimationGraphView graphView, SerializableChildrenNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
