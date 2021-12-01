#
# Licensed to the Apache Software Foundation (ASF) under one or more
# contributor license agreements.  See the NOTICE file distributed with
# this work for additional information regarding copyright ownership.
# The ASF licenses this file to You under the Apache License, Version 2.0
# (the "License"); you may not use this file except in compliance with
# the License.  You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine-amd64 AS build

WORKDIR /app

COPY proxy/Apache.OpenWhisk.Runtime.Dotnet.Minimal/*.csproj ./Apache.OpenWhisk.Runtime.Dotnet.Minimal/
COPY proxy/openwhisk-runtime-dotnet.sln ./openwhisk-runtime-dotnet.sln
RUN dotnet restore


COPY proxy/Apache.OpenWhisk.Runtime.Dotnet.Minimal/. ./Apache.OpenWhisk.Runtime.Dotnet.Minimal/
WORKDIR /app/Apache.OpenWhisk.Runtime.Dotnet.Minimal
RUN dotnet publish -c Release -r linux-musl-x64 -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine-amd64 AS runtime

# Get the latest security fixes in case the base image does not contain them already.
RUN apk update \
    && apk upgrade \
    && rm -f /var/cache/apk/*

WORKDIR /app
COPY --from=build /app/Apache.OpenWhisk.Runtime.Dotnet.Minimal/out ./
ENV ASPNETCORE_URLS http://+:8080
EXPOSE 8080/tcp
ENTRYPOINT ["dotnet", "Apache.OpenWhisk.Runtime.Dotnet.Minimal.dll"]
