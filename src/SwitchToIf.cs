﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class SwitchToIf
    {
        private readonly Common mCommon;

        public SwitchToIf()
        {
            //Console.WriteLine("\n[ SwitchToIf ]\n");
            mCommon = new Common();
        }

        public void InspectSourceCode(String csFile)
        {
            String savePath = Common.mRootOutputPath + this.GetType().Name + "/";
            CompilationUnitSyntax root = mCommon.GetParseUnit(csFile);
            if (root != null)
            {
                var switchNodes = root.DescendantNodes().
                    OfType<SwitchStatementSyntax>().ToList();

                int programId = 0;
                for (int place = 0; place < switchNodes.Count; place++)
                {
                    var switchNode = switchNodes.ElementAt(place);
                    var modSwitchNode = ApplySwitchToIf(switchNode);
                    if (modSwitchNode != null)
                    {
                        programId++;
                        var modRoot = root.ReplaceNode(switchNode, modSwitchNode);
                        mCommon.SaveTransformation(savePath, modRoot, csFile, Convert.ToString(programId));
                    }
                }
            }
        }

        private IfStatementSyntax ApplySwitchToIf(SwitchStatementSyntax node)
        {
            List<IfStatementSyntax> allIfStatementSyntax = new List<IfStatementSyntax>();
            ElseClauseSyntax lastElseClauseSyntax = null;

            SwitchStatementSyntax switchStatementSyntax = (SwitchStatementSyntax)node;

            foreach (SwitchSectionSyntax switchSectionSyntax in switchStatementSyntax.Sections)
            {
                bool defaultCase = false;

                String expressionSyntaxes = "";
                foreach (SwitchLabelSyntax switchLabelSyntax in switchSectionSyntax.Labels)
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

                SyntaxList<StatementSyntax> ifElseStatementSyntaxes = new SyntaxList<StatementSyntax>();
                foreach (StatementSyntax caseStatementSyntax in switchSectionSyntax.Statements)
                {
                    if (caseStatementSyntax.Kind() == SyntaxKind.Block)
                    {
                        foreach (StatementSyntax blockStatementSyntax in ((BlockSyntax)caseStatementSyntax).Statements)
                        {
                            if (blockStatementSyntax.Kind() != SyntaxKind.BreakStatement)
                            {
                                ifElseStatementSyntaxes = ifElseStatementSyntaxes.Add(blockStatementSyntax);
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
                    }
                }
                StatementSyntax ifElseBlock = SyntaxFactory.Block(ifElseStatementSyntaxes);

                if (defaultCase)
                {
                    ElseClauseSyntax elseClauseSyntax = SyntaxFactory.ElseClause(ifElseBlock);
                    lastElseClauseSyntax = elseClauseSyntax;
                }
                else
                {
                    IfStatementSyntax ifStatementSyntax = SyntaxFactory.IfStatement(ifElseCondition, ifElseBlock);
                    allIfStatementSyntax.Add(ifStatementSyntax);
                }
            }

            IfStatementSyntax mIfStatementSyntax = null;
            if (allIfStatementSyntax.Count == 0)
            {
                /*
                ExpressionSyntax defaultCondition = SyntaxFactory.ParseExpression(switchStatementSyntax.Expression + "==" + switchStatementSyntax.Expression);
                StatementSyntax defaultStatement = SyntaxFactory.ParseStatement("{\n}"); //empty
                if (lastElseClauseSyntax != null) //only default
                {
                    defaultStatement = lastElseClauseSyntax.Statement;
                }
                return SyntaxFactory.IfStatement(defaultCondition, defaultStatement);
                */
                return null;
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
    }
}
