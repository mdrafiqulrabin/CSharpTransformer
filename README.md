# CSharpTransformer
A tool to apply program transformations on CSharp **(\*.cs)** methods for generating semantic adversarial input programs.
- - -

# Version:
- TargetFrameworkVersion v4.7
- ToolsVersion 4.0
- Microsoft.Net.Compilers 3.1.1
- Microsoft.CodeAnalysis 3.1.0

# CSharpTransformer.csproj:

- Given input and output path, execute csproj:
  ```
  # input_path  = Input directory to the original programs.
  # output_path = Output directory to the augmented programs.
  $ dotnet run --project=CSharpTransformer/CSharpTransformer.csproj "input_path" "output_path"
  ```

## Transformations:

- BooleanExchange
- LogStatement
- LoopExchange
- PermuteStatement
- ReorderCondition
- SwitchToIf
- TryCatch
- UnusedStatement
- VariableRenaming

# References:

- Testing Neural Program Analyzers [[Paper](https://arxiv.org/abs/1908.10711)] [[GitHub](https://github.com/mdrafiqulrabin/tnpa-framework)]
- On the generalizability of Neural Program Models with respect to semantic-preserving program transformations [[Paper](https://arxiv.org/abs/2008.01566)] [[GitHub](https://github.com/mdrafiqulrabin/tnpa-generalizability)]
- Roslyn: https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/
