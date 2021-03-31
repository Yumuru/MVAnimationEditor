using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[Serializable]
public class SerializableCalculateValueField {
  [Serializable]
  public class Field {
    public string inputPortGuid;
    public float value;
    public Field() {
      inputPortGuid = Guid.NewGuid().ToString();
    }
    public Field(CalculateValueField.Field field) {
      this.inputPortGuid = field.inputPortGuid;
      this.value = field.valueField.value;
    }
  }
  public List<Field> fields = new List<Field>();

  public SerializableCalculateValueField() { }
  public SerializableCalculateValueField(CalculateValueField calculateField) {
    foreach (var field in calculateField.fields) {
      this.fields.Add(new Field(field));
    }
  }
}
public class CalculateValueField : VisualElement {
  public class Field : VisualElement {
    public Port inputPort { get; private set; }
    public string inputPortGuid { get; private set; }
    public FloatField valueField { get; private set; }
    public Func<object> Calculate;
    public Field(Port inputPort, SerializableCalculateValueField.Field field) {
      this.inputPort = inputPort;
      valueField = new FloatField();
      valueField.label = " ";
      valueField.labelElement.style.minWidth = 0f;
      this.style.flexDirection = FlexDirection.Row;
      this.Add(inputPort);
      this.Add(valueField);

      Calculate = () => {
        if (inputPort.connected) {
          return CalculatePort.GetCalculatedValue(inputPort);
        } else {
          return valueField.value;
        }
      };

      this.inputPortGuid = field.inputPortGuid;
      this.valueField.value = field.value;
    }
  }
  public List<Field> fields { get; private set; } = new List<Field>();
  public bool isContainVectorOrMatrixInput { get { return fields.Where(f => isVectorOrMatrix(f.inputPort.portType)).Any(); } }
  
  public bool isVectorOrMatrix(Type type) {
    if (type == typeof(Vector2)) return true;
    if (type == typeof(Vector3)) return true;
    if (type == typeof(Vector4)) return true;
    if (type == typeof(Matrix4x4)) return true;
    return false;
  }

  public CalculateValueField(IGraphNode node, SerializableCalculateValueField serializable) {
    foreach (var sfield in serializable.fields) {
      var inputPort = (node as Node).InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
      node.graphNode.RegisterPort(inputPort, sfield.inputPortGuid);
      var field = new Field(inputPort, sfield);
      this.fields.Add(field);
      this.Add(field);
    }
  }
}
}
