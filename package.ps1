$date = Get-Date
$version = $date.ToString("yyyy-dd-M--HH-mm-ss")
$filename = "S3MailGeneratorv2-" + $version + ".zip"
cd .\S3MailGeneratorv2\src\S3MailGeneratorv2
dotnet lambda package ..\..\..\Packages\$filename --configuration Release -frun dotnet6 -farch arm64
cd ..\..\..