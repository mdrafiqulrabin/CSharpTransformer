using System;
using System.IO;

namespace CSharpTransformer.src
{
    public class ASTExplorer
    {
        public ASTExplorer() { }
        public void Call()
        {
            var rootDir = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            var path = Path.Combine(rootDir, "data/original/");
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                Console.WriteLine("\n");
                Console.WriteLine("File = " + Path.GetFileName(file));
                var txtCode = File.ReadAllText(file);
                Console.WriteLine("Code = " + txtCode);
            }
        }
    }
}
