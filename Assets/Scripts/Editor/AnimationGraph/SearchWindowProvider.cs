using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public class SearchWindowProvider : ScriptableObject, ISearchWindowProvider {
  private AnimationGraphView graphView;

  public void Initialize(AnimationGraphView graphView) {
    this.graphView = graphView;
  }

  List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context) {
    var entries = new List<SearchTreeEntry>();
    entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));

    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
      foreach (var type in assembly.GetTypes()) {
        if (type.IsClass && type.Namespace == "AnimationGraph" && !type.IsAbstract && (type.IsSubclassOf(typeof(Node)))) {
          entries.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 1, userData = type });
        }
      }
    }

    return entries;
  }

  bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
    var type = searchTreeEntry.userData as System.Type;
    var node = Activator.CreateInstance(type) as Node;
    if (node is IAnimationGraphNode graphNode) {
      graphNode.InitializeNew();
    }
    graphView.AddElement(node);
    EditorUtility.SetDirty(graphView.graphAsset);
    return true;
  }
}
}
