using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableCurveNode {
  public SerializableGraphNode graphNode;
  public string outputPortGuid;
  public AnimationCurve curve;
  public SerializableCurveNode() {
    this.outputPortGuid = Guid.NewGuid().ToString();
    this.curve = new AnimationCurve();
    curve.AddKey(0f, 0f);
    curve.AddKey(1f, 1f);
  }
  public SerializableCurveNode(CurveNode node) {
    this.graphNode = new SerializableGraphNode(node.graphNode);
    this.outputPortGuid = node.outputPortGuid;
    this.curve = node.curveField.value;
  }
}

public class CurveNode : Node, ICalculateNode {
  public IGraphNodeLogic graphNode { get; private set; }
  public CurveField curveField { get; private set; }
  public string outputPortGuid;
  public Dictionary<Port, Func<object>> Calculate { get; private set; } = new Dictionary<Port, Func<object>>();

  void SaveAsset(GraphAsset asset) {
    asset.curveNodes.Add(new SerializableCurveNode(this));
  }

  void Construct(SerializableCurveNode serializable) {
    this.title = "Curve";

    this.outputPortGuid = serializable.outputPortGuid;
    var outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(AnimationCurve));
    this.graphNode.RegisterPort(outputPort, outputPortGuid);
    this.outputContainer.Add(outputPort);

    this.curveField = new CurveField();
    curveField.value = serializable.curve;

    Calculate[outputPort] = () => this.curveField.value;

    this.mainContainer.Add(curveField);
  }
  
  public CurveNode() {
    this.graphNode = new GraphNodeLogic(this, SaveAsset);
    this.Construct(new SerializableCurveNode());
  }
  public CurveNode(GraphView graphView, SerializableCurveNode serializable) {
    this.graphNode = new GraphNodeLogic(this, graphView, SaveAsset);
    serializable.graphNode.Load(graphNode as GraphNodeLogic);
    this.Construct(serializable);
  }
}
}
