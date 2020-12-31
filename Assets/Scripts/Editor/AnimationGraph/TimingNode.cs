using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableTimeNode {
  public SerializableAnimationGraphNode graphNode;
  public SerializableTimeNode(TimeNode node) {
    this.graphNode = new SerializableAnimationGraphNode(node);
  }
}
public class TimeNode : Node, IProcessNode {
  public AnimationGraphView graphView { get; set; }
  public string guid { get; set; }
  public ProcessPort processPort { get; set; }
  public List<ProcessPort> processPorts { get; set; } = new List<ProcessPort>();

  public void Initialize() {
    this.title = "Time";
    processPort = new ProcessPort();
    this.inputContainer.Add(processPort.CreateInput());
    this.outputContainer.Add(processPort.CreateOutput());
  }

  public void InitializeNew() {
    this.guid = Guid.NewGuid().ToString();
    this.Initialize();
  }

  public TimeNode() {}
  public TimeNode(SerializableTimeNode serializable) {
    serializable.graphNode.Load(this);
    this.Initialize();
  }
}
}
