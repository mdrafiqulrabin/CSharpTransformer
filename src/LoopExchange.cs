using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class LoopExchange
    {
        public LoopExchange()
        {
            //Console.WriteLine("\n[ LoopExchange ]\n");
        }

        public void InspectSourceCode(String csFile)
        {
            Common.SetOutputPath(this, csFile);
            CompilationUnitSyntax root = Common.GetParseUnit(csFile);
            if (root != null)
            {
                // apply to single place
                var loopNodes = root.DescendantNodes().Where(n => (n.IsKind(SyntaxKind.ForStatement) || n.IsKind(SyntaxKind.WhileStatement))).ToList();
                for (int place = 0; place < loopNodes.Count; place++)
                {
                    var loopNode = loopNodes.ElementAt(place);
                    if (loopNode.IsKind(SyntaxKind.ForStatement))
                    {
                        var modRoot = root.ReplaceNode(loopNode, ForToWhile((ForStatementSyntax)loopNode));
                        Common.SaveTransformation(modRoot, csFile, Convert.ToString(place + 1));
                    }
                    else if (loopNode.IsKind(SyntaxKind.WhileStatement))
                    {
                        var modRoot = root.ReplaceNode(loopNode, WhileToFor((WhileStatementSyntax)loopNode));
                        Common.SaveTransformation(modRoot, csFile, Convert.ToString(place + 1));
                    } else
                    {
                        continue;
                    }
                }

                // apply to all place 
                var loopExchange = new ApplyLoopExchange();
                root = (CompilationUnitSyntax)loopExchange.Visit(root);
                Common.SaveTransformation(root, csFile, Convert.ToString(0));
            }
        }

        public class ApplyLoopExchange : CSharpSyntaxRewriter
        {
            public ApplyLoopExchange() { }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node.IsKind(SyntaxKind.ForStatement))
                {
                    return ForToWhile((ForStatementSyntax)node);
                }
                else if (node.IsKind(SyntaxKind.WhileStatement))
                {
                    return WhileToFor((WhileStatementSyntax)node);
                }
                return base.Visit(node);
            }
        }

        private static SyntaxNode ForToWhile(ForStatementSyntax node)
        {
            ExpressionSyntax condition = node.Condition;
            if (condition == null)
            {
                condition = SyntaxFactory.ParseExpression("true");
            }
            var innerStatements = new SyntaxList<StatementSyntax>();
            StatementSyntax forStatements = node.Statement;
            if (forStatements != null && !forStatements.IsKind(SyntaxKind.EmptyStatement))
            {
                foreach (var v in (forStatements as BlockSyntax).Statements)
                {
                    innerStatements = innerStatements.Add(v);
                }
            }
            foreach (var v in node.Incrementors)
            {
                innerStatements = innerStatements.Add(SyntaxFactory.ExpressionStatement(v));
            }
            WhileStatementSyntax whileLoop;
            if (innerStatements.Count > 0)
            {
                var innerBlock = SyntaxFactory.Block(innerStatements);
                whileLoop = SyntaxFactory.WhileStatement(condition, innerBlock);
            }
            else
            {
                whileLoop = SyntaxFactory.WhileStatement(condition, SyntaxFactory.EmptyStatement());
            }

            var outerStatements = new SyntaxList<StatementSyntax>();
            VariableDeclarationSyntax declaration = node.Declaration;
            if (declaration != null)
            {
                outerStatements = outerStatements.Add(SyntaxFactory.ParseStatement(declaration + ";"));
            }
            foreach (var v in node.Initializers)
            {
                outerStatements = outerStatements.Add(SyntaxFactory.ExpressionStatement(v));
            }
            if (outerStatements.Count > 0)
            {
                outerStatements = outerStatements.Add(whileLoop);
                return SyntaxFactory.Block(outerStatements);
            }
            else
            {
                return whileLoop;
            }
        }

        private static SyntaxNode WhileToFor(WhileStatementSyntax node)
        {
            SeparatedSyntaxList<ExpressionSyntax> initializers = new SeparatedSyntaxList<ExpressionSyntax>();
            ExpressionSyntax condition = node.Condition;
            SeparatedSyntaxList<ExpressionSyntax> incrementors = new SeparatedSyntaxList<ExpressionSyntax>();
            StatementSyntax statement = node.Statement;
            return SyntaxFactory.ForStatement(null, initializers, condition, incrementors, statement);
        }
    }
}