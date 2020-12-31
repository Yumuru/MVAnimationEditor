using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[CreateAssetMenu(menuName = "AnimationGraph")]
public class AnimationGraphAsset : ScriptableObject {
  public List<SerializableAnimationGenerateNode> animationGenerateNodes = new List<SerializableAnimationGenerateNode>();
  public List<SerializableTimeNode> timeNodes = new List<SerializableTimeNode>();
  public List<SerializableEdge> edges = new List<SerializableEdge>();
  public static AnimationGraphAsset SaveAsset(AnimationGraphView graphView) {
    var asset = new AnimationGraphAsset();

    graphView.nodes.ForEach(node => {
      switch(node) {
        case AnimationGenerateNode agn :
          asset.animationGenerateNodes.Add(new SerializableAnimationGenerateNode(agn));
          break;
        case TimeNode tn :
          asset.timeNodes.Add(new SerializableTimeNode(tn));
          break;
      }
    });

    graphView.edges.ForEach(edge => {
      var outputNode = edge.output.node as IAnimationGraphNode;
      var inputNode = edge.input.node as IAnimationGraphNode;
      asset.edges.Add(new SerializableEdge() {
        fromNodeGuid = outputNode.guid,
        toNodeGuid = inputNode.guid
      });
    });

    return asset;
  }
  public void LoadAsset(AnimationGraphView graphView) {
    foreach (var serializable in animationGenerateNodes) {
      graphView.AddElement(new AnimationGenerateNode(serializable, graphView)); }
    foreach (var serializable in timeNodes) {
      graphView.AddElement(new TimeNode(serializable, graphView)); }
    var nodes = graphView.nodes.ToList().Cast<IAnimationGraphNode>().ToList();
    foreach (var fromNode in nodes) {
      var edges = this.edges.Where(e => e.fromNodeGuid == fromNode.guid);
      foreach (var e in edges) {
        var toNode = nodes.First(n => n.guid == e.toNodeGuid);
        var outputPort = ((Node)fromNode).outputContainer.Q<Port>();
        var inputPort = ((Node)toNode).inputContainer.Q<Port>();
        var edge = outputPort.ConnectTo(inputPort);
        graphView.Add(edge);
      }
    }
  }
}
[Serializable]
public class SerializableAnimationGraphNode {
  public string guid;
  public Vector2 position;

  public SerializableAnimationGraphNode(IAnimationGraphNode node) {
    this.guid = node.guid;
    this.position = ((VisualElement)node).transform.position;
  }

  public void Load(IAnimationGraphNode node) {
    node.guid = guid;
    ((VisualElement)node).transform.position = position;
  }
}
[Serializable]
public class SerializableEdge {
  public string fromNodeGuid;
  public string toNodeGuid;
}
}
