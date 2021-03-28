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

public interface IProcessNode : IGraphNode {
  Action<ProcessParameter> Proceed { get; }
}
public static class ProcessNode {
  public static void Proceed(ProcessParameter parameter, Port port) {
    foreach (var edge in port.connections) {
      var node = edge.input.node as IProcessNode;
      node.Proceed?.Invoke(parameter);
    }
  }
}
}
