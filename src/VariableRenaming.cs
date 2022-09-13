using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class VariableRenaming
    {
        private readonly Common mCommon;

        public VariableRenaming()
        {
            //Console.WriteLine("\n[ VariableRenaming ]\n");
            mCommon = new Common();
        }

        public void InspectSourceCode(String csFile)
        {
            String savePath = Common.mRootOutputPath + this.GetType().Name + "/";
            CompilationUnitSyntax root = mCommon.GetParseUnit(csFile);
            if (root != null)
            {
                var variableNames = root.DescendantNodes().OfType<ParameterSyntax>().Select(p => p.Identifier.Text)
                    .Concat(root.DescendantNodes().OfType<VariableDeclaratorSyntax>().Select(v => v.Identifier.Text))
                    .ToArray();
                if (variableNames.Count() > 0)
                {
                    String newVariablename = @"var";
                    int variableId = 0;
                    while (variableNames.Contains(newVariablename))
                    {
                        variableId++;
                        newVariablename = @"var" + variableId;
                    }
                    ApplyToPlace(savePath, csFile, root, variableNames, newVariablename);
                }
            }
        }

        private void ApplyToPlace(String savePath, String csFile,
            CompilationUnitSyntax orgRoot, string[] variableNames,
            String newVariablename)
        {
            int programId = 0;
            CompilationUnitSyntax modRoot = orgRoot;
            foreach (var oldVariablename in variableNames)
            {
                var variableRenaming = new ApplyVariableRenaming(oldVariablename, newVariablename);
                modRoot = mCommon.GetParseUnit(csFile);
                modRoot = (CompilationUnitSyntax)variableRenaming.Visit(modRoot);
                programId++;
                mCommon.SaveTransformation(savePath, modRoot, csFile, Convert.ToString(programId));
            }
        }

        private class ApplyVariableRenaming : CSharpSyntaxRewriter
        {
            private readonly string mNewVariableName, mOldVariableName;
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
