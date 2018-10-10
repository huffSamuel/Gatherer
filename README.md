# Gatherer
Gatherer is a lightweight plugin loading framework for .NET Core. Unopinionated and unbiased, this framework
simply loads the types in the folders you declare. It is up to you to inject these loaded types into the IoC
of your choice.

## Installation

Download the nuget package

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