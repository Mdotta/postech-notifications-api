
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /repo

FROM base AS build
COPY src ./src
COPY Postech.Shared.dll ./Postech.Shared.dll
WORKDIR /repo/src/Postech.Notifications.Api
RUN dotnet restore Postech.Notifications.Api.csproj
RUN dotnet build Postech.Notifications.Api.csproj -c Release -o /app/build

FROM build AS test
WORKDIR /repo/src/Postech.Notifications.Api.Tests
RUN dotnet test Postech.Notifications.Api.Tests.csproj -c Release --verbosity normal

FROM build AS publish
WORKDIR /repo/src/Postech.Notifications.Api
RUN dotnet publish Postech.Notifications.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Postech.Notifications.Api.dll"]
