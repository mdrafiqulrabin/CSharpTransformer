using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer
{
    public class RenameVariable
    {
        public RenameVariable() { }

        public void InspectSourceCode()
        {
            string path = @"/Users/mdrafiqulrabin/Projects/TNP/CSharpTransformer/CSharpTransformer/data/originalCode";
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                if (!file.Contains("sample.cs")) continue;
                Console.WriteLine("file name = " + Path.GetFileName(file));
                var sampleCode = File.ReadAllText(file);

                SyntaxTree tree = CSharpSyntaxTree.ParseText(sampleCode);
                var root = (CompilationUnitSyntax)tree.GetRoot();
                Console.WriteLine("Original = " + root);

                LocateVariables locateVariables = new LocateVariables();
                locateVariables.Visit(root);
                HashSet<SyntaxToken> mVariableList = locateVariables.GetVariableList();

                int variableId = 0;
                foreach (var oldVariable in mVariableList)
                {
                    String oldVariablename = oldVariable.ToString();
                    String newVariablename = @"var" + variableId;
                    var vr = new ApplyVariableRenaming(oldVariablename, newVariablename);
                    root = (CompilationUnitSyntax)vr.Visit(root);
                    variableId++;
                }

                Console.WriteLine("Transformed = " + root);
            }
        }
    }

    public class LocateVariables : CSharpSyntaxWalker
    {
        private HashSet<SyntaxToken> mVariableList;

        public HashSet<SyntaxToken> GetVariableList()
        {
            return mVariableList;
        }

        public LocateVariables() : base(SyntaxWalkerDepth.Token)
        {
            mVariableList = new HashSet<SyntaxToken>();
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
                mVariableList.Add(token);

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
