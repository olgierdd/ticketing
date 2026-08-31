FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Ticketing.Api/Ticketing.Api.csproj Ticketing.Api/
RUN dotnet restore Ticketing.Api/Ticketing.Api.csproj

COPY Ticketing.Api/ Ticketing.Api/
WORKDIR /src/Ticketing.Api
RUN dotnet publish Ticketing.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Ticketing.Api.dll"]
