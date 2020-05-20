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
                int programId = 0;
                var basicBlockStmts = LocateBasicBlockStatements(root);
                for (int k = 0; k < basicBlockStmts.Count; k++)
                {
                    var stmtNodes = basicBlockStmts.ElementAt(k);
                    for (int i = 0; i < stmtNodes.Count - 1; i++)
                    {
                        for (int j = i + 1; j < stmtNodes.Count; j++)
                        {
                            var modRoot = applyTransformation(csFile, k, i, j);
                            if (modRoot != null)
                            {
                                programId++;
                                Common.SaveTransformation(modRoot, csFile, Convert.ToString(programId));
                            }
                        }
                    }
                }
            }
        }

        public List<List<StatementSyntax>> LocateBasicBlockStatements(CompilationUnitSyntax root)
        {
            var innerStatements = new List<StatementSyntax>();
            var basicBlockStmts = new List<List<StatementSyntax>>();
            var allStatements = root.DescendantNodes().OfType<StatementSyntax>().ToList();

            foreach(var stmt in allStatements)
            {
                if(stmt.IsKind(SyntaxKind.ExpressionStatement)
                    && stmt.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList().Count == 0
                    && IsPermuteApplicable(stmt))
                {
                    innerStatements.Add(stmt);
                } else
                {
                    if (innerStatements.Count > 1)
                    {
                        basicBlockStmts.Add(new List<StatementSyntax>(innerStatements));
                    }
                    innerStatements.Clear();
                }
            }

            return basicBlockStmts;
        }

        public CompilationUnitSyntax applyTransformation(String csFile, int k, int i, int j)
        {
            var root = Common.GetParseUnit(csFile);
            var basicBlockStmts = LocateBasicBlockStatements(root);
            var innerStatements = basicBlockStmts.ElementAt(k);
            if (PermeableStatement(innerStatements, i, j))
            {
                var modRoot = root.ReplaceNodes(new[] { innerStatements[i], innerStatements[j] },
                    (original, _) => original == innerStatements[i] ? innerStatements[j] : innerStatements[i]);
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

        private bool IsPermuteApplicable(StatementSyntax node)
        {
            return !(
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

