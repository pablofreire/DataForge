# Multi-stage build shared by the three hosts. Select one with --build-arg HOST=DataForge.Web|DataForge.Worker|DataForge.Migrator.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG HOST=DataForge.Web
WORKDIR /src

# Restore first so package layers are cached across source-only changes.
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/ src/
RUN dotnet restore "src/Hosts/${HOST}/${HOST}.csproj"
RUN dotnet publish "src/Hosts/${HOST}/${HOST}.csproj" -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ARG HOST=DataForge.Web
ENV HOST_ASSEMBLY=${HOST}.dll \
    DOTNET_gcServer=1
WORKDIR /app

# Shared volumes for uploaded files and the data protection key ring, writable by the non-root app user.
RUN mkdir -p /data/files /data/keys && chown -R ${APP_UID} /data

COPY --from=build /app .
USER ${APP_UID}
ENTRYPOINT ["sh", "-c", "exec dotnet \"$HOST_ASSEMBLY\" \"$@\"", "--"]
