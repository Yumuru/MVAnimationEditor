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
public class SerializablePropertyNode {
  public SerializableGraphNode graphNode;
  public string gameObjectPortGuid;
  public string typeName;
  public string propertyName;
  public string outputNodeGuid;
  public SerializablePropertyNode() {
    gameObjectPortGuid = Guid.NewGuid().ToString();
    outputNodeGuid = Guid.NewGuid().ToString();
  }
  public SerializablePropertyNode(PropertyNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.typeName = node.typeNameField.value;
    this.propertyName = node.propertyNameField.value;
    this.gameObjectPortGuid = node.gameObjectPortGuid;
    this.outputNodeGuid = node.outputPortGuid;
  }
}

public struct PropertyData {
  public GameObject gameObject;
  public string gameObjectHierarchyPath;
  public Type type;
  public string propertyName;
}

public class PropertyNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public TextField typeNameField { get; private set; }
  public TextField propertyNameField { get; private set; }
  public string gameObjectPortGuid;
  public string outputPortGuid;

  void SaveAsset(GraphAsset asset) {
    asset.propertyNodes.Add(new SerializablePropertyNode(this));
  }

  Type GetType(string typeName) {
    foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())) {
      if (type.Namespace == "UnityEngine" && type.Name == typeName) {
        return type;
      }
    }
    return null;
  }

  void Construct(SerializablePropertyNode serializable) {
    this.title = "Property";

    this.gameObjectPortGuid = serializable.gameObjectPortGuid;
    var gameObjectPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(GameObject));
    this.graphNode.RegisterPort(gameObjectPort, gameObjectPortGuid);
    this.inputContainer.Add(gameObjectPort);

    this.typeNameField = new TextField();
    this.typeNameField.label = "Type Name";
    this.typeNameField.value = serializable.typeName;

    this.propertyNameField = new TextField();
    this.propertyNameField.label = "Property Name";
    this.propertyNameField.value = serializable.propertyName;

    this.outputPortGuid = serializable.outputNodeGuid;
    var outputPort = CalculatePort.CreateOutput<PropertyData>();
    this.graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    outputPort.source = new PortObject<PropertyData>(() => {
      var gameObject = CalculatePort.GetCalculatedValue<GameObject>(gameObjectPort);
      return new PropertyData() {
        gameObject = gameObject,
        gameObjectHierarchyPath = GetHeirarchyPath(gameObject.transform),
        type = GetType(typeNameField.value),
        propertyName = propertyNameField.value
      };
    });

    this.mainContainer.Add(typeNameField);
    this.mainContainer.Add(propertyNameField);
  }
  
  public PropertyNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializablePropertyNode());
  }
  public PropertyNode(AnimationGraphView graphView, SerializablePropertyNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
