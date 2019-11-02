using System;
using System.IO;

namespace CSharpTransformer.src
{
    public class ASTExplorer
    {
        public ASTExplorer(String inpPath, String outPath) {
            if (!inpPath.EndsWith("/", StringComparison.Ordinal))
            {
                inpPath += "/";
            }
            Common.mRootInputPath = inpPath;

            if (!outPath.EndsWith("/", StringComparison.Ordinal))
            {
                outPath += "/";
            }
            Common.mRootOutputPath = outPath;
        }

        public void Call()
        {
            inspectDataset();
        }

        private void inspectDataset()
        {
            String input_dir = Common.mRootInputPath;
            foreach (string csFile in Directory.EnumerateFiles(input_dir, "*.cs",
                SearchOption.AllDirectories))
            {
                try
                {
                    new RenameVariable().InspectSourceCode(csFile);
                    new BooleanExchange().InspectSourceCode(csFile);
                    new LoopExchange().InspectSourceCode(csFile);
                    new SwitchConditional().InspectSourceCode(csFile);
                    new PermuteStatement().InspectSourceCode(csFile);
                    new UnusedStatement().InspectSourceCode(csFile);
                    new UnreachableStatement().InspectSourceCode(csFile);
                    new TryCatch().InspectSourceCode(csFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

        }
    }
}
