# CSharpTransformer
A tool to apply program transformations to CSharp methods, that generates new adversarial programs by inducing small (semantic-preserving) changes to original input programs.
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
