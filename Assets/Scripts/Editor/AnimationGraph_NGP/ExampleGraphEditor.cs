using GraphProcessor;
using UnityEngine;
using UnityEditor;

namespace AnimationGraph_NGP {
[CustomEditor(typeof(ExampleGraph))]
public class ExampleGraphEditor : Editor {
  public override void OnInspectorGUI() {
    base.OnInspectorGUI();
    if (GUILayout.Button("Process")) {
      var graph = target as ExampleGraph;
      var processor = new ExampleGraphProcessor(graph);
      processor.Run();
      Debug.Log(processor.Result);
    }
  }
}
}
