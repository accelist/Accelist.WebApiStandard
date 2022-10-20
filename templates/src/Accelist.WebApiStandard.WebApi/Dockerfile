#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Accelist.WebApiStandard.WebApi/Accelist.WebApiStandard.WebApi.csproj", "Accelist.WebApiStandard.WebApi/"]
RUN dotnet restore "Accelist.WebApiStandard.WebApi/Accelist.WebApiStandard.WebApi.csproj"
COPY . .
WORKDIR "/src/Accelist.WebApiStandard.WebApi"
RUN dotnet build "Accelist.WebApiStandard.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Accelist.WebApiStandard.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Accelist.WebApiStandard.WebApi.dll"]