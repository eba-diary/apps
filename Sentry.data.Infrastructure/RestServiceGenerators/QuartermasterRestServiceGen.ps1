$ErrorActionPreference = "Stop"

write-host "Getting Quartermaster REST API swagger json.."
$swaggerJson = Invoke-WebRequest -UseDefaultCredential https://quartermasterservice.sentry.com/v1/swagger
$swaggerJson.Content | Out-File "QuartermasterRestClient.swagger.json" ASCII

#Remove Form URL-Encode because it causes generation (w/ nswag version) that doesn't handle complex objects - https://github.com/RicoSuter/NSwag/issues/3414
(Get-Content QuartermasterRestClient.swagger.json).Replace(',"application/x-www-form-urlencoded"','') | Set-Content QuartermasterRestClient.swagger.json

write-host "Generating Quartermaster REST API CSharp Client.."
nswag\nswag.cmd swagger2csclient /input:"QuartermasterRestClient.swagger.json" /output:"QuartermasterRestClient.cs" /ContractsOutput:"IQuartermasterRestClient.cs" /Namespace:Sentry.data.Infrastructure.QuartermasterRestClient /ContractsNamespace:Sentry.data.Core.Interfaces.QuartermasterRestClient /GenerateClientInterfaces:true /GenerateContractsOutput:true

try 
{
    cp QuartermasterRestClient.cs ..\..\Sentry.data.Infrastructure\ServiceImplementations\QuartermasterRestClient.cs
    cp IQuartermasterRestClient.cs ..\..\Sentry.data.Core\Interfaces\IQuartermasterRestClient.cs
    rm QuartermasterRestClient.swagger.json
    rm QuartermasterRestClient.cs
    rm IQuartermasterRestClient.cs
} 
catch 
{
    Write-Host $_.Exception.Message -ForegroundColor red
}
