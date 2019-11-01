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
                // apply to single place
                var stmtNodes = root.DescendantNodes().OfType<StatementSyntax>().ToList();
                for (int place = 0; place < stmtNodes.Count - 1; place++)
                {
                    var modRoot = ApplyToOne(Common.GetParseUnit(csFile), place);
                    Common.SaveTransformation(modRoot, csFile, Convert.ToString(place + 1));
                }

                // apply to all place 
                root = ApplyToAll(root);
                Common.SaveTransformation(root, csFile, Convert.ToString(0));
            }
        }

        public CompilationUnitSyntax ApplyToOne(CompilationUnitSyntax root, int p)
        {
            var statements = root.DescendantNodes().OfType<StatementSyntax>().ToList();
            if (PermeableStatement(statements[p], statements[p+1]))
            {
                root = root.ReplaceNodes(new[] { statements[p], statements[p+1] },
                    (original, _) => original == statements[p] ? statements[p+1] : statements[p]);
            }
            return root;
        }

        public CompilationUnitSyntax ApplyToAll(CompilationUnitSyntax root)
        {
            var statements = root.DescendantNodes().OfType<StatementSyntax>().ToList();
            for (int i = 0, j = 1; i < statements.Count - 1; i++, j++)
            {
                if (PermeableStatement(statements[i], statements[j]))
                {
                    root = root.ReplaceNodes(new[] { statements[i], statements[j] },
                        (original, _) => original == statements[i] ? statements[j] : statements[i]);
                    statements = root.DescendantNodes().OfType<StatementSyntax>().ToList();
                }
            }
            return root;
        }

        private bool PermeableStatement(StatementSyntax stmti, StatementSyntax stmtj)
        {
            if (stmti.Parent == stmtj.Parent
                && !(NotPermeableStatement(stmti) || NotPermeableStatement(stmtj)))
            {
                var iIdentifiers = stmti.DescendantTokens().Where(x => x.IsKind(SyntaxKind.IdentifierToken)).Select(x => x.ToString()).ToList();
                var jIdentifiers = stmtj.DescendantTokens().Where(x => x.IsKind(SyntaxKind.IdentifierToken)).Select(x => x.ToString()).ToList();
                if (!iIdentifiers.Intersect(jIdentifiers).Any() && !jIdentifiers.Intersect(iIdentifiers).Any())
                {
                    return true;
                }
            }
            return false;
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

