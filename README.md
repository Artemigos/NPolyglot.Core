# NPolyglot Core
Infastructure for NPolyglot - a tool that allows you to create custom DSLs processed entirely during compile time. Useless by itself - to start using it:

1. Install a parser package
   * [Sprache parser](https://github.com/Artemigos/NPolyglot.Parsers.Sprache)
1. Install a template engine package
   * [Handlebars engine](https://github.com/Artemigos/NPolyglot.Templating.Handlebars)
1. Write a DSL parser in a way required by the selected parser package
1. Write a code template in a way required by the selected templating package
1. *Finally* write DSL code in a file marked as `DslCode` in your project (set Build Action in Visual Studio file properties or edit csproj directly and include the file as `<DslCode Include="your_file.txt" />`)

   **NOTE:** Visual Studio caches the list of available build actions so it might be necessary to close the solution and open it again.

Also, be aware that the `DslCode` file might need to point to the parser and template that should be used to process it. This can be done in 2 ways:
* add `@parser ParserName` and `@template TemplateName` directives at the top of the `DslCode` file, one directive per line
* edit csproj to add them as item metadata:
  ```xml
  <DslCode Include="your_file.txt">
    <Parser>ParserName</Parser>
    <Template>TemplateName</Template>
  </DslCode>
  ```
