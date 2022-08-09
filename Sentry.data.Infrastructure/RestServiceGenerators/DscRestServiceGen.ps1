

write-host "Generating DSC REST API CSharp Client.."
nswag\nswag.cmd swagger2csclient /input:"DscRestClient.swagger.json" /output:"DscRestClient.cs" /ContractsOutput:"IDscRestClient.cs" /Namespace:Sentry.data.Infrastructure.DscRest /ContractsNamespace:Sentry.data.Core.Interfaces.DscRest /GenerateClientInterfaces:true /GenerateContractsOutput:true

try 
{
    cp DscRestClient.cs ..\..\Sentry.data.Infrastructure\ServiceImplementations\DscRest\DscRestClient.cs
    cp IDscRestClient.cs ..\..\Sentry.data.Core\Interfaces\DscRest\IDscRestClient.cs
} 
catch 
{
    Write-Host $_.Exception.Message -ForegroundColor red
}
