using System.IO;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using UnityEngine.Assertions;

namespace AnimationGraph_NGP {
public class ExampleGraphWindow : BaseGraphWindow {
  protected override void InitializeWindow(BaseGraph graph) {
    Assert.IsNotNull(graph);
    
    var fileName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(graph));
    titleContent = new GUIContent(ObjectNames.NicifyVariableName(fileName));

    if (graphView == null) {
      // ExampleGraphViewに変更する
      graphView = new ExampleGraphView(this);
    }
    rootView.Add(graphView);
  }
}
}
