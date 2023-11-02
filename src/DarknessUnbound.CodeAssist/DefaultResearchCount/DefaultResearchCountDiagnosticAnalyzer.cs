using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace DarknessUnbound.CodeAssist.DefaultResearchCount;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DefaultResearchCountDiagnosticAnalyzer : AbstractDiagnosticAnalyzer {
    public static readonly DiagnosticDescriptor RULE = new(
        Diagnostics.DefaultResourceCount.ID,
        Diagnostics.DefaultResourceCount.TITLE,
        Diagnostics.DefaultResourceCount.MESSAGE_FORMAT,
        Categories.USAGE,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Diagnostics.DefaultResourceCount.DESCRIPTION
    );

    public DefaultResearchCountDiagnosticAnalyzer() : base(RULE) { }

    protected override void InitializeWorker(AnalysisContext context) {
        context.RegisterOperationAction(
            static (context) => {
                // If this operation is not a simple assignment operation, the
                // target is not a property, or the property is not being set to
                // a constant literal value of 1, we don't care.
                if (context.Operation is not ISimpleAssignmentOperation { Target: IPropertyReferenceOperation propertyRef, Value: ILiteralOperation { ConstantValue: { HasValue: true, Value: 1 } } } isaOperation)
                    return;

                // Make sure the property's containing type is Terraria.Item and
                // the property's name is ResearchUnlockCount.
                // Since we checked the value earlier, we can report the
                // diagnostic now.
                var propertySymbol = propertyRef.Property;
                if (propertySymbol.ContainingType.IsSameAsFullyQualifiedString(TERRARIA_ITEM) && propertySymbol.Name.Equals(RESEARCH_UNLOCK_COUNT))
                    context.ReportDiagnostic(Diagnostic.Create(RULE, isaOperation.Syntax.GetLocation()));
            },
            OperationKind.SimpleAssignment
        );
    }
}
