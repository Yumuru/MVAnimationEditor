using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public interface IEdgeConnectCallback {
  Action<Edge> OnConnected { get; }
}
public class EdgeConnectCallback : IEdgeConnectCallback {
  public Action<Edge> OnConnected { get; }
  public EdgeConnectCallback(Action<Edge> OnConnected) {
    this.OnConnected = OnConnected;
  }
}
public interface IPortObject {
  Func<object> getter { get; }
}
public class PortObject<T> : IPortObject {
  public Func<object> getter { get; private set; }

  public PortObject(Func<T> getter) {
    this.getter = () => getter();
  }
}
}
