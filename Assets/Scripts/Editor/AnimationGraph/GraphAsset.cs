using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
using static UnityUtil;
[Serializable]
public class SerializableGameObject {
  public string assetPath;
  public string hierarchyPath;
  public SerializableGameObject(SerializableGameObject serializable) {
    this.assetPath = serializable.assetPath;
    this.hierarchyPath = serializable.hierarchyPath;
  }
  public SerializableGameObject(GameObject gameObject) {
    if (gameObject == null) return;
    this.assetPath = AssetDatabase.GetAssetOrScenePath(gameObject);
    this.hierarchyPath = GetHeirarchyPath(gameObject.transform);
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

[CreateAssetMenu(menuName = "AnimationGraph")]
public class GraphAsset : ScriptableObject {
  public Vector3 viewPosition;
  public Vector3 viewScale;
  public List<SerializableAnimationGenerateNode> animationGenerateNodes = new List<SerializableAnimationGenerateNode>();
  public List<SerializableGameObjectFieldNode> gameObjectFieldNodes = new List<SerializableGameObjectFieldNode>();
  public List<SerializablePropertyNode> propertyNodes = new List<SerializablePropertyNode>();
  public List<SerializablePropertyTransitionNode> propertyTransitionNodes = new List<SerializablePropertyTransitionNode>();
  public List<SerializableSequenceNode> sequenceNodes = new List<SerializableSequenceNode>();
  public List<SerializableForloopNode> forloopNodes = new List<SerializableForloopNode>();
  public List<SerializableTimeNode> timeNodes = new List<SerializableTimeNode>();
  public List<SerializablePropertyValueChangeNode> propertyValueChangeNodes = new List<SerializablePropertyValueChangeNode>();
  public List<SerializableTestNode> testNodes = new List<SerializableTestNode>();
  public List<SerializableFloatValueNode> floatValueNodes = new List<SerializableFloatValueNode>();
  public List<SerializableFloatListNode> floatListNodes = new List<SerializableFloatListNode>();
  public List<SerializableCurveNode> curveNodes = new List<SerializableCurveNode>();
  public List<SerializableMultiplyNode> multiplyNodes = new List<SerializableMultiplyNode>();
  public List<SerializableEdge> edges = new List<SerializableEdge>();
  public void SaveAsset(AnimationGraphView graphView) {
    viewPosition = graphView.viewTransform.position;
    viewScale = graphView.viewTransform.scale;

    animationGenerateNodes.Clear();
    gameObjectFieldNodes.Clear();
    propertyNodes.Clear();
    propertyTransitionNodes.Clear();
    sequenceNodes.Clear();
    forloopNodes.Clear();
    timeNodes.Clear();
    propertyValueChangeNodes.Clear();
    testNodes.Clear();
    floatValueNodes.Clear();
    floatListNodes.Clear();
    curveNodes.Clear();
    multiplyNodes.Clear();
    edges.Clear();

    graphView.nodes.ForEach(n => {
      var node = (n as IGraphNode).graphNode;
      node.SaveAsset(this);
    });

    graphView.edges.ForEach(edge => {
      var outputNode = (edge.output.node as IGraphNode).graphNode;
      var inputNode = (edge.input.node as IGraphNode).graphNode;
      string outputPortGuid, inputPortGuid;
      if (outputNode.portGuids.TryGetValue(edge.output, out outputPortGuid)
        && inputNode.portGuids.TryGetValue(edge.input, out inputPortGuid)) {
        this.edges.Add(new SerializableEdge() {
          outputPortGuid = outputPortGuid,
          inputPortGuid = inputPortGuid
        });
      }
    });

    EditorUtility.SetDirty(this);
    AssetDatabase.SaveAssets();
  }
  public void LoadAsset(AnimationGraphView graphView) {
    graphView.viewTransform.position = viewPosition;
    graphView.viewTransform.scale = viewScale;

    foreach (var serializable in animationGenerateNodes) {
      graphView.AddElement(new AnimationGenerateNode(graphView, serializable)); }
    foreach (var serializable in gameObjectFieldNodes) {
      graphView.AddElement(new GameObjectFieldNode(graphView, serializable)); }
    foreach (var serializable in propertyNodes) {
      graphView.AddElement(new PropertyNode(graphView, serializable)); }
    foreach (var serializable in propertyTransitionNodes) {
      graphView.AddElement(new PropertyTransitionNode(graphView, serializable)); }
    foreach (var serializable in sequenceNodes) {
      graphView.AddElement(new SequenceNode(graphView, serializable)); }
    foreach (var serializable in forloopNodes) {
      graphView.AddElement(new ForloopNode(graphView, serializable)); }
    foreach (var serializable in timeNodes) {
      graphView.AddElement(new TimeNode(graphView, serializable)); }
    foreach (var serializable in propertyValueChangeNodes) {
      graphView.AddElement(new PropertyValueChangeNode(graphView, serializable)); }
    foreach (var serializable in testNodes) {
      graphView.AddElement(new TestNode(graphView, serializable)); }
    foreach (var serializable in floatValueNodes) {
      graphView.AddElement(new FloatValueNode(graphView, serializable)); }
    foreach (var serializable in floatListNodes) {
      graphView.AddElement(new FloatListNode(graphView, serializable)); }
    foreach (var serializable in curveNodes) {
      graphView.AddElement(new CurveNode(graphView, serializable)); }
    foreach (var serializable in multiplyNodes) {
      graphView.AddElement(new MultiplyNode(graphView, serializable)); }

    var nodes = graphView.nodes.ToList()
      .Cast<IGraphNode>().Select(n => n.graphNode).ToList();
    foreach (var e in this.edges) {
      Port outputPort = null, inputPort = null;
      foreach (var n in nodes) {
        if (n.guidPorts.TryGetValue(e.outputPortGuid, out outputPort)) break;
      }
      foreach (var n in nodes) {
        if (n.guidPorts.TryGetValue(e.inputPortGuid, out inputPort)) break;
      }
      if (outputPort == null || inputPort == null) break;
      var edge = outputPort.ConnectTo(inputPort);
      graphView.Add(edge);
    }
  }
}
[Serializable]
public class SerializableEdge {
  public string outputPortGuid;
  public string inputPortGuid;
}
}
