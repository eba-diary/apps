$ErrorActionPreference = "Stop"

write-host "Getting SAID REST API swagger json.."
$swaggerJson = Invoke-WebRequest -UseDefaultCredential https://saservicerestqual.sentry.com/v1/swagger
$swaggerJson.Content | Out-File "SAIDRestClient.swagger.json" ASCII

write-host "Generating SAID REST API CSharp Client.."
nswag\nswag.cmd swagger2csclient /input:"SAIDRestClient.swagger.json" /output:"SAIDRestClient.cs" /ContractsOutput:"ISAIDRestClient.cs" /Namespace:Sentry.data.Infrastructure.SAIDRestClient /ContractsNamespace:Sentry.data.Core.Interfaces.SAIDRestClient /GenerateClientInterfaces:true /GenerateContractsOutput:true

try 
{
    cp SAIDRestClient.cs ..\..\Sentry.data.Infrastructure\ServiceImplementations\SAIDRestClient.cs
    cp ISAIDRestClient.cs ..\..\Sentry.data.Core\Interfaces\ISAIDRestClient.cs
    rm SAIDRestClient.swagger.json
    rm SAIDRestClient.cs
    rm ISAIDRestClient.cs
} 
catch 
{
    Write-Host $_.Exception.Message -ForegroundColor red
}
