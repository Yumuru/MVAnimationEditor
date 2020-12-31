using System;
using System.Linq;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public interface IAnimationGraphNode {
  string guid { get; set; }
  void InitializeNew(AnimationGraphView graphView);
}
public interface IProcessNode : IAnimationGraphNode {
  ProcessPort processPort { get; set; }
}
public class ProcessPort {
  public Port InputPort;
  public Port OutputPort;
  public Port CreateInput() { 
    InputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ProcessPort));
    InputPort.portName = "In";
    return InputPort;
  }
  public Port CreateOutput() { 
    OutputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(ProcessPort));
    OutputPort.portName = "Out";
    return OutputPort;
  }
}
}

