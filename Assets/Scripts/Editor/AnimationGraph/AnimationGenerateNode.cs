using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using Yumuru;

namespace AnimationGraph {
using static UnityUtil;
[Serializable]
public class SerializableAnimationGenerateNode {
  public SerializableGraphNode graphNode;
  public string outputPortGuid;
  public AnimationClip clip;
  public SerializableGameObject rootGameObject;
  public SerializableAnimationGenerateNode() {
    this.outputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableAnimationGenerateNode(AnimationGenerateNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.outputPortGuid = node.outputPortGuid;
    this.clip = (AnimationClip) node.animationClipField.value;
    if (node.rootField.value is GameObject go) {
      this.rootGameObject = new SerializableGameObject(go); }
  }
}
public class AnimationGenerateNode : Node, IProcessNode {
  public IGraphNodeLogic graphNode { get; private set; }

  public ObjectField animationClipField;
  public ObjectField rootField;
  public string outputPortGuid;

  void SaveAsset(GraphAsset asset) {
    asset.animationGenerateNodes.Add(new SerializableAnimationGenerateNode(this));
  }

  public Action<ProcessParameter> Proceed { get; private set; }

  void Construct(SerializableAnimationGenerateNode serializable) {
    this.title = "AnimationGenerate";
    this.inputContainer.RemoveFromHierarchy();
    var outputPort = ProcessPort.CreateOutput();
    this.outputPortGuid = serializable.outputPortGuid;
    this.graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    this.animationClipField = new ObjectField();
    this.animationClipField.objectType = typeof(AnimationClip);
    this.animationClipField.label = "Animation Clip";
    this.animationClipField.value = serializable.clip;

    this.rootField = new ObjectField();
    this.rootField.objectType = typeof(GameObject);
    this.rootField.label = "Root Object";
    this.rootField.value = serializable.rootGameObject?.Find();

    this.mainContainer.Add(animationClipField);
    this.mainContainer.Add(rootField);

    this.Proceed = p => {
      ProcessNode.Proceed(p, outputPort);
    };

    this.mainContainer.Add(new Button(() => {
      var constructor = new AnimationConstructor(
        (rootField.value as GameObject).transform,
        animationClipField.value as AnimationClip);
      constructor.clip.ClearCurves();
      constructor.Construct(c => {
        this.Proceed(new ProcessParameter() {
          constructor = c,
          time = 0f,
          clip = animationClipField.value as AnimationClip,
        });
      });
    }));
  }
  
  public AnimationGenerateNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializableAnimationGenerateNode());
  }
  public AnimationGenerateNode(GraphView graphView, SerializableAnimationGenerateNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
