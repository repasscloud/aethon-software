FROM repasscloud/aethon-web-builder:mcr-dotnet-sdk-10.0 AS build
WORKDIR /src

COPY ["Aethon.sln", "."]
COPY ["global.json", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Aethon.Web/Aethon.Web.csproj", "src/Aethon.Web/"]
COPY ["src/Aethon.Shared/Aethon.Shared.csproj", "src/Aethon.Shared/"]
COPY ["src/Aethon.Data/Aethon.Data.csproj", "src/Aethon.Data/"]

RUN dotnet restore "src/Aethon.Web/Aethon.Web.csproj"

COPY . .

# Download TinyMCE from npm (no CDN / no API key needed at runtime)
RUN npm install --prefix /tmp/tinymce-pkg tinymce@7 \
    && mkdir -p src/Aethon.Web/wwwroot/lib/tinymce \
    && cp -r /tmp/tinymce-pkg/node_modules/tinymce/* src/Aethon.Web/wwwroot/lib/tinymce/

RUN dotnet publish "src/Aethon.Web/Aethon.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Aethon.Web.dll"]