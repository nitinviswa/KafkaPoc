FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5157

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ServiceB/ServiceB.csproj", "ServiceB/"]
RUN dotnet restore "ServiceB/ServiceB.csproj"
COPY ServiceB/. ./ServiceB/
WORKDIR "/src/ServiceB"
RUN dotnet build "ServiceB.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServiceB.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish ./
ENTRYPOINT ["dotnet", "ServiceB.dll"]
    