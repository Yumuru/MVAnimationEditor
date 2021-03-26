using System;
using UnityEngine;

namespace AnimationGraph {

public struct ProcessParameter {
  public float time;
  public AnimationClip clip;
}

public interface IProcessNode : IGraphNode {
  Action<ProcessParameter> Proceed { get; }
}
}
