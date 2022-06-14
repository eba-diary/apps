$ErrorActionPreference = "Stop"

write-host "Getting Infrastructure Eventing REST API swagger json.."
$cred = Get-Credential -Message "Enter your credentials to retrieve the Inev Swagger definition"
$swaggerJson = Invoke-WebRequest https://inevdev.sentry.com/v2/api-docs -Credential $cred
$swaggerJson.Content | Out-File "InevRestClient.swagger.json" ASCII

write-host "Generating Infrastructure Eventing REST API CSharp Client.."
nswag\nswag.cmd swagger2csclient /input:"InevRestClient.swagger.json" /output:"InevRestClient.cs" /ContractsOutput:"IInevRestClient.cs" /Namespace:Sentry.data.Infrastructure.InfrastructureEventing /ContractsNamespace:Sentry.data.Core.Interfaces.InfrastructureEventing /GenerateClientInterfaces:true /GenerateContractsOutput:true

try 
{
    cp InevRestClient.cs ..\..\Sentry.data.Infrastructure\ServiceImplementations\InfrastructureEventing\InevRestClient.cs
    cp IInevRestClient.cs ..\..\Sentry.data.Core\Interfaces\InfrastructureEventing\IInevRestClient.cs
    rm InevRestClient.swagger.json
    rm InevRestClient.cs
    rm IInevRestClient.cs
} 
catch 
{
    Write-Host $_.Exception.Message -ForegroundColor red
}
