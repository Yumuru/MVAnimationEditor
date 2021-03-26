using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using static Yumuru.AnimationConstructor;

namespace Yumuru {
public class AnimationConstructor {
  public string rootPath;
  public AnimationClip clip;
  public float frameTime;

  public AnimationConstructor(Transform root, AnimationClip clip) {
    this.rootPath = GetHeirarchyPath(root);
    this.clip = clip;
    frameTime = 1f / clip.frameRate;
  }

  public interface IPropertyInfo<T> {
    void Act(Action<FloatPropertyInfo> action);
    void Act(T value, Action<FloatPropertyInfo, float> action);
  }
  public class FloatPropertyInfo : IPropertyInfo<float>, IEquatable<FloatPropertyInfo> {
    public string targetPath;
    public Type type;
    public string propertyName;
    public FloatPropertyInfo(string targetPath, Type type, string propertyName) {
      this.targetPath = targetPath;
      this.type = type;
      this.propertyName = propertyName;
    }
    public FloatPropertyInfo(Transform target, Type type, string propertyName) :
      this(GetHeirarchyPath(target), type, propertyName) {}
    public void Act(Action<FloatPropertyInfo> action) { action(this); }
    public void Act(float value, Action<FloatPropertyInfo, float> action) { action(this, value); }
    public override string ToString() {
      return "Property (TargetPath: " + targetPath + ", type: " + type + ", propertyName: " + propertyName + ")";
    }
    public bool Equals(FloatPropertyInfo info) {
      return this.targetPath == info.targetPath && this.propertyName == info.propertyName;
    }
    public override bool Equals(object obj) {
      return this.Equals((FloatPropertyInfo) obj);
    }
    public override int GetHashCode() {
      return targetPath.GetHashCode() ^ propertyName.GetHashCode();
    }
  } 
  public class Vector3PropertyInfo : IPropertyInfo<Vector3> {
    public FloatPropertyInfo x, y, z;
    public Vector3PropertyInfo(string targetPath, Type type, string propertyName) {
      x = new FloatPropertyInfo(targetPath, type, propertyName + ".x");
      y = new FloatPropertyInfo(targetPath, type, propertyName + ".y");
      z = new FloatPropertyInfo(targetPath, type, propertyName + ".z");
    }
    public Vector3PropertyInfo(Transform target, Type type, string propertyName) :
      this(GetHeirarchyPath(target), type, propertyName) {}
    public void Act(Action<FloatPropertyInfo> action) { 
      action(x); action(y); action(z); }
    public void Act(Vector3 value, Action<FloatPropertyInfo, float> action) { 
      action(x, value.x); action(y, value.y); action(z, value.z); }
  }
  public class ColorPropertyInfo : IPropertyInfo<Color> {
    public FloatPropertyInfo r, g, b, a;
    public ColorPropertyInfo(string targetPath, Type type, string propertyName) {
      r = new FloatPropertyInfo(targetPath, type, propertyName + ".r");
      g = new FloatPropertyInfo(targetPath, type, propertyName + ".g");
      b = new FloatPropertyInfo(targetPath, type, propertyName + ".b");
      a = new FloatPropertyInfo(targetPath, type, propertyName + ".a");
    }
    public ColorPropertyInfo(Transform target, Type type, string propertyName) :
      this(GetHeirarchyPath(target), type, propertyName) {}
    public void Act(Action<FloatPropertyInfo> action) {
      action(r); action(g); action(b); action(a); }
    public void Act(Color value, Action<FloatPropertyInfo, float> action) { 
      action(r, value.r); action(g, value.g); action(b, value.b); action(a, value.a); }
  }
  public class BoolPropertyInfo : IPropertyInfo<bool> {
    public FloatPropertyInfo p;
    public BoolPropertyInfo(string targetPath, Type type, string propertyName) {
      p = new FloatPropertyInfo(targetPath, type, propertyName);
    }
    public BoolPropertyInfo(Transform target, Type type, string propertyName) :
      this(GetHeirarchyPath(target), type, propertyName) {}
    public void Act(Action<FloatPropertyInfo> action) { action(p); }
    public void Act(bool value, Action<FloatPropertyInfo, float> action) {
      action(p, value ? 1 : 0); }
  }

  public EditorCurveBinding MakeBinding(FloatPropertyInfo propertyInfo) {
    if (!propertyInfo.targetPath.Contains(rootPath)) {
      throw new Exception();
    }
    var path = propertyInfo.targetPath.Remove(0, rootPath.Length);
    if (path.Length > 0 && path[0] == '/') path = path.Remove(0, 1);
    var binding = new EditorCurveBinding();
    binding.path = path;
    binding.type = propertyInfo.type;
    binding.propertyName = propertyInfo.propertyName;
    return binding;
  }

