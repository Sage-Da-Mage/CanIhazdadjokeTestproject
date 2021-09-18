#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Learning.API/Learning.API.csproj", "Learning.API/"]
RUN dotnet restore "Learning.API/Learning.API.csproj"
COPY . .
WORKDIR "/src/Learning.API"
RUN dotnet build "Learning.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Learning.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Learning.API.dll"]