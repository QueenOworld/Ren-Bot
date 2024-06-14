FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY . /build
WORKDIR /build
RUN dotnet build ./src

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS run
COPY --from=build /build /app
WORKDIR /app/src

CMD ["dotnet", "run"]
