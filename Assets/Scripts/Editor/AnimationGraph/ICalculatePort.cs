using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
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
  public static Port CreateCalculateInPort<T>() {
    return Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(T));
  }
  public static T GetCalculatedValue<T>(Port calculatePort) {
    var port = calculatePort.connections.First().output as ICalculatedOutPort<T>;
    return port.Calculate();
  }
}
}
