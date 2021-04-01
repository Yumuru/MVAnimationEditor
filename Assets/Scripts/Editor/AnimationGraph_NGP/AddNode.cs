using System;
using GraphProcessor;

[Serializable]
[NodeMenuItem("Custom/Add")] // 作成時のメニュー名
public class AddNode : BaseNode {
  // 入力ポートを定義
  [Input(name = "A")]
  public float input1;
  // 2つ目の入力ポートを定義
  [Input(name = "B")]
  public float input2;

  // 出力ポートを定義
  [Output(name = "Out", allowMultiple = false)]
  public float output;

  public override string name => "Add";

  // 計算処理
  protected override void Process() {
    output = input1 + input2;
  }
}
