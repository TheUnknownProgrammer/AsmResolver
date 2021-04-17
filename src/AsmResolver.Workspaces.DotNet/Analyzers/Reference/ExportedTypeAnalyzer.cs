using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Reference
{
    /// <summary>
    /// Analyzes a <see cref="ExportedType"/> for its definitions
    /// </summary>
    public class ExportedTypeAnalyzer : ObjectAnalyzer<ExportedType>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, ExportedType subject)
        {
            if (subject.DeclaringType is not null)
            {
                context.ScheduleForAnalysis(subject.DeclaringType);
            }

            if (context.Workspace is not DotNetWorkspace workspace)
                return;

            var definition = subject.Resolve();
            if (definition is null || !workspace.Assemblies.Contains(definition.Module.Assembly))
                return;

            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(definition);
            var candidateNode = index.GetOrCreateNode(subject);
            node.OutgoingEdges.Add(DotNetRelations.ReferenceExportedType, candidateNode);
        }
    }
}