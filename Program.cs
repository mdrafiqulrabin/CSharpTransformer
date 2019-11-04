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

        static readonly String inpPath = "/Users/mdrafiqulrabin/Projects/TNPA/dataset/csharp/methods/";
        static readonly String outPath = "/Users/mdrafiqulrabin/Projects/TNPA/dataset/csharp/transforms/";

        public static void Main(string[] args)
        {
            new ASTExplorer(inpPath, outPath).Call();
        }
    }
}
