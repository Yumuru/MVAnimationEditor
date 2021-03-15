using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public class GraphView : UnityEditor.Experimental.GraphView.GraphView {
  public GraphAsset graphAsset { get; set; }

  public GraphView(GraphAsset asset) : base() {
    this.graphAsset = asset;
    SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
    style.flexGrow = 1;

    Insert(0, new GridBackground());

    this.AddManipulator(new ContentDragger());
    this.AddManipulator(new SelectionDragger());
    this.AddManipulator(new RectangleSelector());

    var searchWindowProvider = new SearchWindowProvider(this);

    this.RegisterCallback((KeyDownEvent evt) => {
      if (evt.ctrlKey && evt.keyCode == UnityEngine.KeyCode.S) {
        var path = AssetDatabase.GetAssetPath(graphAsset);
        if (string.IsNullOrEmpty(path)) {
          path = EditorUtility.SaveFilePanelInProject("Save", "AnimationGraph", "asset", "");
          if (!string.IsNullOrEmpty(path)) {
            AssetDatabase.CreateAsset(this.graphAsset, path);
          }
        }
        this.graphAsset.SaveAsset(this);
      }
    });

    nodeCreationRequest += context => {
      SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindowProvider);
    };

    asset.LoadAsset(this);
  }

  public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter) {
    var compatiblePorts = new List<Port>();
    foreach (var port in ports.ToList()) {
      var graphNode = (port.node as IGraphNode).graphNode;
      if (startAnchor.node == port.node ||
          startAnchor.direction == port.direction ||
          !graphNode.isCompatible(startAnchor, port)) {
        continue;
      }

      compatiblePorts.Add(port);
    }
    return compatiblePorts;
  }
}
}
