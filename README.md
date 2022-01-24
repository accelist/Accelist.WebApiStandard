# Accelist Web API Standard
This is the standard template to be used on all Accelist projects for ensuring our standardized web API request and response flow using the Model-Validation-Request-Response (MVRR) pattern.

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