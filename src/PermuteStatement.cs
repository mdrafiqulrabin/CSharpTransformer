using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CSharpTransformer.src
{
    public class PermuteStatement
    {
        public PermuteStatement()
        {
            Console.WriteLine("\n[ PermuteStatement ]\n");
        }

        public void InspectSourceCode()
        {
            var rootDir = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            var path = Path.Combine(rootDir, "data/original/");
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                Console.WriteLine("File = " + Path.GetFileName(file));
                var txtCode = File.ReadAllText(file);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(txtCode);
                var root = (CompilationUnitSyntax)tree.GetRoot();
                Console.WriteLine("Original = \n" + root + "\n");

                root = ApplyPermuteStatement(root);
                root = (CompilationUnitSyntax)Formatter.Format(root, new AdhocWorkspace());
                Console.WriteLine("Transformed = \n" + root + "\n");
            }
        }

        public CompilationUnitSyntax ApplyPermuteStatement(CompilationUnitSyntax root)
        {
            var statements = root.DescendantNodes().OfType<StatementSyntax>().ToList();
            for (int i = 0, j = 1; i < statements.Count - 1; i++, j++)
            {
                if (statements[i].Parent == statements[j].Parent)
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
                    }
                }
            }
            return root;
        }
    }
}

