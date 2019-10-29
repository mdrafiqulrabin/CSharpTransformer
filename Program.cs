using System;
using CSharpTransformer.src;

namespace CSharpTransformer
{
    class MainClass
    {
        /* root folder for input -> '~/methods'
         * root folder for output -> '~/transforms'
         *
         * extracted single method of project should be in 'methods' folder 
         * seperate folder for each refactoring will be cretaed in 'transforms' folder
         */

        static readonly String inpPath = "/Users/mdrafiqulrabin/Projects/TNPA/dataset/csharp/methods/dummy";
        static readonly String outPath = "/Users/mdrafiqulrabin/Projects/TNPA/dataset/csharp/transforms/dummy";

        public static void Main(string[] args)
        {
            new ASTExplorer(inpPath, outPath).Call();
        }
    }
}
