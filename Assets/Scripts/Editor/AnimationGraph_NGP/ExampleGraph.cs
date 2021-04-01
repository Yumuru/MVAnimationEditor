using System.Linq;
using GraphProcessor;
using UnityEditor.Callbacks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnimationGraph_NGP {
// CreateメニューからScriptableObjectのアセットを作れるように
[CreateAssetMenu(menuName = "Example Graph")]
public class ExampleGraph : BaseGraph {
#if UNITY_EDITOR

  protected override void OnEnable() {
    base.OnEnable();
    
    // ResultNodeが無かったらつくる
    if (!nodes.Any(x => x is ResultNode)) {
      AddNode(BaseNode.CreateFromType<ResultNode>(Vector2.zero));
    }
  }

    // ダブルクリックでウィンドウが開かれるように
  

  // ダブルクリックでウィンドウが開かれるように
  [OnOpenAsset(0)]
  public static bool OnBaseGraphOpened(int instanceID, int line) {
    var asset = EditorUtility.InstanceIDToObject(instanceID) as ExampleGraph;

    if (asset == null) return false;
    
    var window = EditorWindow.GetWindow<ExampleGraphWindow>();
    window.InitializeGraph(asset);
    return true;
  }
#endif
}
}
