# Build context: parent folder that contains both postech-notifications-api and postech-shared (e.g. projeto2).
# Example: docker build -f postech-notifications-api/Dockerfile .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY postech-shared ./postech-shared
COPY postech-notifications-api/src ./postech-notifications-api/src
RUN dotnet publish postech-notifications-api/src/Postech.Notifications.Api/Postech.Notifications.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Postech.Notifications.Api.dll"]
