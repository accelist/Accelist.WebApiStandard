#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Accelist.WebApiStandard.KafkaBackgroundService/Accelist.WebApiStandard.KafkaBackgroundService.csproj", "Accelist.WebApiStandard.KafkaBackgroundService/"]
RUN dotnet restore "Accelist.WebApiStandard.KafkaBackgroundService/Accelist.WebApiStandard.KafkaBackgroundService.csproj"
COPY . .
WORKDIR "/src/Accelist.WebApiStandard.KafkaBackgroundService"
RUN dotnet build "Accelist.WebApiStandard.KafkaBackgroundService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Accelist.WebApiStandard.KafkaBackgroundService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Accelist.WebApiStandard.KafkaBackgroundService.dll"]