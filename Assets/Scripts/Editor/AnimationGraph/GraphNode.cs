using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableGraphNode {
  public Rect position;
  public SerializableGraphNode(IGraphNodeLogic node) {
    this.position = node.node.GetPosition();
  }
  public void Load(GraphNodeLogic node) {
    node.node.SetPosition(position);
  }
}
public interface IGraphNode {
  IGraphNodeLogic graphNode { get; }
}
public interface IGraphNodeLogic {
  Node node { get; }
  Dictionary<Port, string> portGuids { get; }
  Dictionary<string, Port> guidPorts { get; }
  Func<Port, Port, bool> isCompatible { get; set; }
  Action<GraphAsset> SaveAsset { get; }
  void Initialize(AnimationGraphView graphView);
  void RegisterPort(Port port, string guid);
  void UnregisterPort(Port port);
}
public class GraphNodeLogic : IGraphNodeLogic {
  public Node node { get; private set; }
  public AnimationGraphView graphView { get; private set; }
  public Dictionary<Port, string> portGuids { get; } = new Dictionary<Port, string>();
  public Dictionary<string, Port> guidPorts { get; }= new Dictionary<string, Port>();
  public Action<GraphAsset> SaveAsset { get; private set; }
  public Func<Port, Port, bool> isCompatible { get; set; }
    = (input, output) => input.portType == output.portType;
  void Construct(Node node) {
    this.node = node;
    this.node.RegisterCallback((DetachFromPanelEvent evt) => {
      foreach (var p in portGuids) {
        foreach (var e in p.Key.connections) { e.RemoveFromHierarchy(); }
      }
    });
  }
  public void Initialize(AnimationGraphView graphView) {
    this.graphView = graphView;
  }
  public GraphNodeLogic(Node node, Action<GraphAsset> saveAsset) {
    this.Construct(node);
    this.SaveAsset = saveAsset;
  }
  public GraphNodeLogic(Node node, AnimationGraphView animationGraphView, Action<GraphAsset> saveAsset) {
    this.Construct(node);
    this.graphView = animationGraphView;
    this.SaveAsset = saveAsset;
  }
  public void RegisterPort(Port port, string guid) {
    portGuids[port] = guid;
    guidPorts[guid] = port;
  }
  public void UnregisterPort(Port port) {
    port.RemoveFromHierarchy();
    foreach (var edge in port.connections) {
      edge.RemoveFromHierarchy(); }
    guidPorts.Remove(portGuids[port]);
    portGuids.Remove(port);
  }
}
}
