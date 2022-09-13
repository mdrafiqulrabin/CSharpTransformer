using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class UnusedStatement
    {
        private readonly Common mCommon;

        public UnusedStatement()
        {
            //Console.WriteLine("\n[ UnusedStatement ]\n");
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
                var stmtNodes = methodSyntax.DescendantNodes().OfType<StatementSyntax>()
                    .Where(node => !node.IsKind(SyntaxKind.Block)).ToList();
                if (stmtNodes.Count > 0)
                {
                    int place = new Random().Next(1, stmtNodes.Count);
                    StatementSyntax unusedStr = GetUnusedStatement(stmtNodes.ElementAt(place));
                    root = root.ReplaceNode(stmtNodes.ElementAt(place), unusedStr);
                }
            }
            return root;
        }

        private StatementSyntax GetUnusedStatement(StatementSyntax stmt)
        {
            String unusedStr = "if (false) { temp = 1; };\n" + stmt + "\n";
            return SyntaxFactory.ParseStatement(unusedStr);
        }
    }
}
