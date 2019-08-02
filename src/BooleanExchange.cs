using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CSharpTransformer.src
{
    public class BooleanExchange
    {
        public BooleanExchange()
        {
            Console.WriteLine("\n[ BooleanExchange ]\n");
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

                SemanticModel semanticModel = CSharpCompilation.Create("BooleanExchange")
                               .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                               .AddSyntaxTrees(tree)
                               .GetSemanticModel(tree);
                var booleanExchange = new ApplyBooleanExchange(semanticModel);
                root = (CompilationUnitSyntax)booleanExchange.Visit(root);
                root = (CompilationUnitSyntax)Formatter.Format(root, new AdhocWorkspace());
                Console.WriteLine("Transformed = \n" + root + "\n");
            }
        }

        public class ApplyBooleanExchange : CSharpSyntaxRewriter
        {
            private SemanticModel semanticModel = null;
            public ApplyBooleanExchange(SemanticModel model)
            {
                semanticModel = model;
            }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (
                    node != null && node.Parent != null &&
                    (node.IsKind(SyntaxKind.TrueLiteralExpression) || node.IsKind(SyntaxKind.FalseLiteralExpression)) &&
                    ((node.Parent.Parent != null && node.Parent.Parent.IsKind(SyntaxKind.VariableDeclarator)) || node.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression) || node.Parent.IsKind(SyntaxKind.EqualsExpression))
                    )
                {
                    if (node.IsKind(SyntaxKind.TrueLiteralExpression)) {
                        return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
                    } else if (node.IsKind(SyntaxKind.FalseLiteralExpression)) { 
                        return SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
                    }
                } else if (
                    node != null && node.Parent != null &&
                    node.IsKind(SyntaxKind.IdentifierName) &&
                    semanticModel.GetTypeInfo(node).Type.ToString().Equals("boolean") &&
                    !(node.Parent.IsKind(SyntaxKind.VariableDeclaration) || node.Parent.IsKind(SyntaxKind.Parameter) || node.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression))
                    )
                {
                    /*if (node.Parent != null && node.Parent.IsKind(SyntaxKind.LogicalNotExpression))
                    {
                        // TODO: ReplaceNode (!x to x) -> node.Parent by node.
                    }*/
                    return SyntaxFactory.ParseExpression("!" + node);
                }

                return base.Visit(node);
            }
        }
    }
}


