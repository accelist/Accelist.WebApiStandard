# Accelist Web API Standard
This is the standard template to be used on all Accelist projects for ensuring our standardized web API request and response flow using the Model-Validation-Request-Response (MVRR) pattern.

## `dotnet new` Template

Step 1: Install the template

```
git clone https://github.com/accelist/Accelist.WebApiStandard.git
cd .\Accelist.WebApiStandard\
dotnet new --install ./
```

> Run `dotnet new --uninstall ./` to remove a stale template

This message should be displayed in the console:

```
Success: installed the following templates:
Template Name      Short Name         Language  Tags
-----------------  -----------------  --------  --------------------
Accelist Standard  accelist-standard  [C#]      ASP.NET Core/Web API
```

Step 2: Go to an empty new project folder

```
cd D:\VS\Company.ProjectName
dotnet new accelist-standard -n Company.ProjectName
```

## Accelist Next.js Starter Jump Start

Set `.env.development` or `.env.local` to these values:

```
BACKEND_API_HOST=http://localhost:5052
OIDC_ISSUER=http://localhost:5051
OIDC_CLIENT_ID=cms
OIDC_SCOPE=openid profile email roles offline_access api
``` 

## Architecture

### MVRR Pattern Diagram
![MVRR Pattern](/docs/assets/images/mvrr-pattern.png)

### MVRR Flow Diagram
![MVRR Flow](/docs/assets/images/mvrr-flow.png)

## Installation
### Developer Tools
To begin the development of the application, you must install these following softwares:
| Name | Version | Type | DL Link | Notes |
| ---- | ------- | ---- | ------- | ----- |
| .NET | 6 | SDK | https://dotnet.microsoft.com/en-us/download/dotnet/6.0 |  |
| Visual Studio | 2022 | IDE | | (Optional) you could use other IDE or code editor. |
| Visual Studio Code | | IDE | | (Optional) you could use other IDE or code editor. |
| Docker | | Container | | (Optional) for test and run the app along with Jaeger. |

## Extras

### Jaeger Tracing
This web API template also includes [Jaeger](https://www.jaegertracing.io/) integration.

We also provides an example of Jaeger Docker container script on `run-jaeger.ps1`.