FROM hub.lumigame.com/base/dotnet/core/sdk:5.0.102-ca-patch-buster-slim AS builder
WORKDIR /src

COPY ./SG.Core ./SG.Core
COPY ./SG.Api/SG.Api.csproj ./SG.Api/SG.Api.csproj

WORKDIR /src/SG.Api
RUN dotnet restore -v diag

COPY ./SG.Api .

RUN dotnet publish -r linux-x64 --self-contained false -o /app 

FROM hub.lumigame.com/base/dotnet/core/aspnet:5.0-buster-slim AS base

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "SG.Api.dll"]
