using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public static class CalculatePort {
  public static Port CreateInput<T>() {
    return Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(T));
  }
  public static Port CreateOutput<T>() {
    return Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(T));
  }
  public static object GetCalculatedValue(Port calculatePort) {
    if (!calculatePort.connected) return default;
    var portObject = calculatePort.connections.First().output.source as IPortObject;
    return portObject.getter();
  }
}
}
