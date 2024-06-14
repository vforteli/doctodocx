FROM mcr.microsoft.com/dotnet/runtime-deps:8.0 AS base

RUN apt update && apt install -y libreoffice



FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build

WORKDIR /source

COPY *.csproj ./DocToSomething/

WORKDIR /source/DocToSomething
RUN dotnet restore

WORKDIR /source

COPY . ./DocToSomething/

WORKDIR /source/DocToSomething
RUN dotnet publish -c Release -o /app --self-contained


FROM base AS final

WORKDIR /app
COPY --from=build /app ./

USER $APP_UID
ENTRYPOINT ["./DocToSomething"]
