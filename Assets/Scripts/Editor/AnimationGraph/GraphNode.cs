using System;
using System.Linq;
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
public interface ICalculateNode : IGraphNode {
  Dictionary<Port, Func<object>> Calculate { get; }
}
public interface ICalculatedOutPort<T> {
  Func<T> Calculate { get; set; }
}
public class BasicCalculatedOutPort<T> : Port, ICalculatedOutPort<T> {
  public Func<T> Calculate { get; set; }
  public BasicCalculatedOutPort() : base(Orientation.Horizontal, Direction.Output, Capacity.Multi, typeof(T)) { }
  public BasicCalculatedOutPort(Func<T> calculate) : this() {
    this.Calculate = calculate;
  }
}
public static class CalculatePort {
  public static T GetCalculatedValue<T>(Port caluculatePort) {
    var port = caluculatePort.connections.First().output;
    var node = port.node as ICalculateNode;
    return (T) node.Calculate[port]();
  }
  public static T GetCalculatedValue_New<T>(Port calculatePort) {
    var port = calculatePort.connections.First().output as ICalculatedOutPort<T>;
    return port.Calculate();
  }
}
public interface IGraphNodeLogic {
  Node node { get; }
  Dictionary<Port, string> portGuids { get; }
  Dictionary<string, Port> guidPorts { get; }
  Func<Port, Port, bool> isCompatible { get; set; }
  Action<GraphAsset> SaveAsset { get; }
  void Initialize(GraphView graphView);
  void RegisterPort(Port port, string guid);
  void UnregisterPort(Port port);
}
public class GraphNodeLogic : IGraphNodeLogic {
  public Node node { get; private set; }
  public GraphView graphView { get; private set; }
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
  public void Initialize(GraphView graphView) {
    this.graphView = graphView;
  }
  public GraphNodeLogic(Node node, Action<GraphAsset> saveAsset) {
    this.Construct(node);
    this.SaveAsset = saveAsset;
  }
  public GraphNodeLogic(Node node, GraphView animationGraphView, Action<GraphAsset> saveAsset) {
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
