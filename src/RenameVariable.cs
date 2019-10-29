﻿using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class RenameVariable
    {
        public RenameVariable() 
        {
            //Console.WriteLine("\n[ RenameVariable ]\n");
        }

        public void InspectSourceCode(String csFile)
        {
            Common.SetOutputPath(this, csFile);
            CompilationUnitSyntax root = Common.GetParseUnit(csFile);
            if (root != null)
            {
                var locateVariables = new LocateVariables();
                locateVariables.Visit(root);
                HashSet<SyntaxToken> mVariableNodes = locateVariables.GetVariableList();
                int variableId = 0;
                foreach (var oldVariable in mVariableNodes)
                {
                    String oldVariablename = oldVariable.ToString();
                    String newVariablename = @"var" + variableId;
                    var variableRenaming = new ApplyVariableRenaming(oldVariablename, newVariablename);
                    root = (CompilationUnitSyntax)variableRenaming.Visit(root);
                    variableId++;
                }
                Common.SaveTransformation(root, csFile);
            }
        }
    }

    public class LocateVariables : CSharpSyntaxWalker
    {
        private HashSet<SyntaxToken> mVariableNodes;

        public HashSet<SyntaxToken> GetVariableList()
        {
            return mVariableNodes;
        }

        public LocateVariables() : base(SyntaxWalkerDepth.Token)
        {
            mVariableNodes = new HashSet<SyntaxToken>();
        }

        public override void Visit(SyntaxNode node)
        {
            base.Visit(node);
        }

        public override void VisitToken(SyntaxToken token)
        {
            if (token.IsKind(SyntaxKind.IdentifierToken)
                && (token.Parent.IsKind(SyntaxKind.Parameter)
                || token.Parent.IsKind(SyntaxKind.VariableDeclarator)))
            {
                mVariableNodes.Add(token);

            }
            base.VisitToken(token);
        }
    }

    public class ApplyVariableRenaming : CSharpSyntaxRewriter
    {
        private String mNewVariableName, mOldVariableName;
        public ApplyVariableRenaming(String oldVariableName, String newVariableName)
        {
            mNewVariableName = newVariableName;
            mOldVariableName = oldVariableName;
        }
        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            if (token.IsKind(SyntaxKind.IdentifierToken) && token.ToString().Equals(mOldVariableName))
            {
                var retVal = SyntaxFactory.Identifier(mNewVariableName);
                return retVal;
            }
            return base.VisitToken(token);
        }
    }
}
