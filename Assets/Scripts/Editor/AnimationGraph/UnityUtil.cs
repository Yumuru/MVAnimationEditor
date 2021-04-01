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

  public static float CastFloat(this object value) {
    if (value is float) {
      return (float)value;
    } if (value is int) {
      return (float)(int)value;
    }
    return default;
  }
  public static int CastInt(this object value) {
    if (value is float) {
      return (int)(float)value;
    } if (value is int) {
      return (int)value;
    }
    return default;
  }
}
}
