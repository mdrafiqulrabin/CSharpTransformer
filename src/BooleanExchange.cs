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
                if (booleanNodes.Count > 1)
                {
                    ApplyToPlace(csFile, orgRoot, booleanNodes, false);
                }
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
                    && token.Parent.IsKind(SyntaxKind.VariableDeclarator)
                    && token.Parent.Parent.IsKind(SyntaxKind.VariableDeclaration))
                {
                    VariableDeclarationSyntax vds = (VariableDeclarationSyntax)token.Parent.Parent;
                    if (vds.Type.ToString().ToLower().Equals("bool")
                        || vds.Type.ToString().ToLower().Equals("boolean"))
                    {
                        var boolVal = ((VariableDeclaratorSyntax)token.Parent).Initializer.Value.ToString().ToLower();
                        if (boolVal.Equals("true") || boolVal.Equals("false"))
                        {
                            mBooleanNodes.Add(token);
                        }
                    }
                }
                base.VisitToken(token);
            }
        }

        private static String getNotExpStr(String nodeStr, String nodeRef)
        {
            if (nodeStr.Equals("true"))
            {
                return "false";
            } else if (nodeStr.Equals("false"))
            {
                return "true";
            }
            else if (nodeStr.StartsWith("!", StringComparison.Ordinal) &&
                nodeStr.TrimStart('!').Equals(nodeRef))
            {
                return nodeStr.TrimStart('!');
            }
            else
            {
                String expStr = "!";
                if (nodeStr.Length > 1)
                {
                    expStr += "(" + nodeStr + ")";
                }
                else
                {
                    expStr += nodeStr;
                }
                return expStr;
            }
        }

        class IdentifierNotRewriter : CSharpSyntaxRewriter
        {
            private String mBooleanNode;
            public IdentifierNotRewriter(String booleanNode)
            {
                mBooleanNode = booleanNode;
            }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node != null && (
                    (node.IsKind(SyntaxKind.LogicalNotExpression) && node.ToString().Equals("!"+mBooleanNode))
                    || (node.IsKind(SyntaxKind.IdentifierName) && node.ToString().Equals(mBooleanNode))))
                {
                    return SyntaxFactory.ParseExpression(getNotExpStr(node.ToString(),mBooleanNode));
                }
                return base.Visit(node);
            }
        }

        public class ApplyBooleanExchange : CSharpSyntaxRewriter
        {
            private String mBooleanNode;
            public ApplyBooleanExchange(String booleanNode)
            {
                mBooleanNode = booleanNode;
            }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node != null &&
                    (node.IsKind(SyntaxKind.TrueLiteralExpression)
                        || node.IsKind(SyntaxKind.FalseLiteralExpression)))
                {
                    //i.e. x=true/false --> x=false/true
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
                    if (identifier != null && identifier.Equals(mBooleanNode))
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
                else if (node != null && node.IsKind(SyntaxKind.LogicalNotExpression)
                    && ((PrefixUnaryExpressionSyntax)node).Operand.ToString().Equals(mBooleanNode))
                {
                    //i.e. !x --> x
                    return SyntaxFactory.ParseExpression(((PrefixUnaryExpressionSyntax)node).Operand.ToString());
                }
                else if (node != null && node.Parent != null && node.ToString().Equals(mBooleanNode) &&
                            !(node.Parent.IsKind(SyntaxKind.VariableDeclaration)
                                || node.Parent.IsKind(SyntaxKind.DeclarationExpression)
                                || (node.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression)
                                    && ((AssignmentExpressionSyntax)node.Parent).Left.ToString().Equals(mBooleanNode))))
                {
                    if (node.IsKind(SyntaxKind.Argument))
                    {
                        //i.e. call(x) --> call(!x)
                        var argumentListSyntax = SyntaxFactory.ParseArgumentList(getNotExpStr(node.ToString(), mBooleanNode));
                        return argumentListSyntax.ChildNodes().First();
                    }
                    else
                    {
                        //i.e. y=x --> y=!x
                        return SyntaxFactory.ParseExpression(getNotExpStr(node.ToString(), mBooleanNode));
                    }
                }
                else if (node != null && node.Parent != null
                    && node.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression)
                    && ((AssignmentExpressionSyntax)node.Parent).Left.ToString().Equals(mBooleanNode)
                    && ((AssignmentExpressionSyntax)node.Parent).Right.ToString().Equals(node.ToString()))
                {
                    //i.e. x = call(!x,r(x)) --> x = !(call(!!x,r(!x)))
                    node = new IdentifierNotRewriter(mBooleanNode).Visit(node);
                    return SyntaxFactory.ParseExpression(getNotExpStr(node.ToString(), mBooleanNode));
                }
                else if (node != null && node.Parent != null
                    && node.IsKind(SyntaxKind.EqualsValueClause)
                    && node.Parent.IsKind(SyntaxKind.VariableDeclarator)
                    && ((VariableDeclaratorSyntax)node.Parent).Identifier.ToString().Equals(mBooleanNode)
                    && ((VariableDeclaratorSyntax)node.Parent).Initializer.Value.ToString().Equals(((EqualsValueClauseSyntax)node).Value.ToString()))
                {
                    //i.e. boolean y = x --> boolean y = !x
                    return ((EqualsValueClauseSyntax)node).Update(
                        ((EqualsValueClauseSyntax)node).EqualsToken,
                        SyntaxFactory.ParseExpression(getNotExpStr(((EqualsValueClauseSyntax)node).Value.ToString(), mBooleanNode)));
                }
                return base.Visit(node);
            }
        }
    }
}
