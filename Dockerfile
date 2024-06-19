FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY . /build
WORKDIR /build
RUN dotnet build --configuration Release ./src

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS run
RUN apt-get update && apt-get install -y ffmpeg
COPY --from=build /build /app
WORKDIR /app/src

CMD ["dotnet", "run"]