  public class CTime {
    public float baseTime { get; private set; }
    public float time { get; private set; }
    public float currentTime { get { return baseTime + time; } }

    public CTime(float baseTime) {
      this.baseTime = baseTime;
      this.time = 0f;
    }
    public void Next(float time) { this.time += time; }
    public void TSet(float time) { this.time = time; }
    public void TimeZone(float offset, Action<CTime> zone) { zone(new CTime(currentTime + offset)); }
    public void TimeZone(Action<CTime> zone) { TimeZone(0f, zone); }
  }

  public interface IChangeValue {
    float value { get; set; }
    float time { get; set; }
  }
  public class ImmediateChangeValue : IChangeValue {
    public float value { get; set; }
    public float time { get; set; }
    public ImmediateChangeValue(float value, float time) {
      this.value = value;
      this.time = time;
    }
  }
  public class CurveChangeValue : IChangeValue {
    public float value { get; set; }
    public float time { get; set; }
    public AnimationCurve curve;
    public CurveChangeValue(float destinationValue, float destinationTime, AnimationCurve curve) {
      this.value = destinationValue;
      this.time = destinationTime;
      this.curve = curve;
    }
  }

  public class Constructor {
    public Stack<CTime> times { get; private set; }
    public Stack<List<Action<float>>> assignActions;
    public Dictionary<FloatPropertyInfo, List<IChangeValue>> values;

    public Constructor() {
      this.times = new Stack<CTime>();
      this.times.Push(new CTime(0f));
      this.assignActions = new Stack<List<Action<float>>>();
      this.assignActions.Push(new List<Action<float>>());
      this.values = new Dictionary<FloatPropertyInfo, List<IChangeValue>>();
    }

    public void Next(float forwardTime) {
      var time = times.Peek();
      time.Next(forwardTime);
      var actions = assignActions.Peek();
      foreach (var a in actions) { a(time.currentTime); }
      actions.Clear();
    }

    public void TSet(float setTime) {
      var time = times.Peek();
      time.TSet(setTime);
      var actions = assignActions.Peek();
      foreach (var a in actions) { a(time.currentTime); }
      actions.Clear();
    }

    public bool isValueExist(FloatPropertyInfo p) { return values.ContainsKey(p) && values[p].Count > 0; }

    public void AddChangeValue<T>(IPropertyInfo<T> property,
        Func<FloatPropertyInfo, bool, IChangeValue> changeValue) {
      property.Act(p => {
        var cv = changeValue(p, isValueExist(p));
        if (cv != null) { values[p].Add(cv); }
      });
    }
    public void AddChangeValue<T>(IPropertyInfo<T> property, T value,
        Func<FloatPropertyInfo, float, bool, IChangeValue> changeValue) {
      property.Act(value, (p, v) => {
        var cv = changeValue(p, v, isValueExist(p));
        if (cv != null) { values[p].Add(cv); }
      });
    }
    public void AddChangeValue<T>(IPropertyInfo<T> property, T value, Func<FloatPropertyInfo, float, IChangeValue> changeValue) {
      AddChangeValue(property, value, (p, v, b) => {
        if (!b) { values[p] = new List<IChangeValue>(); }
        return changeValue(p, v);
      });
    }
    public void AddChangeValue<T>(IPropertyInfo<T> property, T value, Func<float, IChangeValue> changeValue) {
      AddChangeValue(property, value, (p, v) => changeValue(v)); }


    public void Add<T>(IPropertyInfo<T> property, T value, AnimationCurve curve) {
      var actions = assignActions.Peek();
      actions.Add(t => { AddChangeValue(property, value, v => new CurveChangeValue(v, t, curve)); });
    }

    public void AddImmediate<T>(IPropertyInfo<T> property, T value) {
      var time = times.Peek().currentTime;
      AddChangeValue(property, value, v => new ImmediateChangeValue(v, time));
    }

    public void AddWithTimeOffset<T>(IPropertyInfo<T> property, float timeOffset, T value, AnimationCurve curve) {
      var time = times.Peek().currentTime;
      AddChangeValue(property, value, v => new CurveChangeValue(v, time + timeOffset, curve));
    }

    public void AddByRate<T>(IPropertyInfo<T> property, float rate, T value, AnimationCurve curve) {
      var actions = assignActions.Peek();
      actions.Add(t => {
        AddChangeValue(property, value, (p, v, b) => {
          if (!b) return new ImmediateChangeValue(v, t);
          var vals = values[p];
          var time = Mathf.Lerp(vals[vals.Count-1].time, t, rate);
          return new CurveChangeValue(v, time, curve);
        });
      });
    }
    
