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
dotnet publish -f net8.0 -r win-x64 --self-contained ./src/Shawarma.AspNetCore.Test/Shawarma.AspNetCore.Test.csproj
```
