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
                var loopNodes = root.DescendantNodes().Where(node =>
                        (node.IsKind(SyntaxKind.ForStatement)
                        || node.IsKind(SyntaxKind.WhileStatement))).ToList();

                // apply to single place
                int programId = 0;
                for (int place = 0; place < loopNodes.Count; place++)
                {
                    var loopNode = loopNodes.ElementAt(place);
                    var modRoot = ReplaceLoopNode(root, loopNode);
                    if (modRoot != null)
                    {
                        programId++;
                        Common.SaveTransformation(modRoot, csFile, Convert.ToString(programId));
                    }
                }

                // apply to all place
                if (loopNodes.Count > 1)
                {
                    for (int place = 0; place < loopNodes.Count; place++)
                    {
                        var loopNode = loopNodes.ElementAt(place);
                        root = ReplaceLoopNode(root, loopNode);
                        loopNodes = root.DescendantNodes().Where(node =>
                            (node.IsKind(SyntaxKind.ForStatement)
                            || node.IsKind(SyntaxKind.WhileStatement))).ToList();
                    }
                    Common.SaveTransformation(root, csFile, Convert.ToString(0));
                }
            }
        }

        private CompilationUnitSyntax ReplaceLoopNode(CompilationUnitSyntax root, SyntaxNode loopNode)
        {
            if (loopNode.IsKind(SyntaxKind.ForStatement))
            {
                return root.ReplaceNode(loopNode, ForToWhile((ForStatementSyntax)loopNode));
            }
            else if (loopNode.IsKind(SyntaxKind.WhileStatement))
            {
                return root.ReplaceNode(loopNode, WhileToFor((WhileStatementSyntax)loopNode));
            }
            else
            {
                return root;
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
                if (forStatements.IsKind(SyntaxKind.Block))
                {
                    foreach (var v in (forStatements as BlockSyntax).Statements)
                    {
                        innerStatements = innerStatements.Add(v);
                    }
                }
                else
                {
                    innerStatements = innerStatements.Add(forStatements);
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
