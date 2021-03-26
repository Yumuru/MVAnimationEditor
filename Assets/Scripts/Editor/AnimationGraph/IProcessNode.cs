using System;
using UnityEngine;
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
}
