using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTransformer.src
{
    public class RemoveComment
    {
        public RemoveComment()
        {
            //Console.WriteLine("\n[ RemoveComment ]\n");
        }

        public void InspectSourceCode(String csFile)
        {
            Common.SetOutputPath(this, csFile);
            CompilationUnitSyntax root = Common.GetParseUnit(csFile);
            if (root != null)
            {
                var modRoot = (CompilationUnitSyntax) new CommentRemoval().Visit(root);
                Common.SaveTransformation(modRoot, csFile, Convert.ToString(1));
            }
        }

        public class CommentRemoval : CSharpSyntaxRewriter
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
    }
}
