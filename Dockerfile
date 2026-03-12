FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/ .
RUN dotnet restore Postech.Notifications.Api/Postech.Notifications.Api.csproj
RUN dotnet build Postech.Notifications.Api/Postech.Notifications.Api.csproj -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
WORKDIR /src
COPY --from=build /src/ .
RUN dotnet publish Postech.Notifications.Api/Postech.Notifications.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Postech.Notifications.Api.dll"]
