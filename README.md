# Gatherer
[![Build status](https://ci.appveyor.com/api/projects/status/8h5rc4542l0ehf8l?svg=true)](https://ci.appveyor.com/project/huffSamuel/gatherer)
[![Nuget](https://img.shields.io/nuget/v/Gatherer.svg)](https://www.nuget.org/packages/Gatherer/)  
---
Gatherer is a lightweight plugin loading framework for .NET Standard. Unopinionated and unbiased, this framework
simply loads the types in the folders you declare. It is up to you to inject these loaded types into the IoC
of your choice.

## Installation

*Package Manager*
```Install-Package Gatherer -Version 1.0.0```

*dotnet CLI*
```dotnet add package Gatherer --version 1.0.0```

## Example Usage

See the included Gatherer.Playground projects or insert this code snippet into your project

```
var gatherer = new Gatherer();
gatherer.LoadAll();
```
Gatherer also offers a number of fluent methods for customizing the way Gatherer loads plugins.

## Change Log
- Initial release

## Contributions
Contributions are always welcome! For minor changes simply submit a pull request. For major changes
or breaking changes please submit an issue first so we can discuss the proposed changes.

## License
This software is licensed under MIT.
