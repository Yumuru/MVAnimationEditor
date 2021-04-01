using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;

[System.Serializable, NodeMenuItem("Primitives/Float")]
public class FloatNode : BaseNode {
  [Output("Out")]
  public float output;
  
  public override string name => "Float";
}

[NodeCustomEditor(typeof(FloatNode))]
public class FloatNodeView : BaseNodeView {
  public override void Enable() {
    var node = nodeTarget as FloatNode;
    this.titleContainer.RemoveFromHierarchy();
    var field = new DoubleField();
    field.RegisterValueChangedCallback(v => {
      owner.RegisterCompleteObjectUndo("Updated floatNode input");
      node.output = (float)v.newValue;
    });
    controlsContainer.Add(field);
  }
}
