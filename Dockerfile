#https://github.com/dotnet/dotnet-docker/blob/master/samples/aspnetapp/Dockerfile.alpine-x64

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview6-alpine3.9 AS build

WORKDIR /app

# copy just sln and csproj files first and do restore.
# this helps us make use of docker's layer caching 
# and hopefully reduce repeated pulls
COPY Opserver.sln .
COPY nuget.config .
COPY src/Opserver.Core/*.csproj ./src/Opserver.Core/
COPY src/Opserver.Web/*.csproj ./src/Opserver.Web/
COPY src/Directory.Build.props /src/
WORKDIR /app/src/Opserver.Web
RUN dotnet restore

# copy everything else and build app
COPY . /app
RUN dotnet publish -c Release -o /out -f netcoreapp3.0
RUN dotnet --list-sdks
RUN ls -la /out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-preview6-alpine3.9 AS runtime
WORKDIR /app
COPY --from=build /out ./
RUN dotnet --list-sdks
ENTRYPOINT ["dotnet", "StackExchange.Opserver.dll"]