    public void Unchange<T>(IPropertyInfo<T> property) {
      var actions = assignActions.Peek();
      var time = times.Peek().currentTime;
      AddChangeValue(property, (p, b) => {
        if (!b) { throw new Exception("The value of the " + p + "is not registerd!"); }
        var vals = values[p];
        return new ImmediateChangeValue(vals[vals.Count-1].value, time);
      });
    }
    public void Unchange<T>(params IPropertyInfo<T>[] property) {
      foreach(var p in property) Unchange(p); }

    public void TimeZone(float offset, Action zone) {
      times.Peek().TimeZone(offset, time => {
        times.Push(time);
        assignActions.Push(new List<Action<float>>());
        zone();
        assignActions.Pop();
        times.Pop();
      });
    }
    public void TimeZone(Action zone) { TimeZone(0f, zone); }
  }

  public IEnumerable<(Keyframe keyframe, bool isConstant)> ConstructKeyframes(List<IChangeValue> changeValues) {
    var list = new List<IChangeValue>();
    list.Add(changeValues[0]);
    foreach (var cv in changeValues.Skip(1)) {
      var last = list[list.Count-1];
      if (last.time + frameTime > cv.time) {
        last.time = cv.time - frameTime;
      }
      list.Add(cv);
    }
    var keyframes = new List<(Keyframe keyframe, bool isConstant)>();
    keyframes.Add((new Keyframe(list[0].time, list[0].value), true));
    for (int i = 1; i < list.Count; i++) {
      var cv = list[i];
      switch(cv) {
        case ImmediateChangeValue icv:
          keyframes.Add((new Keyframe(icv.time, icv.value), true));
          break;
        case CurveChangeValue ccv:
          var lastChangedValue = list[i-1];
          var deltaTime = ccv.time - lastChangedValue.time;
          var deltaValue = ccv.value - lastChangedValue.value;
          Keyframe lastCurveKey = default;
          foreach (var key in ccv.curve.keys) {
            if (key.time < frameTime) { lastCurveKey = key; continue; }
            var time = lastChangedValue.time + deltaTime * key.time;
            var value = lastChangedValue.value + deltaValue * key.value;
            var last = keyframes[keyframes.Count-1];
            var nextKeyframe = new Keyframe(time, value);
            var dTime = time - last.keyframe.time;
            var dValue = value - last.keyframe.value;
            last.keyframe.outTangent = lastCurveKey.outTangent * deltaValue / deltaTime;
            nextKeyframe.inTangent = key.inTangent * deltaValue / deltaTime;
            keyframes[keyframes.Count-1] = last;
            keyframes.Add((nextKeyframe, false));
            lastCurveKey = key;
          }
          break;
      }
    }
    return keyframes;
  }

  public void Construct(Constructor constructor) {
    foreach (var vals in constructor.values) {
      var binding = MakeBinding(vals.Key);
      var curve = AnimationUtility.GetEditorCurve(clip, binding);
      if (curve == null) curve = new AnimationCurve();

      // Construct
      var keyframes = ConstructKeyframes(vals.Value);
      var (sTime, eTime) = (keyframes.Min(k => k.keyframe.time), keyframes.Max(k => k.keyframe.time));

      // RemoveExistKeys
      var removed = 0;
      foreach (var (key, index) in curve.keys.Select((k, i) => (k, i))) {
        if (sTime <= key.time && key.time <= eTime) {
          curve.RemoveKey(index - removed++); } }

      // AddKeys
      foreach (var keyframe in keyframes) {
        var index = curve.AddKey(keyframe.keyframe);
        if (keyframe.isConstant) {
          if (index > 0)
            AnimationUtility.SetKeyRightTangentMode(curve, index-1, AnimationUtility.TangentMode.Constant); }
      }
      AnimationUtility.SetEditorCurve(clip, binding, curve);
    }
  }

  public void Construct(Action<Constructor> constructAction) {
    var constructor = new Constructor();
    constructAction(constructor);
    Construct(constructor);
  }

  public static StringBuilder builder = new StringBuilder();
  public static string GetHeirarchyPath(Transform transform) {
    builder.Clear();
    builder.Insert(0, transform.gameObject.name);
    transform = transform.parent;
    while (transform != null) {
      builder.Insert(0, transform.name + "/");
      transform = transform.parent;
    }
    return builder.ToString();
  }
}
}

#endif
