# Build from the repository root: docker build -f src/KnowledgeAi.Mcp/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/KnowledgeAi.Mcp/KnowledgeAi.Mcp.csproj", "src/KnowledgeAi.Mcp/"]
COPY ["src/KnowledgeAi.Application/KnowledgeAi.Application.csproj", "src/KnowledgeAi.Application/"]
COPY ["src/KnowledgeAi.Domain/KnowledgeAi.Domain.csproj", "src/KnowledgeAi.Domain/"]
RUN dotnet restore "src/KnowledgeAi.Mcp/KnowledgeAi.Mcp.csproj"

COPY src/ src/
RUN dotnet publish "src/KnowledgeAi.Mcp/KnowledgeAi.Mcp.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KnowledgeAi.Mcp.dll"]
