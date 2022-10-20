#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Accelist.WebApiStandard.KafkaWorker/Accelist.WebApiStandard.KafkaWorker.csproj", "Accelist.WebApiStandard.KafkaWorker/"]
RUN dotnet restore "Accelist.WebApiStandard.KafkaWorker/Accelist.WebApiStandard.KafkaWorker.csproj"
COPY . .
WORKDIR "/src/Accelist.WebApiStandard.KafkaWorker"
RUN dotnet build "Accelist.WebApiStandard.KafkaWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Accelist.WebApiStandard.KafkaWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Accelist.WebApiStandard.KafkaWorker.dll"]