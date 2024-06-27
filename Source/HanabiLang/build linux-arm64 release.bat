@echo off
dotnet publish --runtime linux-arm64 --configuration Release --self-contained
@pause