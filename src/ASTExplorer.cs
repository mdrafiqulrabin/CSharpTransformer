using System;
using System.IO;

namespace CSharpTransformer.src
{
    public class ASTExplorer
    {
        public ASTExplorer(String inpPath, String outPath)
        {
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
            InspectDataset();
        }

        private void InspectDataset()
        {
            String input_dir = Common.mRootInputPath;
            //TODO: parallel transformation
            foreach (string csFile in Directory.EnumerateFiles(input_dir, "*.cs",
                SearchOption.AllDirectories))
            {
                try
                {
                    new VariableRenaming().InspectSourceCode(csFile);
                    new BooleanExchange().InspectSourceCode(csFile);
                    new LoopExchange().InspectSourceCode(csFile);
                    new SwitchToIf().InspectSourceCode(csFile);
                    new PermuteStatement().InspectSourceCode(csFile);
                    new ReorderCondition().InspectSourceCode(csFile);
                    new UnusedStatement().InspectSourceCode(csFile);
                    new LogStatement().InspectSourceCode(csFile);
                    new TryCatch().InspectSourceCode(csFile);
                    new RemoveComment().InspectSourceCode(csFile);
                    new RemoveEmptyStatement().InspectSourceCode(csFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + csFile);
                    Console.WriteLine(ex);
                }
            }

        }
    }
}
