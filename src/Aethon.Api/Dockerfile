FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Aethon.sln", "."]
COPY ["global.json", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Aethon.Api/Aethon.Api.csproj", "src/Aethon.Api/"]
COPY ["src/Aethon.Data/Aethon.Data.csproj", "src/Aethon.Data/"]
COPY ["src/Aethon.Shared/Aethon.Shared.csproj", "src/Aethon.Shared/"]
COPY ["src/Aethon.Application/Aethon.Application.csproj", "src/Aethon.Application/"]

RUN dotnet restore "src/Aethon.Api/Aethon.Api.csproj"

COPY . .
RUN dotnet publish "src/Aethon.Api/Aethon.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Aethon.Api.dll"]