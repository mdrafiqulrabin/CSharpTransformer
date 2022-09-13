using System;
using System.IO;
using System.Threading.Tasks;
using CSharpTransformer.src;
using static System.Net.WebRequestMethods;

namespace CSharpTransformer
{
    class MainClass
    {
        /* arg[0]: root path for input folder, ie, ~/data/methods/
         * arg[1]: root path for output folder, ie, ~/data/transforms/
         *
         * extracted single method of project should be in 'methods' folder
         * separate folder for each refactoring will be created in 'transforms' folder
         */

        public static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                String inputPath = args[0];
                if (!inputPath.EndsWith("/", StringComparison.Ordinal))
                {
                    inputPath += "/";
                }
                Common.mRootInputPath = inputPath;

                String outputPath = args[1];
                if (!outputPath.EndsWith("/", StringComparison.Ordinal))
                {
                    outputPath += "/";
                }
                Common.mRootOutputPath = outputPath;

                inspectDataset();
            } else
            {
                String msg = "Error (args):" +
                        "\n\targ[0]: root path for input folder" +
                        "\n\targ[1]: root path for output folder";
                Console.WriteLine(msg);
            }
        }

        private static void inspectDataset()
        {
            String input_dir = Common.mRootInputPath;
            Parallel.ForEach(Directory.EnumerateFiles(input_dir, "*.cs",
                SearchOption.AllDirectories), csFile =>
            {
                try
                {
                    new VariableRenaming().InspectSourceCode(csFile);
                    new BooleanExchange().InspectSourceCode(csFile);
                    new LoopExchange().InspectSourceCode(csFile);
                    new SwitchToIf().InspectSourceCode(csFile);
                    new ReorderCondition().InspectSourceCode(csFile);
                    new PermuteStatement().InspectSourceCode(csFile);
                    new UnusedStatement().InspectSourceCode(csFile);
                    new LogStatement().InspectSourceCode(csFile);
                    new TryCatch().InspectSourceCode(csFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + csFile);
                    Console.WriteLine(ex.ToString());
                }
            });
        }
    }
}
