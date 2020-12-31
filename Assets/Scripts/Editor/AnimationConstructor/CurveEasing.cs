using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
namespace Yumuru {
public static class CurveEasing {
  public static AnimationCurve Make(float outTangent, float inTangent) {
    return new AnimationCurve() {
      keys = new Keyframe[] {
        new Keyframe(0f, 0f, 0f, outTangent),
        new Keyframe(1f, 1f, inTangent, 0f) } };
  }
  public static AnimationCurve Merge(AnimationCurve curve1, AnimationCurve curve2, float separate) {
    var keys = new List<Keyframe>();
    foreach (var key in curve1.keys) {
      keys.Add(new Keyframe(key.time * separate, key.value * separate, key.inTangent, key.outTangent));
    }
    var curve2keys = curve2.keys;
    var lastKey = keys[keys.Count-1];
    lastKey.outTangent = curve2keys[0].outTangent;
    keys[keys.Count-1] = lastKey;
    foreach (var key in curve2keys.Skip(1)) {
      keys.Add(new Keyframe(lastKey.time + key.time * (1-separate), lastKey.value + key.value * (1-separate), key.inTangent, key.outTangent));
    }
    return new AnimationCurve() { keys = keys.ToArray() };
  }
  public static AnimationCurve Linear = Make(1f, 1f);
  public static AnimationCurve InSine = Make(0.0001509906f, 1.1980813f);
  public static AnimationCurve OutSine = Make(1.416193f, 0.050944973f);
  public static AnimationCurve InOutSine = Merge(InSine, OutSine, 0.5f);
  public static AnimationCurve InQuad = Make(0.02946811f, 1.842232f);
  public static AnimationCurve OutQuad = Make(1.686899f, 0.049764033f);
  public static AnimationCurve InOutQuad = Merge(InQuad, OutQuad, 0.5f);
  public static AnimationCurve InCubic = Make(0.02277081f, 3.1423726f);
  public static AnimationCurve OutCubic = Make(2.9087877f, 0.011833555f);
  public static AnimationCurve InOutCubic = Merge(InCubic, OutCubic, 0.5f);
  public static AnimationCurve InQuart = Make(0.056719585f, 3.4534116f);
  public static AnimationCurve OutQuart = Make(3.3953815f, 0.056712147f);
  public static AnimationCurve InOutQuart = Merge(InQuart, OutQuart, 0.5f);
}
}
#endif
