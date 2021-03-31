using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using Yumuru;

namespace AnimationGraph {
public delegate ProcessParameter Event(ProcessParameter parameter);
public delegate ProcessParameter Process(ProcessParameter parameter);
public struct ProcessParameter {
  public AnimationConstructor.Constructor constructor;
  public float time;
  public AnimationClip clip;
}

public static class EventPort {
  public static Port CreateInput() { 
    return Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Event));
  }
  public static Port CreateInput(Process proceed) { 
    var port = CreateInput();
    port.source = proceed;
    return port;
  }
  public static Port CreateOutput() { 
    return Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Event));
  }
  public static void NewEvent(this Port inputPort, Event proceed) { inputPort.source = proceed; }
  public static ProcessParameter Proceed(ProcessParameter parameter, Port port) {
    foreach (var edge in port.connections) {
      var proceed = edge.input.source as Event;
      if (proceed != null) parameter = proceed(parameter);
    }
    return parameter;
  }
}
}
