# Build the API
FROM mcr.microsoft.com/dotnet/nightly/sdk AS api-build
ARG COMMIT_SHA
ENV COMMIT_SHA=${COMMIT_SHA:-v1.0.0}
WORKDIR /src/server
COPY . /src
RUN dotnet publish \
    --version-suffix $COMMIT_SHA \
    -r linux-musl-x64 --self-contained true -p:PublishSingleFile=true \
    -c Release -o /publish

FROM mcr.microsoft.com/dotnet/nightly/runtime-deps:7.0-alpine AS runtime
ENV \
    # Use the default port for Cloud Run
    ASPNETCORE_URLS=http://+:8080
WORKDIR /app
COPY --from=api-build /publish .

ENTRYPOINT ["./Trucks.Server"]
