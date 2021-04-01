using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using GraphProcessor;

namespace AnimationGraph_NGP {
public class ExampleGraphProcessor : BaseGraphProcessor {
  private List<BaseNode> _processList;
  public float Result { get; private set; }

  public ExampleGraphProcessor(BaseGraph graph) : base(graph) {
  }

  public override void UpdateComputeOrder() {
    _processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
  }
  
  public override void Run() {
    var count = _processList.Count;

    // すべてのノードを順番に処理する
    for (var i = 0; i < count; i++) {
      _processList[i].OnProcess();
    }

    JobHandle.ScheduleBatchedJobs();

    // Resultノードを取得する
    var resultNode = _processList.OfType<ResultNode>().FirstOrDefault();
    Result = resultNode.Result;
  }
}
}
