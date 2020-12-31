using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnimationGraph {
[CreateAssetMenu(menuName = "AnimationGraph")]
public class AnimationGraphAsset : ScriptableObject {
  public List<SerializableAnimationGenerateNode> animationGenerateNodes = new List<SerializableAnimationGenerateNode>();
  public List<SerializableTimeNode> timeNodes = new List<SerializableTimeNode>();
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

    return asset;
  }
  public void LoadAsset(AnimationGraphView graphView) {
    foreach (var serializable in animationGenerateNodes) {
      graphView.AddElement(new AnimationGenerateNode(serializable));
    }
    foreach (var serializable in timeNodes) {
      graphView.AddElement(new TimeNode(serializable));
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
}
