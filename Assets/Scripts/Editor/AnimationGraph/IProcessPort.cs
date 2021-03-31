using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using Yumuru;

namespace AnimationGraph {
public delegate ProcessParameter Proceed(ProcessParameter process);
public struct ProcessParameter {
  public AnimationConstructor.Constructor constructor;
  public float time;
  public AnimationClip clip;
}

public static class ProcessPort {
  public static Port CreateInput() { 
    return Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Proceed));
  }
  public static Port CreateInput(Proceed proceed) { 
    var port = CreateInput();
    port.source = proceed;
    return port;
  }
  public static Port CreateOutput() { 
    return Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Proceed));
  }
  public static void SetProceed(this Port inputPort, Proceed proceed) { inputPort.source = proceed; }
  public static ProcessParameter Proceed(ProcessParameter parameter, Port port) {
    foreach (var edge in port.connections) {
      var proceed = edge.input.source as Proceed;
      if (proceed != null) parameter = proceed(parameter);
    }
    return parameter;
  }
}
}
