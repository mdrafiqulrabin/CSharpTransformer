using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CSharpTransformer.src
{
    public class RenameVariable
    {
        public RenameVariable() 
        {
            Console.WriteLine("\n[ RenameVariable ]\n");
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
                root = (CompilationUnitSyntax)Formatter.Format(root, new AdhocWorkspace());
                Console.WriteLine("Transformed = \n" + root + "\n");
                Console.WriteLine("\n");
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
