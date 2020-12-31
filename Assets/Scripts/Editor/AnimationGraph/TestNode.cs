using System;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public class TestNode : Node {
  public Port InputPort { get; set; }
  public Port OutputPort { get; set; }

  public TestNode() {
    InputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Port));
    InputPort.portName = "In";
    inputContainer.Add(InputPort);

    OutputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Port));
    OutputPort.portName = "Out";
    outputContainer.Add(OutputPort);
  }
}
}
