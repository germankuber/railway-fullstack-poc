FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TodoApi.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish TodoApi.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TodoApi.dll"]
