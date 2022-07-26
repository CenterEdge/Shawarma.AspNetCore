# Developing Shawarma.AspNetCore

## Shawarma.UnitTests

This project contains unit tests, and will be run with every build.

## Shawarma.AspNetCore.Test

This project may be run to test behaviors locally. Application state may be
posted to <http://localhost:64007/applicationstate> as JSON. The console logs may
be observed to see the `TestService` starting and stopping.

### Testing trimming

To test trimming support, publish the `Shawarma.AspNetCore.Test` application. This
will produce warnings for any trimming issues.

```sh
dotnet publish -f net6.0 -r win10-x64 --self-contained ./src/Shawarma.AspNetCore.Test/Shawarma.AspNetCore.Test.csproj
```

However, note that warnings for ASP.NET assemblies will appear. This is because ASP.NET 6
doesn't yet fully support trimming. We're doing this as future proofing for ASP.NET 7 which
is planned to support trimming. So long as all of the warnings are for .NET assemblies and
not Shawarma assemblies it is okay.
