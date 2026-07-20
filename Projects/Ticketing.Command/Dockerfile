FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["Projects/Ticketing.Command/Ticketing.Command.csproj", "Projects/Ticketing.Command/"]
RUN dotnet restore "Projects/Ticketing.Command/Ticketing.Command.csproj"

# Copy everything and publish
COPY . .
RUN dotnet publish "Projects/Ticketing.Command/Ticketing.Command.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Ticketing.Command.dll"]
