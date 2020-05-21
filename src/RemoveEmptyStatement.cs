using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class RemoveEmptyStatement
    {
        public RemoveEmptyStatement()
        {
            //Console.WriteLine("\n[ RemoveEmptyStatement ]\n");
        }

        public void InspectSourceCode(String csFile)
        {
            Common.SetOutputPath(this, csFile);
            CompilationUnitSyntax root = Common.GetParseUnit(csFile);
            if (root != null)
            {
                var modRoot = (CompilationUnitSyntax)new EmptyStatementRemoval().Visit(root);
                Common.SaveTransformation(modRoot, csFile, Convert.ToString(1));
            }
        }

        public class EmptyStatementRemoval : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitEmptyStatement(EmptyStatementSyntax node)
            {
                return null;
            }
        }
    }
}
