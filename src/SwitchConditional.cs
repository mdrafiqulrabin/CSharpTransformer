using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CSharpTransformer.src
{
    public class SwitchConditional
    {
        public SwitchConditional()
        {
            Console.WriteLine("\n[ SwitchConditional ]\n");
        }

        public void InspectSourceCode()
        {
            var rootDir = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            var path = Path.Combine(rootDir, "data/original/");
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                Console.WriteLine("File = " + Path.GetFileName(file));
                var txtCode = File.ReadAllText(file);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(txtCode);
                var root = (CompilationUnitSyntax)tree.GetRoot();
                Console.WriteLine("Original = \n" + root + "\n");

                var switchConditional = new ApplySwitchConditional();
                root = (CompilationUnitSyntax)switchConditional.Visit(root);
                root = (CompilationUnitSyntax)Formatter.Format(root, new AdhocWorkspace());
                Console.WriteLine("Transformed = \n" + root + "\n");
            }
        }
    }

    public class ApplySwitchConditional : CSharpSyntaxRewriter
    {
        public ApplySwitchConditional() { }

        public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
        { 
            if (node.IsKind(SyntaxKind.SwitchStatement))
            {
                List<IfStatementSyntax> allIfStatementSyntax = new List<IfStatementSyntax>();
                ElseClauseSyntax lastElseClauseSyntax = null;

                SwitchStatementSyntax switchStatementSyntax = (SwitchStatementSyntax)node;

                foreach (SwitchSectionSyntax switchSectionSyntax in switchStatementSyntax.Sections)
                {
                    bool defaultCase = false;

                    String expressionSyntaxes = "";
                    foreach(SwitchLabelSyntax switchLabelSyntax in switchSectionSyntax.Labels)
                    {
                        if (switchLabelSyntax.Keyword.ToString().Equals("default"))
                        {
                            defaultCase = true;
                        }
                        foreach (SyntaxNode caseSyntaxNode in switchLabelSyntax.ChildNodes())
                        {
                            if (expressionSyntaxes.Length != 0)
                            {
                                expressionSyntaxes = expressionSyntaxes + " || ";
                            }
                            expressionSyntaxes = expressionSyntaxes + "(" + switchStatementSyntax.Expression + "==" + caseSyntaxNode + ")";
                        }
                    }
                    if (Regex.Matches(expressionSyntaxes, "==").Count == 1)
                    {
                        expressionSyntaxes = expressionSyntaxes.Trim('(', ')');
                    }
                    ExpressionSyntax ifElseCondition = SyntaxFactory.ParseExpression(expressionSyntaxes);
                    //Console.WriteLine("ifElseCondition = " + ifElseCondition);

                    SyntaxList <StatementSyntax> ifElseStatementSyntaxes = new SyntaxList<StatementSyntax>();
                    foreach (StatementSyntax caseStatementSyntax in switchSectionSyntax.Statements)
                    {
                        if (caseStatementSyntax.Kind() == SyntaxKind.Block)
                        {
                            foreach (StatementSyntax blockStatementSyntax in ((BlockSyntax)caseStatementSyntax).Statements)
                            {
                                if (blockStatementSyntax.Kind() != SyntaxKind.BreakStatement)
                                {
                                    ifElseStatementSyntaxes = ifElseStatementSyntaxes.Add(blockStatementSyntax);
                                    //Console.WriteLine("blockStatementSyntax = " + blockStatementSyntax);
                                }
                            }
                        }
                        else if (caseStatementSyntax.Kind() == SyntaxKind.BreakStatement)
                        {
                            continue;
                        }
                        else
                        {
                            ifElseStatementSyntaxes = ifElseStatementSyntaxes.Add(caseStatementSyntax);
                            //Console.WriteLine("statementSyntax = " + caseStatementSyntax);
                        }
                    }
                    StatementSyntax ifElseBlock = SyntaxFactory.Block(ifElseStatementSyntaxes);
                    //Console.WriteLine("ifElseStatement = " + ifElseStatement);

                    if (defaultCase)
                    {
                        ElseClauseSyntax elseClauseSyntax = SyntaxFactory.ElseClause(ifElseBlock);
                        lastElseClauseSyntax = elseClauseSyntax;
                        //Console.WriteLine("elseClauseSyntax = " + elseClauseSyntax);
                    }
                    else
                    {
                        IfStatementSyntax ifStatementSyntax = SyntaxFactory.IfStatement(ifElseCondition, ifElseBlock);
                        //Console.WriteLine("ifStatementSyntax = " + ifStatementSyntax);
                        allIfStatementSyntax.Add(ifStatementSyntax);
                    }
                    //Console.WriteLine("\n");
                }

                IfStatementSyntax mIfStatementSyntax = null;
                if (allIfStatementSyntax.Count == 0 && lastElseClauseSyntax != null) //only default
                {
                    ExpressionSyntax onlyDefaultCondition = SyntaxFactory.ParseExpression(switchStatementSyntax.Expression + "==" + switchStatementSyntax.Expression);
                    IfStatementSyntax onlyDefaultIf = SyntaxFactory.IfStatement(onlyDefaultCondition, lastElseClauseSyntax.Statement);
                    return onlyDefaultIf;
                }
                else
                {
                    for (int i = allIfStatementSyntax.Count - 1; i >= 0; i--)
                    {
                        if (mIfStatementSyntax == null)
                        {
                            if (lastElseClauseSyntax == null)
                            {
                                mIfStatementSyntax = allIfStatementSyntax[i];
                            }
                            else
                            {
                                mIfStatementSyntax = SyntaxFactory.IfStatement(allIfStatementSyntax[i].Condition, allIfStatementSyntax[i].Statement, lastElseClauseSyntax);

                            }
                        }
                        else
                        {
                            mIfStatementSyntax = SyntaxFactory.IfStatement(allIfStatementSyntax[i].Condition, allIfStatementSyntax[i].Statement, SyntaxFactory.ElseClause(mIfStatementSyntax));

                        }
                    }
                    return mIfStatementSyntax;
                }
            }
            return base.Visit(node);
        }
    }
}
