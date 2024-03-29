﻿using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class TryCatch
    {
        private readonly Common mCommon;

        public TryCatch()
        {
            //Console.WriteLine("\n[ TryCatch ]\n");
            mCommon = new Common();
        }

        public void InspectSourceCode(String csFile)
        {
            String savePath = Common.mRootOutputPath + this.GetType().Name + "/";
            CompilationUnitSyntax root = mCommon.GetParseUnit(csFile);
            if (root != null)
            {
                root = ApplyTransformation(root);
                mCommon.SaveTransformation(savePath, root, csFile, Convert.ToString(1));
            }
        }

        private CompilationUnitSyntax ApplyTransformation(CompilationUnitSyntax root)
        {
            if (root.Members.Any())
            {
                MemberDeclarationSyntax methodSyntax = (MemberDeclarationSyntax)root.Members.First();
                var tryNodes = methodSyntax.DescendantNodes().OfType<TryStatementSyntax>().ToList();
                var methodCalls = methodSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
                if (tryNodes.Count == 0 || methodCalls.Count > 0)
                {
                    var tcNodes = methodSyntax.DescendantNodes().OfType<StatementSyntax>()
                        .Where(node => IsTryCatchApplicable(node)).ToList();
                    if (tcNodes.Count > 0)
                    {
                        int place = new Random().Next(1, tcNodes.Count);
                        StatementSyntax tryStr = GetTryCatch(tcNodes.ElementAt(place));
                        root = root.ReplaceNode(tcNodes.ElementAt(place), tryStr);
                    }
                }
            }
            return root;
        }

        private StatementSyntax GetTryCatch(StatementSyntax stmt)
        {
            string tryStr = "try \n" +
                    "{ \n" +
                    stmt + "\n" +
                    "} \n" +
                    "catch (Exception ex) \n" +
                    "{ \n" +
                    "Console.WriteLine(ex.ToString()); \n" +
                    "} \n";
            return SyntaxFactory.ParseStatement(tryStr);
        }

        private bool IsTryCatchApplicable(StatementSyntax node)
        {
            var methodCalls = node.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
            bool isPermeableStatement = !(
                node.IsKind(SyntaxKind.EmptyStatement) ||
                node.IsKind(SyntaxKind.Block) ||
                node.IsKind(SyntaxKind.LocalDeclarationStatement) ||
                node.IsKind(SyntaxKind.GotoStatement) ||
                node.IsKind(SyntaxKind.LabeledStatement) ||
                node.IsKind(SyntaxKind.BreakStatement) ||
                node.IsKind(SyntaxKind.ContinueStatement) ||
                node.IsKind(SyntaxKind.ReturnStatement)
            );
            return methodCalls.Count > 0 && isPermeableStatement;
        }
    }
}
