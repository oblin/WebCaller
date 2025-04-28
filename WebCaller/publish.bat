## publish to linux
### release
## dotnet publish -c release -r linux-x64 
### debug
dotnet publish -c debug -r linux-x64 --self-contained true
pushd .\bin\Debug\net8.0\linux-x64\publish\

pscp -pw Cent@490910 -r .\* centos@192.168.19.211:/home/centos/webcaller
pscp -pw Cent@490910 -r .\appsettings.linux.json centos@192.168.19.211:/home/centos/webcaller/appsettings.json

popd
