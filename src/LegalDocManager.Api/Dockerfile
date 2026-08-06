FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/LegalDocManager.Api/LegalDocManager.Api.csproj", "src/LegalDocManager.Api/"]
COPY ["src/AgenticTesting.Engine/AgenticTesting.Engine.csproj", "src/AgenticTesting.Engine/"]
RUN dotnet restore "src/LegalDocManager.Api/LegalDocManager.Api.csproj"
COPY . .
WORKDIR "/src/src/LegalDocManager.Api"
RUN dotnet build "LegalDocManager.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LegalDocManager.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "LegalDocManager.Api.dll"]
