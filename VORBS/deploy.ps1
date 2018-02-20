param(
    [string]$OutDir = "$HOME\Desktop\VORBS_DEPLOY"
)

bower install

msbuild .\VORBS.csproj /p:Configuration=Release /p:Platform=AnyCPU /t:WebPublish /p:WebPublishMethod=FileSystem /p:DeleteExistingFiles=True /p:PublishUrl=$OutDir