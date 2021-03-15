using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableAnimationGenerateNode {
  public SerializableGraphNode graphNode;
  public string outputPortGuid;
  public AnimationClip clip;
  public SerializableAnimationGenerateNode(AnimationGenerateNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.outputPortGuid = node.outputPortGuid;
    this.clip = (AnimationClip) node.animationClipField.value;
  }
}
public class AnimationGenerateNode : Node, IProcessNode {
  public IGraphNodeLogic graphNode { get; private set; }

  public ObjectField animationClipField;
  public string outputPortGuid;

  void SaveAsset(GraphAsset asset) {
    asset.animationGenerateNodes.Add(new SerializableAnimationGenerateNode(this));
  }

  public Action<ProcessParameter> Proceed { get; private set; }

  void Construct() {
    this.title = "AnimationGenerate";
    this.inputContainer.RemoveFromHierarchy();
    var outputPort = ProcessPort.CreateOutput();
    this.graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    this.animationClipField = new ObjectField();
    this.animationClipField.objectType = typeof(AnimationClip);
    this.animationClipField.label = "Animation Clip";
    this.mainContainer.Add(animationClipField);

    this.Proceed = p => {
      foreach (var edge in outputPort.connections) {
        var node = edge.input.node as IProcessNode;
        node.Proceed?.Invoke(p);
      }
    };

    this.mainContainer.Add(new Button(() => {
      this.Proceed(new ProcessParameter() {
        time = 0f,
        clip = animationClipField.value as AnimationClip,
      });
    }));
  }
  
  public AnimationGenerateNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    outputPortGuid = Guid.NewGuid().ToString();
    this.Construct();
  }
  public AnimationGenerateNode(GraphView graphView, SerializableAnimationGenerateNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(graphNode as GraphNodeLogic);
    this.outputPortGuid = serializable.outputPortGuid;
    this.Construct();
    animationClipField.value = serializable.clip;
  }
}
}
