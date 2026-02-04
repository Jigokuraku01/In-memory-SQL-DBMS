**Language:** [Русский](STAGE-1.md) | **English**

---

# Environment Setup

Link to install ASP.NET Core Runtime if not yet installed (comes with SDK):

> https://dotnet.microsoft.com/en-us/download/dotnet/6.0

# Creating an ASP.NET Core Project from Scratch

The main steps for creating a new project have already been done, however, I'll provide
them here so you can create a similar project following this instruction.

To create a minimalist Web service using ASP.NET Core
you need to execute several console commands:

1. Create a solution

> dotnet new sln -o mkn-immisql

2. Next, navigate to the created solution directory and create a project

> dotnet new webapi -minimal -o mkn-immisql

3. Add the created project to the solution

> dotnet sln add ./mkn-immisql/mkn-immisql.csproj

After project generation, we get this generated project file:

```
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>mkn_immisql</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.2.3" />
  </ItemGroup>

</Project>
```

I've tuned it a bit, based on our conventions from the first semester,
and requirements for working with ASP.NET Core.
You can change these settings as you see fit, however, if you use nullable, write code in the corresponding style. Version 10 of the language
is the minimum requirement here. Also renamed the project's root namespace,
to better match the naming style, resulted in:

```
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <RootNamespace>MknImmiSql</RootNamespace>
    <LangVersion>10</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.2.3" />
  </ItemGroup>

</Project>
```

Then the code was refined to match the new settings and demonstrate
working with controllers.

- [__Self-study__] Create your own project in a separate directory and familiarize yourself
  with how it looked at the very beginning.

Our minimalist project is ready. Can be launched.

# Launching the Service

## Launching in Development Environment

When launching the application from Rider or Visual Studio, it should automatically start
in developer mode, which will give you access to the Swagger interface for debugging your
endpoint's (or "handles", as they're often called) of your web service.

When launching from Rider, this page opens automatically, if not, then first
look at the console of the running application to determine the port on which
the service started:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7216
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5101
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: K:\.archive\СПбГУ-МКН\2023\23.Б10-МКН\ООП-2\Задания\mkn-immisql\mkn-immisql\
```

Copy the HTTPS link and add the swagger address, should look like:

> https://localhost:7216/swagger/index.html

Open the page in a browser and see the complete list of endpoints that your service supports. On first opening, the browser will likely tell you that
you're trying to access an unsafe site. This happens because
by default your service will use a self-signed certificate to protect
the network connection via HTTPS protocol. Feel free to proceed to the dangerous site, or,
if your browser will keep bothering you with this message each time, then you can
add the developer certificate to trusted on your machine. This can be done
as described in the instruction:

> https://learn.microsoft.com/ru-ru/aspnet/core/getting-started/?view=aspnetcore-8.0&tabs=windows#trust-the-development-certificate

To launch in Development environment from console, you need to pass the application
a command line argument, like this:

> mkn-immisql.exe --environment Development

## Launching in Production Environment

Here everything is simple, this is the default environment, so when launching your
application from the console, it will start in this mode.

In it, the swagger interface won't be available, which is good, because it reveals
information about absolutely all endpoints that your service supports, even those
secret ones that you don't advertise and use for internal needs and
don't include in the public API of the service.

A good practice would be to prohibit the use of swagger interface in production.

# Assignment 1

1. Read about ASP.Net Core articles from MSDN.
   > https://learn.microsoft.com/ru-ru/aspnet/core/getting-started
2. Study the ExampleController on how to implement
   standard service termination via IHostApplicationLifetime.
3. Add a class that will represent the global service context.
   Think about which design pattern is suitable for this purpose. Apply it.
   When creating the context, a random token should be generated, upon receiving which
   the service should terminate. It's convenient to generate random tokens using the
   random Guid generation method.

```c#
Guid.NewGuid().ToString("N") // can use other formatting types
```

4. Add a TerminateController with a POST endpoint to the service.
   This endpoint should accept as input an object with a single parameter token and
   String type. If the passed token equals the one you generated at
   application startup, you need to stop the service using the method from (2) and return
   Ok (200) from the endpoint. If the token differs, you need to return an error
   Forbidden (403).

```
POST /api/v1/terminate
input
{
  "token": "TERMINATION_TOKEN"
}
```

5. Add to the info endpoint (InfoController) a TerminationToken field
   that will externally provide the token that needs to be passed to the endpoint from (4) for
   application termination. Obviously, this shouldn't be done in production, and such a token
   should be buried in stores closed to the outside world, but for educational purposes
   it will do. The test environment will use this mechanism to terminate the service.
6. Remove ExampleController from the application.
7. Before sending source codes to the server, change in the mkn-immisql.testlab.json file,
   which is located next to the solution file, the value of the stage field to 1. This will tell the test
   lab which test suite to run for your application.
