using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace DarknessUnbound.CodeAssist.CodeFixes;

public abstract class AbstractCodeFixer : CodeFixProvider {
    protected readonly record struct Parameters(in SyntaxNode Root, in Diagnostic Diagnostic) {
        public  TextSpan DiagnosticSpan => Diagnostic.Location.SourceSpan;
    }

    public override ImmutableArray<string> FixableDiagnosticIds { get; }

    protected AbstractCodeFixer(string diagnosticId) {
        FixableDiagnosticIds = ImmutableArray.Create(diagnosticId);
    }

    public override FixAllProvider? GetFixAllProvider() {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics[0];
        var parameters = new Parameters(root, diagnostic);
        await RegisterAsync(context, parameters);
    }

    protected virtual Task RegisterAsync(CodeFixContext context, in Parameters parameters) {
        return Task.CompletedTask;
    }
}
