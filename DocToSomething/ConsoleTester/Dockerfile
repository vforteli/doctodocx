FROM mcr.microsoft.com/dotnet/runtime:8.0 AS derp

RUN apt update && apt install -y libreoffice



FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build

WORKDIR /source

COPY DocToSomething/ConsoleTester/*.csproj ./ConsoleTester/

WORKDIR /source/ConsoleTester
RUN dotnet restore

WORKDIR /source

COPY DocToSomething/ConsoleTester/. ./ConsoleTester/

WORKDIR /source/ConsoleTester
RUN dotnet publish -c Release -o /app


FROM derp AS final

WORKDIR /app
COPY --from=build /app ./
COPY helloworld.doc ./
RUN chmod 666 helloworld.doc

USER $APP_UID
ENTRYPOINT ["dotnet", "ConsoleTester.dll"]
