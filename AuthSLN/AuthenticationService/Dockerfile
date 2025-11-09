#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AuthenticationService/AuthenticationService.csproj", "AuthenticationService/"]
RUN dotnet restore "AuthenticationService/AuthenticationService.csproj"
COPY . .
WORKDIR "/src/AuthenticationService"
RUN dotnet restore "AuthenticationService.csproj"
RUN dotnet build "AuthenticationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AuthenticationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["AuthenticationService/jwt_public.pem", "."]
COPY ["AuthenticationService/jwt_private.pem", "."]
COPY ["AuthenticationService/aspnetapp.pfx", "."]
ENTRYPOINT ["dotnet", "AuthenticationService.dll"]
