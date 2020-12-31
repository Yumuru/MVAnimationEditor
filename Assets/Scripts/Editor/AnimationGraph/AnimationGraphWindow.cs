using UnityEditor;
using UnityEngine.UIElements;

namespace AnimationGraph {
public class AnimationGraphWindow : EditorWindow {
  AnimationGraphAsset graphAsset;

  [MenuItem("Sandbox/Open AnimationGraph")]
  public static void Open() {
    var window = GetWindow<AnimationGraphWindow>("AnimationGraph");
    window.graphAsset = new AnimationGraphAsset();
  }

  void OnEnable() {
    var graphView = new AnimationGraphView(graphAsset);
    rootVisualElement.Add(graphView);
  }
}
}
