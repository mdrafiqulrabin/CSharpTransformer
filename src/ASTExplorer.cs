using System;
using System.IO;

namespace CSharpTransformer.src
{
    public class ASTExplorer
    {
        public ASTExplorer() { }
        public void Call()
        {
            new RenameVariable().InspectSourceCode();
        }
    }
}
