using System;
using GraphProcessor;

[Serializable]
[NodeMenuItem("Custom/Result")]
public class ResultNode : BaseNode {
  [Input(name = "Result")]
  public float input;

  private float _result;
  public float Result => _result;

  public override string name => "Result";

  protected override void Process() {
    _result = input;
  }
}
