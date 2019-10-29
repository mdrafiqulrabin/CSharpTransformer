﻿using System;
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
                var loopExchange = new ApplyLoopExchange();
                root = (CompilationUnitSyntax)loopExchange.Visit(root);
                Common.SaveTransformation(root, csFile);
            }
        }
    }

    public class ApplyLoopExchange : CSharpSyntaxRewriter
    {
        public ApplyLoopExchange() {}

        public override SyntaxNode Visit(SyntaxNode node)
        {
            if (node.IsKind(SyntaxKind.ForStatement))
            {
                ExpressionSyntax condition = (node as ForStatementSyntax).Condition;
                var innerStatements = new SyntaxList<StatementSyntax>();
                foreach (var v in ((node as ForStatementSyntax).Statement as BlockSyntax).Statements)
                {
                    innerStatements = innerStatements.Add(v);
                }
                foreach (var v in (node as ForStatementSyntax).Incrementors)
                {
                    innerStatements = innerStatements.Add(SyntaxFactory.ExpressionStatement(v));
                }
                var innerBlock = SyntaxFactory.Block(innerStatements);
                var whileLoop = SyntaxFactory.WhileStatement(condition, innerBlock);

                var outerStatements = new SyntaxList<StatementSyntax>();
                outerStatements = outerStatements.Add(SyntaxFactory.ParseStatement((node as ForStatementSyntax).Declaration + ";"));
                foreach (var v in (node as ForStatementSyntax).Initializers)
                {
                    outerStatements = outerStatements.Add(SyntaxFactory.ExpressionStatement(v));
                }
                outerStatements = outerStatements.Add(whileLoop);
                var outerBlock = SyntaxFactory.Block(outerStatements);

                return outerBlock;
            }
            else if (node.IsKind(SyntaxKind.WhileStatement))
            {
                SeparatedSyntaxList<ExpressionSyntax> initializers = new SeparatedSyntaxList<ExpressionSyntax>();
                ExpressionSyntax condition = (node as WhileStatementSyntax).Condition;
                SeparatedSyntaxList<ExpressionSyntax> incrementors = new SeparatedSyntaxList<ExpressionSyntax>();
                StatementSyntax statement = (node as WhileStatementSyntax).Statement;
                var retVal = SyntaxFactory.ForStatement(null, initializers, condition, incrementors, statement);
                return retVal;
            }
            return base.Visit(node);
        }
    }
}