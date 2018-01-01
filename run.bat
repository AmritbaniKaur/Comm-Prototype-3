@echo off

cd ./RemoteBuildServer/bin/Debug/

START RemoteBuildServer.exe 3



cd ../../../NavigatorClient/bin/Debug/

START NavigatorClient.exe



cd ../../../RepoMock/bin/Debug

START RepoMock.exe

