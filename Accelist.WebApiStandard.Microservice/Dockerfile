#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Accelist.WebApiStandard.Microservice/Accelist.WebApiStandard.Microservice.csproj", "Accelist.WebApiStandard.Microservice/"]
RUN dotnet restore "Accelist.WebApiStandard.Microservice/Accelist.WebApiStandard.Microservice.csproj"
COPY . .
WORKDIR "/src/Accelist.WebApiStandard.Microservice"
RUN dotnet build "Accelist.WebApiStandard.Microservice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Accelist.WebApiStandard.Microservice.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Accelist.WebApiStandard.Microservice.dll"]