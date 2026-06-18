# Build from the repository root: docker build -f src/KnowledgeAi.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/KnowledgeAi.Api/KnowledgeAi.Api.csproj", "src/KnowledgeAi.Api/"]
COPY ["src/KnowledgeAi.Application/KnowledgeAi.Application.csproj", "src/KnowledgeAi.Application/"]
COPY ["src/KnowledgeAi.Domain/KnowledgeAi.Domain.csproj", "src/KnowledgeAi.Domain/"]
COPY ["src/KnowledgeAi.Infrastructure/KnowledgeAi.Infrastructure.csproj", "src/KnowledgeAi.Infrastructure/"]
RUN dotnet restore "src/KnowledgeAi.Api/KnowledgeAi.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/KnowledgeAi.Api/KnowledgeAi.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KnowledgeAi.Api.dll"]
