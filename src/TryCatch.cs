using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class TryCatch
    {
        public TryCatch()
        {
            //Console.WriteLine("\n[ TryCatch ]\n");
        }

        public void InspectSourceCode(String csFile)
        {
            Common.SetOutputPath(this, csFile);
            CompilationUnitSyntax root = Common.GetParseUnit(csFile);
            if (root != null)
            {
                root = ApplyTransformation(root);
                Common.SaveTransformation(root, csFile, Convert.ToString(1));
            }
        }

        private CompilationUnitSyntax ApplyTransformation(CompilationUnitSyntax root)
        {
            var tryNodes = root.DescendantNodes().OfType<TryStatementSyntax>().ToList();
            var methodCalls = root.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
            if (tryNodes.Count > 0 || methodCalls.Count == 0) return null;

            var loopNodes = root.DescendantNodes().OfType<StatementSyntax>()
                .Where(node => IsTryCatchApplicable(node)).ToList();

            if (loopNodes.Count > 0)
            {
                int place = new Random().Next(0, loopNodes.Count); // overflow +1
                StatementSyntax tryStr = GetTryCatch(loopNodes.ElementAt(place));
                root = root.ReplaceNode(loopNodes.ElementAt(place), tryStr);
                return root;
            }
            return null;
        }

        private StatementSyntax GetTryCatch(StatementSyntax stmt)
        {
            string tryStr = "try \n" +
                    "{ \n" +
                    stmt + "\n" +
                    "} \n" +
                    "catch (Exception ex) \n" +
                    "{ \n" +
                    "Console.WriteLine(ex); \n" +
                    "} \n";
            return SyntaxFactory.ParseStatement(tryStr);
        }

        private bool IsTryCatchApplicable(StatementSyntax node)
        {
            var methodCalls = node.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
            bool isPermeableStatement = !(
                node.IsKind(SyntaxKind.EmptyStatement) ||
                node.IsKind(SyntaxKind.Block) ||
                node.IsKind(SyntaxKind.LocalDeclarationStatement) ||
                node.IsKind(SyntaxKind.GotoStatement) ||
                node.IsKind(SyntaxKind.LabeledStatement) ||
                node.IsKind(SyntaxKind.BreakStatement) ||
                node.IsKind(SyntaxKind.ContinueStatement) ||
                node.IsKind(SyntaxKind.ReturnStatement)
            );
            return methodCalls.Count > 0 && isPermeableStatement;
        }
    }
}
