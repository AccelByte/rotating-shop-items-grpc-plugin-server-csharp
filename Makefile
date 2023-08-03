# Copyright (c) 2022-2023 AccelByte Inc. All Rights Reserved.
# This is licensed software from AccelByte Inc, for limitations
# and restrictions contact your company contract manager.

SHELL := /bin/bash

IMAGE_NAME := $(shell basename "$$(pwd)")-app
DOTNETVER := 6.0.302

.PHONY: build image imagex test

build:
	docker run --rm -u $$(id -u):$$(id -g) -v $$(pwd):/data/ -w /data/src -e HOME="/data" -e DOTNET_CLI_HOME="/data" mcr.microsoft.com/dotnet/sdk:$(DOTNETVER) \
			dotnet build

image:
	docker build -t ${IMAGE_NAME} .

imagex:
	docker buildx inspect ${IMAGE_NAME}-builder \
			|| docker buildx create --name ${IMAGE_NAME}-builder --use 
	docker buildx build -t ${IMAGE_NAME} --platform linux/arm64/v8,linux/amd64 .
	docker buildx build -t ${IMAGE_NAME} --load .
	#docker buildx rm ${IMAGE_NAME}-builder

test:
	docker run --rm -u $$(id -u):$$(id -g) -v $$(pwd):/data/ -w /data/src -e HOME="/data" -e DOTNET_CLI_HOME="/data" mcr.microsoft.com/dotnet/sdk:$(DOTNETVER) \
			dotnet test