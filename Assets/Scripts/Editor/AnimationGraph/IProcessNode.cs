using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using Yumuru;

namespace AnimationGraph {

public struct ProcessParameter {
  public AnimationConstructor.Constructor constructor;
  public float time;
  public AnimationClip clip;
}

public interface IProcessPort {
  Action<ProcessParameter> OnProceed { get; set; }
}
public class ProcessPort : Port, IProcessPort {
  public Action<ProcessParameter> OnProceed { get; set; }
  public ProcessPort(Orientation orientation, Direction direction, Capacity capacity, Type type) : base(orientation, direction, capacity, type) { }

  public static void Proceed(ProcessParameter parameter, Port port) {
    foreach (var edge in port.connections) {
      var processPort = edge.input as IProcessPort;
      processPort.OnProceed(parameter);
    }
  }
  public static ProcessPort CreateInput() { 
    return new ProcessPort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ProcessPort));
  }
  public static ProcessPort CreateInput(Action<ProcessParameter> onProceed) { 
    var port = CreateInput();
    port.OnProceed = onProceed;
    return port;
  }
  public static Port CreateOutput() { 
    return Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ProcessPort));
  }
}
}
