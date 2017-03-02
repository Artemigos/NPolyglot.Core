# NPolyglot Core [![NuGet](https://img.shields.io/nuget/v/NPolyglot.Core.svg)](https://www.nuget.org/packages/NPolyglot.Core/) [![Build status](https://ci.appveyor.com/api/projects/status/yrbpvai1216clur3?svg=true)](https://ci.appveyor.com/project/Artemigos/npolyglot-core)
Infastructure for NPolyglot - a tool that allows you to create and use compile time custom DSLs in your project.

## Installation

`Install-Package NPolyglot.Core -Pre`

**NOTE:** Visual Studio caches the list of available build actions so it might be necessary to unload the project and load it again.

## General concept

NPolyglot is a comile time tool, which lets you create and use DSL languages and still end up with statically compiled code in your assemblies.
This is achieved by extending the build process with custom steps. The steps are executed before actual compilation:

* preprocess DSL code file - read directives, read actual content, make sure that the data is prepared for next steps
* parse DSL code - read the text and turn it into object representation of the information that's contained there
* transform data into code - turn the object returned from the previous step into actual C# code

After that, when compilation is started the code is already prepared to be compiled.

## Coded languages

By default the package comes with the capability to use languages coded in C# in the project that will be using them.
While possible, it is recommended to use the precompiled approach, because it gets better autocompletion support in editors.
Also - more importantly - precompiled languages can easily use pretty much any libraries and tools available for development of the language,
whereas coded languages require for the tool to have adapters for NPolyglot (which might or might not be trivial to develop).

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

Instead of compiling the parsers and transforms right before using them it's possible to use ready-made languages in a form of a DLL.
Such language behaves exactly the same as a coded language and, in fact, it IS a coded language. The differece is that the project that uses it
no longer needs to run additional compilation steps.

### Using precompiled languages

Unfortunetely referencing precompiled languages involves modifying the `.csproj` file (at least until someone decides to make a VS extension for that).
To reference the language use:
```xml
<ItemGroup>
  <DslLanguageReference Include="path\to\the\language.dll" />
</ItemGroup>
```

That is enough to use the parsers and transforms contained in it.

Some language DLLs could ship with a `.targets` file that defines that reference (and probably some other things) so instead of referencing it
explicitly it would be enough to import the file at the end of your project:

```xml
<Import Project="path\to\the\language.targets" />
```

### Creating precompiled languages

To create a precompiled language:

* create new class library project
* install NPolyglot.LanguageDesign from NuGet: `Install-Package NPolyglot.LanguageDesign -Pre`
* create a class for the parser and have it implement `NPolyglot.LanguageDesign.ICodedParser` like you would for the coded parser
* create a class for the transform and have it implement `NPolyglot.LanguageDesign.ICodedTransform` like you would for the coded transform
* do NOT change any build actions - let the project compile normally

After the project is built ship the resulting assembly and all of its dependencies wherever needed.

It is also possible to have a precompiled language project in your application's solution. After the project with the language is ready just add a language reference
like you would with other precompiled languages (previous section). The setup should look like this:

* MyApp.MyLang project
  * has NPolyglot.LanguageDesign installed
  * has parsers and transforms
* MyApp project
  * has NPolyglot.Core installed
  * has language reference set up using `<DslLanguageReference ...` in `.csproj`
  * is set to depend on MyApp.MyLang (to ensure proper build order)

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
