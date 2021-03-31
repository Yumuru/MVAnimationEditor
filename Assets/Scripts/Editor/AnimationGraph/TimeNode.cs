using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableTimeNode {
  public SerializableGraphNode graphNode;
  public string timePortGuid;
  public List<string> instanceActionPortGuids = new List<string>();
  public string outputPortGuid;
  public SerializableTimeNode() {
    this.timePortGuid = Guid.NewGuid().ToString();
    this.outputPortGuid = Guid.NewGuid().ToString();
  }
  public SerializableTimeNode(TimeNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.instanceActionPortGuids = node.instanceActionPortGuids.ToList();
    this.timePortGuid = node.timePortGuid;
    this.outputPortGuid = node.outputPortGuid;
  }
}
public class TimeNode : Node, IGraphNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public string timePortGuid;
  public string outputPortGuid;

  public List<string> instanceActionPortGuids = new List<string>();

  public void SaveAsset(GraphAsset graphAsset) {
    graphAsset.timeNodes.Add(new SerializableTimeNode(this));
  }

  void Construct(SerializableTimeNode serializable) {
    this.title = "Time";

    var timePort = CalculatePort.CreateInput<float>();
    timePortGuid = serializable.timePortGuid;
    graphNode.RegisterPort(timePort, timePortGuid);
    this.inputContainer.Add(timePort);

    foreach (var instanceActionPortGuid in serializable.instanceActionPortGuids) {
      var instanceActionPort = CalculatePort.CreateInput<Proceed>();
      graphNode.RegisterPort(instanceActionPort, instanceActionPortGuid);
      instanceActionPortGuids.Add(instanceActionPortGuid);
      this.inputContainer.Add(instanceActionPort);
    }
    var button = new Button(() => {
      var instanceActionPort = CalculatePort.CreateInput<Proceed>();
      var instanceActionPortGuid = Guid.NewGuid().ToString();
      graphNode.RegisterPort(instanceActionPort, instanceActionPortGuid);
      instanceActionPortGuids.Add(instanceActionPortGuid);
      this.inputContainer.Add(instanceActionPort);
    });
    button.text = "Add";
    this.mainContainer.Add(button);

    var outputPort = CalculatePort.CreateOutput<Proceed>();
    this.outputPortGuid = serializable.outputPortGuid;
    graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    outputPort.source = new SequenceActionParameter(true, () => {
      return p => {
        p.time += CalculatePort.GetCalculatedValue<float>(timePort);
        p.constructor.TSet(p.time);
        foreach (var instanceActionPort in this.inputContainer.Children().Skip(1).Cast<Port>()) {
          var action = CalculatePort.GetCalculatedValue<Proceed>(instanceActionPort);
          p = action(p);
        }
        return p;
      };
    });
  }

  public TimeNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializableTimeNode());
  }
  public TimeNode(AnimationGraphView graphView, SerializableTimeNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(this.graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
