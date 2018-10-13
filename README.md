# Gatherer
[![Build status](https://ci.appveyor.com/api/projects/status/8h5rc4542l0ehf8l?svg=true)](https://ci.appveyor.com/project/huffSamuel/gatherer)
[![Nuget](https://img.shields.io/nuget/v/Gatherer.svg)](https://www.nuget.org/packages/Gatherer/)  
Gatherer is a lightweight plugin loading framework for .NET Standard. Unopinionated and unbiased, this framework
simply loads the types in the folders you declare. It is up to you to inject these loaded types into the IoC
of your choice.

## Installation

*Package Manager*
```Install-Package Gatherer -Version 1.1.1```

*dotnet CLI*
```dotnet add package Gatherer --version 1.1.1```

## Example Usage

See the included Gatherer.Playground projects or insert this code snippet into your project

```c#
var gatherer = new Gatherer();
var harvested = gatherer.LoadAll();
```
Gatherer also offers a number of fluent methods for customizing the way Gatherer loads plugins.

## Change Log

### Version 1.1.1
+ Remove opinionated ReflectionTypeLoadException handling when loading types. Now defaults to logging the LoaderExceptions but can be overridden by calling SetTypeLoadExceptionHandler(Action<ReflectionTypeLoadException>).
+ Move const strings to resource file for localization.
+ Removed cruft.

### Version 1.1
+ Catch ReflectionTypeLoadExceptions when loading assemblies. No more manually handling.

### Version 1.0
+ Initial release ¯\_(ツ)_/¯

## Contributions
Contributions are always welcome! For minor changes simply submit a pull request. For major changes
or breaking changes please submit an issue first so we can discuss the proposed changes.

## License
This software is licensed under MIT.
