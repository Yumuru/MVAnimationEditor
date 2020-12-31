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
  public class TimeProperty {
    public Port timePort;
    public ProcessPort processPort;
  }
  public List<TimeProperty> timeProperties = new List<TimeProperty>();

  public VisualElement timeFields;

  public void Initialize() {
    this.title = "Time";
    processPort = new ProcessPort();
    this.inputContainer.Add(processPort.CreateInput());
    this.outputContainer.Add(processPort.CreateOutput());

    timeFields = new VisualElement();
    this.mainContainer.Add(timeFields);
    var button = new Button(() => {
      var timePort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
      var ve = new VisualElement();
      ve.style.flexDirection = FlexDirection.Row;
      ve.Add(timePort);
      var processPort = new ProcessPort();
      ve.Add(processPort.CreateOutput());
      timeProperties.Add(new TimeProperty() { timePort = timePort, processPort = processPort });
      timeFields.Add(ve);
    });
    this.mainContainer.Add(button);
  }

  public void InitializeNew(AnimationGraphView graphView) {
    this.guid = Guid.NewGuid().ToString();
    this.graphView = graphView;
    this.Initialize();
  }

  public TimeNode() {}
  public TimeNode(SerializableTimeNode serializable, AnimationGraphView graphView) {
    serializable.graphNode.Load(this);
    this.graphView = graphView;
    this.Initialize();
  }
}
}
