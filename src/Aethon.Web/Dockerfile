FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Aethon.sln", "."]
COPY ["global.json", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Aethon.Web/Aethon.Web.csproj", "src/Aethon.Web/"]
COPY ["src/Aethon.Shared/Aethon.Shared.csproj", "src/Aethon.Shared/"]

RUN dotnet restore "src/Aethon.Web/Aethon.Web.csproj"

COPY . .
RUN dotnet publish "src/Aethon.Web/Aethon.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Aethon.Web.dll"]