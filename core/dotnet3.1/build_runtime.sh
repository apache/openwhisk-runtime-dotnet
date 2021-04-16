#!/bin/bash

IMAGE="mwelke/openwhisk-runtime-dotnet-v3.1-nojson:1"

docker build -t $IMAGE .
docker push $IMAGE
