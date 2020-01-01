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
                root = applyTransformation(root);
                Common.SaveTransformation(root, csFile, Convert.ToString(1));
            }
        }

        private CompilationUnitSyntax applyTransformation(CompilationUnitSyntax root)
        {
            var loopNodes = root.DescendantNodes()
                .OfType<StatementSyntax>()
                .Where(node => !IsNotPermeableStatement(node)).ToList();
            if (loopNodes.Count > 1)
            {
                loopNodes.RemoveAt(0); // main block
                int place = new Random().Next(0, loopNodes.Count); // overflow +1
                StatementSyntax tryStr = getTryCatch(loopNodes.ElementAt(place));
                root = root.ReplaceNode(loopNodes.ElementAt(place), tryStr);
            }
            else
            {
                //TODO, i.e. {int r = result(); return r;}
            }
            return root;
        }

        private StatementSyntax getTryCatch(StatementSyntax stmt)
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

        private bool IsNotPermeableStatement(StatementSyntax node)
        {
            return (
                node.IsKind(SyntaxKind.EmptyStatement) ||
                node.IsKind(SyntaxKind.Block) ||
                node.IsKind(SyntaxKind.LocalDeclarationStatement) ||
                node.IsKind(SyntaxKind.GotoStatement) ||
                node.IsKind(SyntaxKind.LabeledStatement) ||
                node.IsKind(SyntaxKind.BreakStatement) ||
                node.IsKind(SyntaxKind.ContinueStatement) ||
                node.IsKind(SyntaxKind.ReturnStatement)
            );
        }
    }
}
