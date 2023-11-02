using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace DarknessUnbound.CodeAssist.DirectionPossibleValues;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DirectionPossibleValuesDiagnosticAnalyzer : AbstractDiagnosticAnalyzer {
    public static readonly DiagnosticDescriptor RULE = new(
        Diagnostics.DirectionPossibleValues.ID,
        Diagnostics.DirectionPossibleValues.TITLE,
        Diagnostics.DirectionPossibleValues.MESSAGE_FORMAT,
        Categories.USAGE,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Diagnostics.DirectionPossibleValues.DESCRIPTION
    );

    public DirectionPossibleValuesDiagnosticAnalyzer() : base(RULE) { }

    protected override void InitializeWorker(AnalysisContext context) {
        context.RegisterOperationAction(
            static (context) => {
                if (context.Operation is not ISimpleAssignmentOperation { Target: IFieldReferenceOperation fieldRef, Value: ILiteralOperation { ConstantValue: { HasValue: true, Value: not -1 and not 1 } } } isaOperation)
                    return;

                var fieldSymbol = fieldRef.Field;
                if (fieldSymbol.ContainingType.IsSameAsFullyQualifiedString(TERRARIA_ENTITY) && fieldSymbol.Name.Equals(DIRECTION))
                    context.ReportDiagnostic(Diagnostic.Create(RULE, isaOperation.Syntax.GetLocation()));
            },
            OperationKind.SimpleAssignment
        );
    }
}
