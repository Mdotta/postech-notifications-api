FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM public.ecr.aws/lambda/dotnet:8
WORKDIR /var/task
COPY --from=build /app .
CMD ["Postech.Notifications.Lambda::Postech.Notifications.Lambda.Function::FunctionHandler"]
