# Rhetos - A DSL platform

Rhetos is a DSL platform for Enterprise Application Development.

* It enables developers to create a **Domain-Specific Programming Language** and use it to write their applications.
* There are libraries available with ready-to-use implementations of many standard business and design patterns or technology integrations.

Rhetos works as a compiler that **generates the business application** from the source written in the DSL scripts.

* The generated application is a standard business applications based on Microsoft .NET technology stack.
* It is focused on the back-end development: It generates the business logic layer (C# object model), the database and the web API (REST, SOAP, etc.).
* The database is not generated from scratch on each deployment, it is upgraded instead, protecting the existing data.

Rhetos comes with the *CommonConcepts* DSL package, a programming language extension that contains many ready-to-use features for building applications.

[Syntax highlighting](https://github.com/Rhetos/Rhetos/wiki/Prerequisites#configure-your-text-editor-for-dsl-scripts-rhe)
is available for Visual Studio Code, SublimeText3 and Notepad++.

## Want to know more

See [Rhetos wiki](https://github.com/Rhetos/Rhetos/wiki) for more information on:

* Creating applications with Rhetos framework
* Rhetos DSL examples
* Available plugins
* Principles behind the [Rhetos platform](https://github.com/Rhetos/Rhetos/wiki/What-is-Rhetos)
* [Prerequisites, Installation and Development Environment](https://github.com/Rhetos/Rhetos/wiki/Development-Environment-Setup)

Visit the project web site at [rhetos.org](http://www.rhetos.org/).

## License

The code in this repository is licensed under version 3 of the AGPL unless
otherwise noted.

Please see `License.txt` for details.

## How to contribute

Contributions are very welcome. The easiest way is to fork this repo, and then
make a pull request from your fork. The first time you make a pull request, you
may be asked to sign a Contributor Agreement.

For more info see [How to Contribute](https://github.com/Rhetos/Rhetos/wiki/How-to-Contribute) on Rhetos wiki.

## Building and testing the source code

*Rhetos.sln* contains the source for Rhetos framework and CommonConcepts plugins (a standard library).
It also contains basic unit tests for the projects.

*CommonConceptsTest.sln* contains the *integration* tests for DSL concepts in CommonConcepts (DefaultConcepts libraries).
It has a test application with DSL scripts and unit tests in one project.
For example, see "Entity SimpleMaxLength" in Validation.rhe and the related tests in MaxLengthTest.cs.

After changing framework code in Rhetos.sln, you will need to run Build.bat
(to build NuGet packages) and Test.bat (to use these packages in CommonConceptsTest).
After that, you can develop tests in CommonConceptsTest.sln.
Note that Test.bat will report an error if you need to complete the initial setup:
create an empty database and enter the connection string in 'rhetos-app.local.settings.json' file.
