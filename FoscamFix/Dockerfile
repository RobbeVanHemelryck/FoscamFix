#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["FoscamFix.csproj", "FoscamFix/"]
RUN dotnet restore "FoscamFix.csproj"
COPY . .
WORKDIR "/src/FoscamFix"
RUN dotnet build "FoscamFix.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FoscamFix.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoscamFix.dll"]