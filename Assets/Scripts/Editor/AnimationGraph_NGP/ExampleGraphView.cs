using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEditor;

namespace AnimationGraph_NGP {
public class ExampleGraphView : BaseGraphView {
  public ExampleGraphView(EditorWindow window) : base(window) {
  }

  public override IEnumerable<(string, Type)> FilterCreateNodeMenuEntries() {
    foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries()) {
        // ResultNodeを追加できないように
      if (nodeMenuItem.type == typeof(ResultNode)) {
        continue;
      }
      yield return nodeMenuItem;
    }
  }

  protected override bool canDeleteSelection {
    // ResultNodeを消せないように
    get { return !selection.Any(e => e is ResultNode); }
  }
}
}
