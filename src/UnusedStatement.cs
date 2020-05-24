using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class UnusedStatement
    {
        public UnusedStatement()
        {
            //Console.WriteLine("\n[ UnusedStatement ]\n");
        }

        public void InspectSourceCode(String csFile)
        {
            Common.SetOutputPath(this, csFile);
            CompilationUnitSyntax root = Common.GetParseUnit(csFile);
            if (root != null)
            {
                root = ApplyTransformation(root);
                Common.SaveTransformation(root, csFile, Convert.ToString(1));
            }
        }

        private CompilationUnitSyntax ApplyTransformation(CompilationUnitSyntax root)
        {
            MethodDeclarationSyntax methodSyntax = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList().First();
            if (methodSyntax != null)
            {
                BlockSyntax mbody = ((MethodDeclarationSyntax)methodSyntax).Body;
                if (mbody != null && mbody.Statements.Count > 0)
                {
                    SyntaxList<StatementSyntax> mstmt = mbody.Statements;
                    int place = new Random().Next(0, mstmt.Count + 1);
                    StatementSyntax unusedStr = (StatementSyntax)GetUnusedStatement(root);
                    if (unusedStr == null) return null;
                    mstmt = mstmt.Insert(place, unusedStr);
                    mbody = mbody.WithStatements(mstmt);
                    return root.ReplaceNode(methodSyntax, methodSyntax.WithBody(mbody));
                }
            }
            return root;
        }

        private StatementSyntax GetUnusedStatement(CompilationUnitSyntax root)
        {
            var variableNames = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().Select(v => v.Identifier.Text)
                .Concat(root.DescendantNodes().OfType<ParameterSyntax>().Select(p => p.Identifier.Text)).ToArray();
            if (!variableNames.Contains(@"debug"))
            {
                String unusedStr = "bool debug = true;\n";
                return SyntaxFactory.ParseStatement(unusedStr);
            }
            return null;
        }
    }
}
