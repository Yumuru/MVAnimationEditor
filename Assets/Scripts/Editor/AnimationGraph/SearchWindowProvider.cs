using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public class SearchWindowProvider : ScriptableObject, ISearchWindowProvider {
  private AnimationGraphWindow window;
  private GraphView graphView;

  public SearchWindowProvider(AnimationGraphWindow window, GraphView graphView) {
    this.window = window;
    this.graphView = graphView;
  }

  List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context) {
    var entries = new List<SearchTreeEntry>();
    entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));

    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
      foreach (var type in assembly.GetTypes()) {
        var checkSubclass =
          type.IsSubclassOf(typeof(Node)) ||
          type.IsSubclassOf(typeof(GraphElement));
        if (type.IsClass && type.Namespace == "AnimationGraph" && !type.IsAbstract && checkSubclass) {
          entries.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 1, userData = type });
        }
      }
    }

    return entries;
  }

  bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
    var type = searchTreeEntry.userData as System.Type;
    var element = Activator.CreateInstance(type) as GraphElement;
    if (element is IGraphNode node) {
      node.graphNode.Initialize(graphView);
    }
    var worldMousePosition = window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent, context.screenMousePosition - window.position.position);
    var localMousePosition = graphView.contentViewContainer.WorldToLocal(worldMousePosition);
    element.SetPosition(new Rect(localMousePosition, default));
    graphView.AddElement(element);
    return true;
  }
}
}
