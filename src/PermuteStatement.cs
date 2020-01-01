using System;
using System.Collections.Generic;
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
                // apply to all pairs
                int cnt = 0;
                var stmtNodes = root.DescendantNodes().OfType<StatementSyntax>()
                    .Where(node => !IsNotPermeableStatement(node)).ToList();
                for (int stmti = 0; stmti < stmtNodes.Count - 1; stmti++)
                {
                    for (int stmtj = stmti+1; stmtj < stmtNodes.Count; stmtj++)
                    {
                        var modRoot = ApplyToAll(Common.GetParseUnit(csFile), stmti, stmtj);
                        if (modRoot != null)
                        {
                            Common.SaveTransformation(modRoot, csFile, Convert.ToString(++cnt));
                        }
                    }
                }
            }
        }

        public CompilationUnitSyntax ApplyToAll(CompilationUnitSyntax root, int i, int j)
        {
            var statements = root.DescendantNodes().OfType<StatementSyntax>()
                    .Where(node => !IsNotPermeableStatement(node)).ToList();
            if (PermeableStatement(statements, i, j))
            {
                var modRoot = root.ReplaceNodes(new[] { statements[i], statements[j] },
                    (original, _) => original == statements[i] ? statements[j] : statements[i]);
                return modRoot;
            }
            return null;
        }

        private bool PermeableStatement(List<StatementSyntax> statements, int i, int j)
        {
            var stmti = statements[i];
            var stmtj = statements[j];
            if (stmti.Parent == stmtj.Parent)
            {
                var iIdentifiers = stmti.DescendantTokens()
                    .Where(x => x.IsKind(SyntaxKind.IdentifierToken))
                    .Select(x => x.ToString()).ToList();
                var jIdentifiers = stmtj.DescendantTokens()
                    .Where(x => x.IsKind(SyntaxKind.IdentifierToken))
                    .Select(x => x.ToString()).ToList();
                if (!iIdentifiers.Intersect(jIdentifiers).Any()
                    && !jIdentifiers.Intersect(iIdentifiers).Any())
                {
                    for (int b = i + 1; b < j; b++)
                    {
                        var stmtb = statements[b];
                        var bIdentifiers = stmtb.DescendantTokens()
                            .Where(x => x.IsKind(SyntaxKind.IdentifierToken))
                            .Select(x => x.ToString()).ToList();
                        if (iIdentifiers.Intersect(bIdentifiers).Any()
                            || bIdentifiers.Intersect(iIdentifiers).Any()
                            || jIdentifiers.Intersect(bIdentifiers).Any()
                            || bIdentifiers.Intersect(jIdentifiers).Any())
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private bool IsNotPermeableStatement(StatementSyntax node)
        {
            return (
                node.IsKind(SyntaxKind.EmptyStatement) ||
                node.IsKind(SyntaxKind.GotoStatement) ||
                node.IsKind(SyntaxKind.LabeledStatement) ||
                node.IsKind(SyntaxKind.BreakStatement) ||
                node.IsKind(SyntaxKind.ContinueStatement) ||
                node.IsKind(SyntaxKind.ReturnStatement)
            );
        }
    }
}

