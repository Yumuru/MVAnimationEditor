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
public class SerializableGameObjectFieldNode {
  public SerializableGraphNode graphNode;
  public SerializableGameObject gameObject;
  public string outputNodeGuid;
  public SerializableGameObjectFieldNode() {
    outputNodeGuid = Guid.NewGuid().ToString();
  }
  public SerializableGameObjectFieldNode(GameObjectFieldNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    var gameObject = node.gameObjectField.value as GameObject;
    if (gameObject != null) this.gameObject = new SerializableGameObject(gameObject);
    this.outputNodeGuid = node.outputPortGuid;
  }
}

public class GameObjectFieldNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public ObjectField gameObjectField { get; private set; }
  public string outputPortGuid;

  void SaveAsset(GraphAsset asset) {
    asset.gameObjectFieldNodes.Add(new SerializableGameObjectFieldNode(this));
  }

  void Construct(SerializableGameObjectFieldNode serializable) {
    this.title = "GameObject Field";

    this.gameObjectField = new ObjectField();
    this.gameObjectField.objectType = typeof(GameObject);
    this.gameObjectField.label = "Game Object";
    this.gameObjectField.value = serializable.gameObject?.Find();

    this.outputPortGuid = serializable.outputNodeGuid;
    var outputPort = CalculatePort.CreateOutput<GameObject>();
    this.graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    outputPort.source = new PortObject<GameObject>(() => { return gameObjectField.value as GameObject; });

    this.mainContainer.Add(gameObjectField);
  }
  
  public GameObjectFieldNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializableGameObjectFieldNode());
  }
  public GameObjectFieldNode(AnimationGraphView graphView, SerializableGameObjectFieldNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
