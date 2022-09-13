using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CSharpTransformer.src
{
    public sealed class Common
    {
        public static String mRootInputPath = "";
        public static String mRootOutputPath = "";

        private String ReplaceFirst(String txtStr,
            String searchStr, String targetStr)
        {
            var regex = new Regex(Regex.Escape(searchStr));
            return regex.Replace(txtStr, targetStr, 1);
        }

        private String RemoveSpaces(String txtStr)
        {
            return Regex.Replace(txtStr, @"^\s*$\n",
                String.Empty, RegexOptions.Multiline)
                     .TrimEnd();
        }

        private class CommentRemoval : CSharpSyntaxRewriter
        {
            public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                    || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    return default(SyntaxTrivia);
                }
                return base.VisitTrivia(trivia);
            }
        }

        private SyntaxTree GetSyntaxTree(String csFile)
        {
            var txtCode = File.ReadAllText(csFile);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(txtCode);
            return tree;
        }

        public CompilationUnitSyntax GetParseUnit(String csFile)
        {
            CompilationUnitSyntax root = null;
            try
            {
                SyntaxTree tree = this.GetSyntaxTree(csFile);
                root = (CompilationUnitSyntax)tree.GetRoot();
                root = (CompilationUnitSyntax)new CommentRemoval().Visit(root);
            }
            catch (Exception) { }
            return root;
        }

        private bool CheckTransformation(CompilationUnitSyntax modRoot, string csFile)
        {
            if (modRoot == null) return false;
            CompilationUnitSyntax orgRoot = this.GetParseUnit(csFile);
            String orgTxt = this.RemoveSpaces(orgRoot.ToString());
            String traTxt = this.RemoveSpaces(modRoot.ToString());
            if (orgTxt.Equals(traTxt))
            {
                return false;
            }
            return true;
        }

        public void SaveTransformation(String savePath, CompilationUnitSyntax root,
            string csFile, string place = "")
        {
            if (this.CheckTransformation(root, csFile))
            {
                String output_dir = savePath + this.ReplaceFirst(csFile,
                    Common.mRootInputPath, "");
                if (place.Length > 0)
                {
                    output_dir = output_dir.Substring(0, output_dir.LastIndexOf(".cs",
                        StringComparison.Ordinal)) + "_" + place + ".cs";
                }
                root = (CompilationUnitSyntax)Formatter.Format(root, new AdhocWorkspace());
                root = (CompilationUnitSyntax)CSharpSyntaxTree.ParseText(root.ToString()).GetRoot();
                this.WriteSourceCode(root, output_dir);
            }
        }

        private void WriteSourceCode(CompilationUnitSyntax root,
            String codePath)
        {
            try
            {
                new FileInfo(codePath).Directory.Create();
                File.WriteAllText(codePath, root.ToString());
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
