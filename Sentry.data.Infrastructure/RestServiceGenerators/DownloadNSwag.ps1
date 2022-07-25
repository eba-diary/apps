$version = '13.15.10'

$url = "https://artifactory.sentry.com/artifactory/developer-tools-local/nSwag/$version/nSwag.zip"

# download
Invoke-WebRequest -Uri $url -OutFile 'nSwag.zip'
# unzip
Expand-Archive -Path 'nSwag.zip' -DestinationPath 'nSwag'
# delete zip
Remove-Item -Path 'nSwag.zip'