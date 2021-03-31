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
public interface IPortObject<T> {
  Func<T> getter { get; }
}
public class PortObject<T> : IPortObject<T> {
  public Func<T> getter { get; private set; }

  public PortObject(Func<T> getter) {
    this.getter = getter;
  }
}
}
