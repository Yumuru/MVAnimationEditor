using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AnimationGraph {
public static class UnityUtil {
  static StringBuilder builder = new StringBuilder();
  public static string GetHeirarchyPath(Transform transform) {
    builder.Clear();
    builder.Insert(0, transform.gameObject.name);
    transform = transform.parent;
    while(transform != null) {
      builder.Insert(0, transform.gameObject.name + "/");
      transform = transform.parent;
    }
    return builder.ToString();
  }
}
}
