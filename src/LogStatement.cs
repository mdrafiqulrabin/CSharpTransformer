using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class LogStatement
    {
        public LogStatement()
        {
            //Console.WriteLine("\n[ LogStatement ]\n");
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
                    if (mstmt.ElementAt(0).ToString().Contains("Console.WriteLine")) return root;
                    StatementSyntax logStr = (StatementSyntax)GetLogStatement();
                    mstmt = mstmt.Insert(0, logStr); //beginning of stmt
                    mbody = mbody.WithStatements(mstmt);
                    return root.ReplaceNode(methodSyntax, methodSyntax.WithBody(mbody));
                }
            }
            return root;
        }

        private StatementSyntax GetLogStatement()
        {
            String logStr = "Console.WriteLine(\"Executing method:\");\n";
            return SyntaxFactory.ParseStatement(logStr);
        }
    }
}
