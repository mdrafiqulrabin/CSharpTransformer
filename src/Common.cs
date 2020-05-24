using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CSharpTransformer.src
{
    public static class Common
    {
        public static String mRootInputPath = "";
        public static String mRootOutputPath = "";
        public static String mSavePath = "";

        public static String ReplaceFirst(String txtStr,
            String searchStr, String targetStr)
        {
            var regex = new Regex(Regex.Escape(searchStr));
            return regex.Replace(txtStr, targetStr, 1);
        }

        public static String RemoveSpaces(String txtStr)
        {
            return Regex.Replace(txtStr, @"^\s*$\n",
                String.Empty, RegexOptions.Multiline)
                     .TrimEnd();
        }

        public static void SetOutputPath(Object obj, String csFile)
        {
            //assume '/transforms' in output path
            Common.mSavePath = Common.ReplaceFirst(Common.mRootOutputPath,
                "/transforms", "/transforms/" + obj.GetType().Name);
        }

        public static SyntaxTree GetSyntaxTree(String csFile)
        {
            var txtCode = File.ReadAllText(csFile);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(txtCode);
            return tree;
        }

        public static CompilationUnitSyntax GetParseUnit(String csFile)
        {
            CompilationUnitSyntax root = null;
            try
            {
                SyntaxTree tree = Common.GetSyntaxTree(csFile);
                root = (CompilationUnitSyntax)tree.GetRoot();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + csFile);
                String error_dir = Common.mSavePath + "cs_parser_error.txt";
                Common.SaveErrText(error_dir, csFile);
                Console.WriteLine(ex);
            }
            return root;
        }

        public static bool CheckTransformation(CompilationUnitSyntax modRoot, string csFile)
        {
            if (modRoot == null) return false;
            CompilationUnitSyntax orgRoot = Common.GetParseUnit(csFile);
            String orgTxt = Common.RemoveSpaces(orgRoot.ToString());
            String traTxt = Common.RemoveSpaces(modRoot.ToString());
            if (orgTxt.Equals(traTxt))
            {
                return false;
            }
            return true;
        }

        public static void SaveTransformation(CompilationUnitSyntax root,
            string csFile, string place = "")
        {
            if (Common.CheckTransformation(root, csFile))
            {
                String output_dir = Common.mSavePath + Common.ReplaceFirst(csFile,
                    Common.mRootInputPath, "");
                if (place.Length > 0)
                {
                    output_dir = output_dir.Substring(0, output_dir.LastIndexOf(".cs",
                        StringComparison.Ordinal)) + "_" + place + ".cs";
                }
                root = (CompilationUnitSyntax)Formatter.Format(root, new AdhocWorkspace());
                Common.WriteSourceCode(root, output_dir);
            }
        }

        public static void SaveErrText(String error_dir, String csFile)
        {
            try
            {
                new FileInfo(csFile).Directory.Create();
                File.AppendAllText(error_dir, csFile);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void WriteSourceCode(CompilationUnitSyntax root,
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

        public static SemanticModel GetSemanticModel(SyntaxTree tree, String name)
        {
            SemanticModel semanticModel = CSharpCompilation.Create(name)
               .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
               .AddSyntaxTrees(tree)
               .GetSemanticModel(tree);
            return semanticModel;
        }
    }
}
