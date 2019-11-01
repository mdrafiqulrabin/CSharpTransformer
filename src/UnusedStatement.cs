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
                root = applyTransformation(root);
                Common.SaveTransformation(root, csFile, Convert.ToString(1));
            }
        }

        private CompilationUnitSyntax applyTransformation(CompilationUnitSyntax root)
        {
            MethodDeclarationSyntax methodSyntax = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList().First();
            if (methodSyntax != null)
            {
                BlockSyntax mbody = ((MethodDeclarationSyntax)methodSyntax).Body;
                if (mbody != null && mbody.Statements.Count > 0)
                {
                    SyntaxList<StatementSyntax> mstmt = mbody.Statements;
                    int place = new Random().Next(0, mstmt.Count + 1);
                    StatementSyntax unusedStr = (StatementSyntax) getUnusedStatement();
                    mstmt = mstmt.Insert(place, unusedStr);
                    mbody = mbody.WithStatements(mstmt);
                    return root.ReplaceNode(methodSyntax, methodSyntax.WithBody(mbody));
                }
            }
            return root;
        }

        private StatementSyntax getUnusedStatement()
        {
            String timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            String unusedStr = "String dummy_timestamp = \"" + timestamp + "\";\n";
            return SyntaxFactory.ParseStatement(unusedStr);
        }
    }
}
