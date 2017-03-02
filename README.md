# NPolyglot Core [![NuGet](https://img.shields.io/nuget/v/NPolyglot.Core.svg)](https://www.nuget.org/packages/NPolyglot.Core/) [![Build status](https://ci.appveyor.com/api/projects/status/yrbpvai1216clur3?svg=true)](https://ci.appveyor.com/project/Artemigos/npolyglot-core)
Infastructure for NPolyglot - a tool that allows you to create and use custom DSLs in your project.

## Installation

`Install-Package NPolyglot.Core -Pre`

**NOTE:** Visual Studio caches the list of available build actions so it might be necessary to unload the project and load it again.

## General concept

TODO

## Coded languages

By default the package comes with the capability to use languages coded in C# in the project that will be using them.
While possible, it is recommended to use the precompiled approach, because it gets better autocompletion support in editors.

### Coded parsers

Coded parser is a class in your project which will handle parsing the text input to an object representation of the data contained in it.

To create a coded parser:

* create a class
* set the file's build action to `CodedParser`
* make sure the class implements `NPolyglot.LanguageDesign.ICodedParser` (no need to worry about a reference - it will be provided)

Implementing the `ICodedParser` interface involves:

* `ExportName` property - name used to reference the parser
* `Parse` method - the method invoked when DSL code needs to be parsed

A sample implementation could look like this:

```csharp
namespace MyLang
{
    public class MyParser : NPolyglot.LanguageDesign.ICodedParser
    {
        public string ExportName => "MyParser";
        public object Parse(string input) => input.Trim().Split(",");
    }
}
```

Coded parsers are **NOT** included in your application's assembly - they are a construct that's used exclusively during compile time.

### Coded transforms

Coded transforms are very similar to coded parsers and also aren't included in your application's assembly.

How to create:

* create a class
* set it's build action to `CodedTransform`
* implement `NPolyglot.LanguageDesign.ICodedTransform`

Implementation elements:

* `ExportName` property - name used to reference the transform
* `Transform` method - will be executed to transform an object to C# code that will be included when compiling your project

Sample:

```csharp
namespace MyLang
{
    public class MyTransform : NPolyglot.LanguageDesign.ICodedTransform
    {
        public string ExportName => "MyTransform";
        public string Transform(object data)
        {
            var typedData = data.ToString();
            return
@"namespace MyProj.Data
{
    public class DataHolder
    {
        public string[] Data => new[] { """ + typedData + @""" };
    }
}"
        }
    }
}
```

### References

It is possible to add DLL references for coded parsers and transforms. To do that it's needed to edit your project's `.csproj` file.

To add a reference for parsers add:
```xml
<ItemGroup>
  <CodedParsersReference Include="path\to\the\reference.dll" />
</ItemGroup>
```

To add a reference for transforms add:
```xml
<ItemGroup>
  <CodedTransformsReference Include="path\to\the\reference.dll" />
</ItemGroup>
```

## Precompiled languages

TODO

## DSL code files

To use your DSL you'll need a DSL code file:

* create a text file
* set it's build action to `DslCode`
* set which parser an transform to use
* write code :)

There are 2 ways to configure which parser and transform should be used for a DSL code file:

* add `@parser ParserName` and `@template TemplateName` directives at the top of the `DslCode` file, one directive per line
* edit `.csproj` to add them as item metadata:

  ```xml
  <DslCode Include="your_file.txt">
    <Parser>ParserName</Parser>
    <Template>TemplateName</Template>
  </DslCode>
  ```

## External parsing tools/transform engines

TODO
