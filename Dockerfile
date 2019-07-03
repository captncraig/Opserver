#https://github.com/dotnet/dotnet-docker/blob/master/samples/aspnetapp/Dockerfile.alpine-x64

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview6-alpine3.9 AS build

WORKDIR /app

# copy just sln and csproj files first and do restore.
# this helps us make use of docker's layer caching 
# and hopefully reduce repeated pulls
COPY Opserver.sln .
COPY nuget.config .
COPY src/Opserver/*.csproj ./src/Opserver/
COPY src/Opserver.Core/*.csproj ./src/Opserver.Core/
COPY src/Opserver.Web/*.csproj ./src/Opserver.Web/
WORKDIR /app/src/Opserver.Web
RUN dotnet restore

# copy everything else and build app
# https://github.com/AArnott/Nerdbank.GitVersioning/issues/314 is killing me
COPY . /app
RUN dotnet publish -c Release -o out -f netcoreapp3.0
