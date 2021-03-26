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
[Serializable]
public class SerializableGameObject {
  public string assetPath;
  public string hierarchyPath;
  public SerializableGameObject(SerializableGameObject serializable) {
    this.assetPath = serializable.assetPath;
    this.hierarchyPath = serializable.hierarchyPath;
  }

  static StringBuilder builder = new StringBuilder();
  public static string GetHeirarchyPath(GameObject gameObject) {
    builder.Clear();
    builder.Insert(0, gameObject.name);
    var transform = gameObject.transform.parent;
    while(transform != null) {
      builder.Insert(0, transform.gameObject.name + "/");
      transform = transform.parent;
    }
    return builder.ToString();
  }
  public SerializableGameObject(GameObject gameObject) {
    this.assetPath = AssetDatabase.GetAssetOrScenePath(gameObject);
    this.hierarchyPath = GetHeirarchyPath(gameObject);
  }
  public GameObject Find() {
    if (assetPath == null || hierarchyPath == null) return null;
    var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>()
      .Where(go => AssetDatabase.GetAssetOrScenePath(go) == assetPath);
    var names = hierarchyPath.Split('/').Reverse();
    foreach (var go in gameObjects) {
      var transform = go.transform;
      foreach (var name in names) {
        if (name != transform.gameObject.name) break;
        if (transform.parent == null) return go;
        transform = transform.parent;
      }
    }
    return null;
  }
}
[Serializable]
public class SerializablePropertyNode {
  public SerializableGraphNode graphNode;
  public SerializableGameObject gameObject;
  public string typeName;
  public string propertyName;
  public string outputNodeGuid;
  public SerializablePropertyNode() {
    outputNodeGuid = Guid.NewGuid().ToString();
  }
  public SerializablePropertyNode(PropertyNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    var gameObject = node.gameObjectField.value as GameObject;
    if (gameObject != null) this.gameObject = new SerializableGameObject(gameObject);
    this.typeName = node.typeNameField.value;
    this.propertyName = node.propertyNameField.value;
    this.outputNodeGuid = node.outputPortGuid;
  }
}

public struct PropertyData {
  public GameObject gameObject;
  public Type type;
  public string propertyName;
}

public class PropertyNode : Node, ICalculateNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public ObjectField gameObjectField { get; private set; }
  public TextField typeNameField { get; private set; }
  public TextField propertyNameField { get; private set; }
  public Dictionary<Port, Func<object>> Calculate { get; } = new Dictionary<Port, Func<object>>();
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

    this.gameObjectField = new ObjectField();
    this.gameObjectField.objectType = typeof(GameObject);
    this.gameObjectField.label = "Game Object";
    this.gameObjectField.value = serializable.gameObject.Find();

    this.typeNameField = new TextField();
    this.typeNameField.label = "Type Name";
    this.typeNameField.value = serializable.typeName;

    this.propertyNameField = new TextField();
    this.propertyNameField.label = "Property Name";
    this.propertyNameField.value = serializable.propertyName;

    this.outputPortGuid = serializable.outputNodeGuid;
    var outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(PropertyData));
    this.graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    this.Calculate[outputPort] = () => {
      return new PropertyData() {
        gameObject = gameObjectField.value as GameObject,
        type = GetType(typeNameField.value),
        propertyName = propertyNameField.value
      };
    };

    this.mainContainer.Add(gameObjectField);
    this.mainContainer.Add(typeNameField);
    this.mainContainer.Add(propertyNameField);
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
