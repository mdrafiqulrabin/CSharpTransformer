using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class PermuteStatement
    {
        public PermuteStatement()
        {
            //Console.WriteLine("\n[ PermuteStatement ]\n");
        }

        public void InspectSourceCode(String csFile)
        {
            Common.SetOutputPath(this, csFile);
            CompilationUnitSyntax root = Common.GetParseUnit(csFile);
            if (root != null)
            {
                root = ApplyPermuteStatement(root);
                Common.SaveTransformation(root, csFile);
            }
        }

        public CompilationUnitSyntax ApplyPermuteStatement(CompilationUnitSyntax root)
        {
            var statements = root.DescendantNodes().OfType<StatementSyntax>().ToList();
            for (int i = 0, j = 1; i < statements.Count - 1; i++, j++)
            {
                if (statements[i].Parent == statements[j].Parent
                    && !(NotPermeableStatement(statements[i]) || NotPermeableStatement(statements[j])))
                {
                    var iIdentifiers = statements[i].DescendantTokens().Where(x => x.IsKind(SyntaxKind.IdentifierToken)).Select(x => x.ToString()).ToList();
                    var jIdentifiers = statements[j].DescendantTokens().Where(x => x.IsKind(SyntaxKind.IdentifierToken)).Select(x => x.ToString()).ToList();
                    if (!iIdentifiers.Intersect(jIdentifiers).Any() && !jIdentifiers.Intersect(iIdentifiers).Any())
                    {
                        /*StatementSyntax tmpSt = SyntaxFactory.ParseStatement("int mrabin = 0;");
                        root = root.ReplaceNode(statements[i], tmpSt);
                        root = root.ReplaceNode(statements[j], iStatement);
                        root = root.ReplaceNode(tmpSt, jStatement);*/

                        root = root.ReplaceNodes(new[] { statements[i], statements[j] },
                            (original, _) => original == statements[i] ? statements[j] : statements[i]);
                        statements = root.DescendantNodes().OfType<StatementSyntax>().ToList();
                        i++; j++;
                    }
                }
            }
            return root;
        }

        private bool NotPermeableStatement(StatementSyntax node)
        {
            return (node.IsKind(SyntaxKind.EmptyStatement)
                || node.IsKind(SyntaxKind.BreakKeyword)
                || node.IsKind(SyntaxKind.ContinueStatement)
                || node.IsKind(SyntaxKind.ReturnStatement));
        }
    }
}

