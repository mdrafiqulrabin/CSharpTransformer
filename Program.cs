using System;
using CSharpTransformer.src;

namespace CSharpTransformer
{
    class MainClass
    {
        /* root folder for input  -> '~/methods'
         * root folder for output -> '~/transforms'
         *
         * extracted single method of project should be in 'methods' folder
         * separate folder for each refactoring will be created in 'transforms' folder
         */

        static String inpPath = "/Users/mdrafiqulrabin/Desktop/RA/TNPA/CSharpAnalysis/DummyData/methods/";
        static String outPath = "/Users/mdrafiqulrabin/Desktop/RA/TNPA/CSharpAnalysis/DummyData/transforms/";

        public static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                inpPath = args[0];
                outPath = args[1];
            }
            new ASTExplorer(inpPath, outPath).Call();
        }
    }
}
