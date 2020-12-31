using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
public class AnimationGraphView : GraphView {
  public AnimationGraphAsset graphAsset { get; set; }

  public AnimationGraphView(AnimationGraphAsset asset) : base() {
    this.graphAsset = asset;
    SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
    style.flexGrow = 1;

    Insert(0, new GridBackground());

    this.AddManipulator(new ContentDragger());
    this.AddManipulator(new SelectionDragger());
    this.AddManipulator(new RectangleSelector());

    var searchWindowProvider = new SearchWindowProvider();
    searchWindowProvider.Initialize(this);

    nodeCreationRequest += context => {
      SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindowProvider);
    };
  }

  public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter) {
    var compatiblePorts = new List<Port>();
    foreach (var port in ports.ToList()) {
      if (startAnchor.node == port.node ||
          startAnchor.direction == port.direction || 
          startAnchor.portType != port.portType) {
        continue;
      }

      compatiblePorts.Add(port);
    }
    return compatiblePorts;
  }
}
}
