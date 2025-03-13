FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.19 AS builder
ARG TARGETARCH
RUN apk update && apk add --no-cache gcompat
WORKDIR /build
COPY src/AccelByte.PluginArch.ItemRotation.Demo.Server/*.csproj .
RUN ([ "$TARGETARCH" = "amd64" ] && echo "linux-musl-x64" || echo "linux-musl-$TARGETARCH") > /tmp/dotnet-rid
RUN dotnet restore -r $(cat /tmp/dotnet-rid)
COPY src/AccelByte.PluginArch.ItemRotation.Demo.Server/ .
RUN dotnet publish -c Release -r $(cat /tmp/dotnet-rid) --no-restore -o /output


FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.19
WORKDIR /app
COPY --from=builder /output/ .
# gRPC server port, Prometheus /metrics port
EXPOSE 6565 8080
ENTRYPOINT ["/app/AccelByte.PluginArch.ItemRotation.Demo.Server"]