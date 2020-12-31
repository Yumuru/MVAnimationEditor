using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableAnimationGenerateNode {
  public SerializableAnimationGraphNode graphNode;
  public AnimationClip clip;
  public SerializableAnimationGenerateNode(AnimationGenerateNode node) {
    this.graphNode = new SerializableAnimationGraphNode(node);
    this.clip = (AnimationClip) node.animationClipField.value;
  }
}
public class AnimationGenerateNode : Node, IProcessNode {
  public AnimationGraphView graphView { get; set; }
  public string guid { get; set; }
  public ProcessPort processPort { get; set; }

  public ObjectField animationClipField;

  public void Initialize() {
    this.title = "AnimationGenerate";
    processPort = new ProcessPort();
    this.outputContainer.Add(processPort.CreateOutput());

    animationClipField = new ObjectField();
    animationClipField.objectType = typeof(AnimationClip);
    animationClipField.label = "Animation Clip";
    this.extensionContainer.Add(animationClipField);
  }

  public void InitializeNew(AnimationGraphView graphView) {
    this.guid = Guid.NewGuid().ToString();
    this.graphView = graphView;
    this.Initialize();
  }
  
  public AnimationGenerateNode() {}
  public AnimationGenerateNode(SerializableAnimationGenerateNode serializable, AnimationGraphView graphView) {
    serializable.graphNode.Load(this);
    this.graphView = graphView;
    this.Initialize();
    animationClipField.value = serializable.clip;
  }
}
}
