using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;

namespace AnimationGraph {
public class AnimationGraphWindow : EditorWindow {
  GraphAsset graphAsset;

  [MenuItem("Sandbox/Open AnimationGraph")]
  public static void Open() {
    GraphAsset graphAsset;
    if (Selection.activeObject is GraphAsset gA) {
      graphAsset = gA;
    } else {
      graphAsset = new GraphAsset();
    }
    var graphWindow = (AnimationGraphWindow) null;
    if (graphWindow == null) {
      graphWindow = CreateInstance<AnimationGraphWindow>();
    }
    graphWindow.Show();
    graphWindow.graphAsset = graphAsset;
    graphWindow.Initialize();
  }

  [OnOpenAsset()]
  static bool OnOpenAsset(int instanceId, int line){
    if (EditorUtility.InstanceIDToObject(instanceId) is GraphAsset) {
      Open();
      return true;
    }
    return false;
  }

  public void OnEnable() {
    if (this.graphAsset != null) {
      Initialize();
    }
  }

  public void Initialize() {
    var graphView = new AnimationGraphView(this, this.graphAsset);
    rootVisualElement.Add(graphView);
  }
}
}
