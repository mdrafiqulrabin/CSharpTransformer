using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class BooleanExchange
    {
        public BooleanExchange()
        {
            //Console.WriteLine("\n[ BooleanExchange ]\n");
        }

        public void InspectSourceCode(String csFile)
        {
            Common.SetOutputPath(this, csFile);
            CompilationUnitSyntax orgRoot = Common.GetParseUnit(csFile);
            if (orgRoot != null)
            {
                var locateBooleans = new LocateBooleans();
                locateBooleans.Visit(orgRoot);
                HashSet<SyntaxToken> booleanNodes = locateBooleans.GetBooleanList();
                ApplyToPlace(csFile, orgRoot, booleanNodes, false);
                ApplyToPlace(csFile, orgRoot, booleanNodes, true);
            }
        }

        public void ApplyToPlace(String csFile, CompilationUnitSyntax orgRoot,
            HashSet<SyntaxToken> booleanNodes, bool singlePlace)
        {
            int placeId = 0;
            CompilationUnitSyntax modRoot = orgRoot;
            foreach (var booleanNode in booleanNodes)
            {
                var booleanExchange = new ApplyBooleanExchange(booleanNode.ToString());
                if (singlePlace)
                {
                    modRoot = Common.GetParseUnit(csFile);
                }
                modRoot = (CompilationUnitSyntax)booleanExchange.Visit(modRoot);
                if (singlePlace)
                {
                    placeId++;
                    Common.SaveTransformation(modRoot, csFile, Convert.ToString(placeId));
                }
            }
            if (!singlePlace)
            {
                Common.SaveTransformation(modRoot, csFile, Convert.ToString(placeId));
            }
        }

        public class LocateBooleans : CSharpSyntaxWalker
        {
            private HashSet<SyntaxToken> mBooleanNodes;

            public HashSet<SyntaxToken> GetBooleanList()
            {
                return mBooleanNodes;
            }

            public LocateBooleans() : base(SyntaxWalkerDepth.Token)
            {
                mBooleanNodes = new HashSet<SyntaxToken>();
            }

            public override void Visit(SyntaxNode node)
            {
                base.Visit(node);
            }

            public override void VisitToken(SyntaxToken token)
            {
                if (token.IsKind(SyntaxKind.IdentifierToken)
                    && token.Parent.IsKind(SyntaxKind.VariableDeclarator))
                {
                    var initObj = ((VariableDeclaratorSyntax)token.Parent).Initializer;
                    if (initObj != null)
                    {
                        String initVal = initObj.Value.ToString().ToLower();
                        if (initVal.Equals("true") || initVal.Equals("false"))
                        {
                            mBooleanNodes.Add(token);
                        }
                    }
                }
                base.VisitToken(token);
            }
        }

        public class ApplyBooleanExchange : CSharpSyntaxRewriter
        {
            private String booleanNode;
            public ApplyBooleanExchange(String bolNode)
            {
                booleanNode = bolNode;
            }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node != null &&
                    (node.IsKind(SyntaxKind.TrueLiteralExpression)
                        || node.IsKind(SyntaxKind.FalseLiteralExpression)))
                {
                    String identifier = null;
                    var declarators = node.Ancestors().OfType<VariableDeclaratorSyntax>().ToList();
                    if (declarators.Count != 0)
                    {
                        identifier = ((VariableDeclaratorSyntax)declarators.First()).Identifier.ToString();
                    } else
                    {
                        var expressions = node.Ancestors().OfType<AssignmentExpressionSyntax>().ToList();
                        if (expressions.Count != 0)
                        {
                            if (((AssignmentExpressionSyntax)expressions.First()).Left.IsKind(SyntaxKind.IdentifierName)) {
                                identifier = ((AssignmentExpressionSyntax)expressions.First()).Left.ToString();
                            } else
                            {
                                identifier = ((AssignmentExpressionSyntax)expressions.First()).Right.ToString();
                            }
                        } else
                        {
                            /*var equals = node.Ancestors().OfType<BinaryExpressionSyntax>().ToList();
                            if (equals.Count != 0)
                            {
                                if (((BinaryExpressionSyntax)equals.First()).Left.IsKind(SyntaxKind.IdentifierName))
                                {
                                    identifier = ((BinaryExpressionSyntax)equals.First()).Left.ToString();
                                }
                                else
                                {
                                    identifier = ((BinaryExpressionSyntax)equals.First()).Right.ToString();
                                }
                            }*/
                        }
                    }
                    if (identifier != null && identifier.Equals(booleanNode))
                    {
                        if (node.IsKind(SyntaxKind.TrueLiteralExpression))
                        {
                            return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
                        }
                        else if (node.IsKind(SyntaxKind.FalseLiteralExpression))
                        {
                            return SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
                        }
                    }
                }
                else if (node != null && node.Parent != null && node.ToString().Equals(booleanNode) &&
                            !(node.Parent.IsKind(SyntaxKind.VariableDeclaration)
                                || (node.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression)
                                    && ((AssignmentExpressionSyntax)node.Parent).Left.ToString().Equals(booleanNode))))
                {
                    return SyntaxFactory.ParseExpression("!" + node);
                }
                return base.Visit(node);
            }
        }
    }
}
