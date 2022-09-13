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
        private readonly Common mCommon;

        public ReorderCondition()
        {
            //Console.WriteLine("\n[ ReorderCondition ]\n");
            mCommon = new Common();
        }

        public void InspectSourceCode(String csFile)
        {
            String savePath = Common.mRootOutputPath + this.GetType().Name + "/";
            CompilationUnitSyntax root = mCommon.GetParseUnit(csFile);
            if (root != null)
            {
                var binaryExNodes = root.DescendantNodes().OfType<BinaryExpressionSyntax>()
                    .Where(IsReorderApplicable).ToList();

                int programId = 0;
                for (int place = 0; place < binaryExNodes.Count; place++)
                {
                    var binExNode = binaryExNodes.ElementAt(place);
                    var modBinExNode = ApplyReorderCondition(binExNode);
                    if (modBinExNode != null)
                    {
                        programId++;
                        var modRoot = root.ReplaceNode(binExNode, modBinExNode);
                        mCommon.SaveTransformation(savePath, modRoot, csFile, Convert.ToString(programId));
                    }
                }
            }
        }

        private BinaryExpressionSyntax ApplyReorderCondition(BinaryExpressionSyntax node)
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
