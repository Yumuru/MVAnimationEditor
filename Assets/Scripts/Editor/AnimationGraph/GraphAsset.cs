﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace AnimationGraph {
[CreateAssetMenu(menuName = "AnimationGraph")]
public class GraphAsset : ScriptableObject {
  public List<SerializableAnimationGenerateNode> animationGenerateNodes = new List<SerializableAnimationGenerateNode>();
  public List<SerializablePropertyNode> propertyNodes = new List<SerializablePropertyNode>();
  public List<SerializablePropertyTransitionNode> propertyTransitionNodes = new List<SerializablePropertyTransitionNode>();
  public List<SerializableSequenceNode> sequenceNodes = new List<SerializableSequenceNode>();
  public List<SerializableTestNode> testNodes = new List<SerializableTestNode>();
  public List<SerializableConstuntNode> constuntNodes = new List<SerializableConstuntNode>();
  public List<SerializableCurveNode> curveNodes = new List<SerializableCurveNode>();
  public List<SerializableAddNode> addNodes = new List<SerializableAddNode>();
  public List<SerializableMultiplyNode> multiplyNodes = new List<SerializableMultiplyNode>();
  public List<SerializableSinNode> sinNodes = new List<SerializableSinNode>();
  public List<SerializableCosNode> cosNodes = new List<SerializableCosNode>();
  public List<SerializableEdge> edges = new List<SerializableEdge>();
  public void SaveAsset(GraphView graphView) {
    animationGenerateNodes.Clear();
    propertyNodes.Clear();
    propertyTransitionNodes.Clear();
    sequenceNodes.Clear();
    testNodes.Clear();
    constuntNodes.Clear();
    curveNodes.Clear();
    addNodes.Clear();
    multiplyNodes.Clear();
    sinNodes.Clear();
    cosNodes.Clear();
    edges.Clear();

    graphView.nodes.ForEach(n => {
      var node = (n as IGraphNode).graphNode;
      node.SaveAsset(this);
    });

    graphView.edges.ForEach(edge => {
      var outputNode = (edge.output.node as IGraphNode).graphNode;
      var inputNode = (edge.input.node as IGraphNode).graphNode;
      string outputPortGuid, inputPortGuid;
      if (outputNode.portGuids.TryGetValue(edge.output, out outputPortGuid)
        && inputNode.portGuids.TryGetValue(edge.input, out inputPortGuid)) {
        this.edges.Add(new SerializableEdge() {
          outputPortGuid = outputPortGuid,
          inputPortGuid = inputPortGuid
        });
      }
    });

    EditorUtility.SetDirty(this);
    AssetDatabase.SaveAssets();
  }
  public void LoadAsset(GraphView graphView) {
    foreach (var serializable in animationGenerateNodes) {
      graphView.AddElement(new AnimationGenerateNode(graphView, serializable)); }
    foreach (var serializable in propertyNodes) {
      graphView.AddElement(new PropertyNode(graphView, serializable)); }
    foreach (var serializable in propertyTransitionNodes) {
      graphView.AddElement(new PropertyTransitionNode(graphView, serializable)); }
    foreach (var serializable in sequenceNodes) {
      graphView.AddElement(new SequenceNode(graphView, serializable)); }
    foreach (var serializable in testNodes) {
      graphView.AddElement(new TestNode(graphView, serializable)); }
    foreach (var serializable in constuntNodes) {
      graphView.AddElement(new ConstuntNode(graphView, serializable)); }
    foreach (var serializable in curveNodes) {
      graphView.AddElement(new CurveNode(graphView, serializable)); }
    foreach (var serializable in addNodes) {
      graphView.AddElement(new AddNode(graphView, serializable)); }
    foreach (var serializable in multiplyNodes) {
      graphView.AddElement(new MultiplyNode(graphView, serializable)); }
    foreach (var serializable in sinNodes) {
      graphView.AddElement(new SinNode(graphView, serializable)); }
    foreach (var serializable in cosNodes) {
      graphView.AddElement(new CosNode(graphView, serializable)); }

    var nodes = graphView.nodes.ToList()
      .Cast<IGraphNode>().Select(n => n.graphNode).ToList();
    foreach (var e in this.edges) {
      Port outputPort = null, inputPort = null;
      foreach (var n in nodes) {
        if (n.guidPorts.TryGetValue(e.outputPortGuid, out outputPort)) break;
      }
      foreach (var n in nodes) {
        if (n.guidPorts.TryGetValue(e.inputPortGuid, out inputPort)) break;
      }
      if (outputPort == null || inputPort == null) break;
      var edge = outputPort.ConnectTo(inputPort);
      graphView.Add(edge);
    }
  }
}
[Serializable]
public class SerializableEdge {
  public string outputPortGuid;
  public string inputPortGuid;
}
}
