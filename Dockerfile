# syntax=docker/dockerfile:1.6
# Shared multi-stage Dockerfile for every Hellion server.
# Build with: docker build --build-arg PROJECT=Hellion.Login -t hellion-login .
# Or via docker-compose, which sets PROJECT for each service.

ARG PROJECT=Hellion.Login

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY global.json ./
COPY Hellion.sln ./
COPY src/ ./src/
COPY test/ ./test/
ARG PROJECT
RUN dotnet restore "src/${PROJECT}/${PROJECT}.csproj"
RUN dotnet publish "src/${PROJECT}/${PROJECT}.csproj" \
    -c Release \
    -o /app \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ARG PROJECT
ENV HELLION_PROJECT=${PROJECT}
COPY --from=build /app ./

# Each server reads its own appsettings.json copied during publish; runtime
# overrides (DB host, ports) come from environment variables via the
# Microsoft.Extensions.Configuration provider.
ENTRYPOINT ["sh", "-c", "exec dotnet ${HELLION_PROJECT}.dll"]
