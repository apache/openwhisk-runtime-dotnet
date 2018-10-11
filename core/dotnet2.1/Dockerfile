FROM microsoft/dotnet:2.1-sdk-alpine AS build

WORKDIR /app
COPY proxy/Apache.OpenWhisk.Runtime.Common/*.csproj ./Apache.OpenWhisk.Runtime.Common/
COPY proxy/Apache.OpenWhisk.Runtime.Dotnet.Minimal/*.csproj ./Apache.OpenWhisk.Runtime.Dotnet.Minimal/
COPY proxy/openwhisk-runtime-dotnet.sln ./openwhisk-runtime-dotnet.sln
RUN dotnet restore

COPY proxy/Apache.OpenWhisk.Runtime.Common/. ./Apache.OpenWhisk.Runtime.Common/
COPY proxy/Apache.OpenWhisk.Runtime.Dotnet.Minimal/. ./Apache.OpenWhisk.Runtime.Dotnet.Minimal/
WORKDIR /app/Apache.OpenWhisk.Runtime.Dotnet.Minimal
RUN dotnet publish -c Release -r alpine.3.7-x64 -o out

FROM microsoft/dotnet:2.1-runtime-alpine AS runtime
WORKDIR /app
COPY --from=build /app/Apache.OpenWhisk.Runtime.Dotnet.Minimal/out ./
ENV ASPNETCORE_URLS http://+:8080
EXPOSE 8080/tcp
ENTRYPOINT ["dotnet", "Apache.OpenWhisk.Runtime.Dotnet.Minimal.dll"]
