using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public delegate T Calculate<T>();
public interface ICalculatedOutPort<T> {
  Func<T> Calculate { get; set; }
}
public static class CalculatePort {
  public static Port CreateInput<T>() {
    return Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(T));
  }
  public static Port CreateOutput<T>() {
    return Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(T));
  }
  public static void SetCalculate<T>(this Port outputPort, Calculate<T> calculate) { outputPort.source = calculate; }
  public static T GetCalculatedValue<T>(Port calculatePort, Func<object, Calculate<T>> selector) {
    return selector(calculatePort.connections.First().output.source)();
  }
  public static T GetCalculatedValue<T>(Port calculatePort) {
    return GetCalculatedValue<T>(calculatePort, v => v as Calculate<T>);
  }
}
}
