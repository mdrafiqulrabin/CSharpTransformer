using System;
using System.Linq;
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
                var variableNames = root.DescendantNodes().OfType<ParameterSyntax>().Select(p => p.Identifier.Text)
                    .Concat(root.DescendantNodes().OfType<VariableDeclaratorSyntax>().Select(v => v.Identifier.Text))
                    .ToArray();
                if (variableNames.Count() > 1)
                {
                    ApplyToPlace(csFile, root, variableNames, false);
                }
                ApplyToPlace(csFile, root, variableNames, true);
            }
        }

        public void ApplyToPlace(String csFile, CompilationUnitSyntax orgRoot,
            string[] variableNames, bool singlePlace)
        {
            int variableId = 0, programId = 0;
            while (variableNames.Contains(@"var" + variableId))
            {
                variableId++;
            }
            CompilationUnitSyntax modRoot = orgRoot;
            foreach (var oldVariablename in variableNames)
            {
                String newVariablename = @"var" + variableId;
                var variableRenaming = new ApplyVariableRenaming(oldVariablename, newVariablename);
                if (singlePlace)
                {
                    modRoot = Common.GetParseUnit(csFile);
                }
                modRoot = (CompilationUnitSyntax)variableRenaming.Visit(modRoot);
                programId++;
                if (singlePlace)
                {
                    Common.SaveTransformation(modRoot, csFile, Convert.ToString(programId));
                } else
                {
                    variableId++;
                }
            }
            if (!singlePlace)
            {
                Common.SaveTransformation(modRoot, csFile, Convert.ToString(0));
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
}
