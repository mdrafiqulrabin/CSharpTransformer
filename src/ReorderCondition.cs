using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class ReorderCondition
    {
        public ReorderCondition()
        {
            //Console.WriteLine("\n[ ReorderCondition ]\n");
        }

        public void InspectSourceCode(String csFile)
        {
            Common.SetOutputPath(this, csFile);
            CompilationUnitSyntax root = Common.GetParseUnit(csFile);
            if (root != null)
            {
                var binaryExNodes = root.DescendantNodes().OfType<BinaryExpressionSyntax>()
                    .Where(IsReorderApplicable).ToList();

                // apply to single place
                int programId = 0;
                for (int place = 0; place < binaryExNodes.Count; place++)
                {
                    var binExNode = binaryExNodes.ElementAt(place);
                    var modBinExNode = ApplyReorderCondition(binExNode);
                    if (modBinExNode != null)
                    {
                        programId++;
                        var modRoot = root.ReplaceNode(binExNode, modBinExNode);
                        Common.SaveTransformation(modRoot, csFile, Convert.ToString(programId));
                    }
                }

                // apply to all place
                if (binaryExNodes.Count > 1)
                {
                    for (int place = 0; place < binaryExNodes.Count; place++)
                    {
                        var binExNode = binaryExNodes.ElementAt(place);
                        var modBinExNode = ApplyReorderCondition(binExNode);
                        if (modBinExNode != null)
                        {
                            root = root.ReplaceNode(binExNode, modBinExNode);
                            binaryExNodes = root.DescendantNodes().OfType<BinaryExpressionSyntax>()
                                .Where(IsReorderApplicable).ToList();
                        }
                    }
                    Common.SaveTransformation(root, csFile, Convert.ToString(0));
                }
            }
        }

        private static BinaryExpressionSyntax ApplyReorderCondition(BinaryExpressionSyntax node)
        {
            SyntaxKind newSyntaxKind;
            switch (node.Kind())
            {
                case SyntaxKind.LessThanExpression:
                    newSyntaxKind = SyntaxKind.GreaterThanExpression;
                    break;
                case SyntaxKind.LessThanOrEqualExpression:
                    newSyntaxKind = SyntaxKind.GreaterThanOrEqualExpression;
                    break;
                case SyntaxKind.GreaterThanExpression:
                    newSyntaxKind = SyntaxKind.LessThanExpression;
                    break;
                case SyntaxKind.GreaterThanOrEqualExpression:
                    newSyntaxKind = SyntaxKind.LessThanOrEqualExpression;
                    break;
                case SyntaxKind.LogicalOrExpression:
                    newSyntaxKind = SyntaxKind.LogicalOrExpression;
                    break;
                case SyntaxKind.LogicalAndExpression:
                    newSyntaxKind = SyntaxKind.LogicalAndExpression;
                    break;
                case SyntaxKind.EqualsExpression:
                    newSyntaxKind = SyntaxKind.EqualsExpression;
                    break;
                case SyntaxKind.NotEqualsExpression:
                    newSyntaxKind = SyntaxKind.NotEqualsExpression;
                    break;
                default:
                    return null;
            }

            return SyntaxFactory.BinaryExpression(newSyntaxKind, node.Right, node.Left);
        }

        private bool IsReorderApplicable(BinaryExpressionSyntax node)
        {
            return (
                node.IsKind(SyntaxKind.LessThanExpression) ||
                node.IsKind(SyntaxKind.LessThanOrEqualExpression) ||
                node.IsKind(SyntaxKind.GreaterThanExpression) ||
                node.IsKind(SyntaxKind.GreaterThanOrEqualExpression) ||
                node.IsKind(SyntaxKind.LogicalOrExpression) ||
                node.IsKind(SyntaxKind.LogicalAndExpression) ||
                node.IsKind(SyntaxKind.EqualsExpression) ||
                node.IsKind(SyntaxKind.NotEqualsExpression)
            );
        }
    }
}
