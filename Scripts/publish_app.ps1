function PrepareFilesForTargetEnvironment {
  param (
    [string]$TargetEnv,
    [string]$PublishFolder
  )

  $TargetFramework = "net8.0"
  Write-Host "Publishing project HttpMock.csproj..." -ForegroundColor Yellow
  dotnet publish -r $TargetEnv -f $TargetFramework --self-contained -p:PublishTrimmed=true -p:PublishReadyToRun=true -p:PublishReadyToRunDir="$PublishFolder" "..\src\HttpMock\HttpMock.csproj"
  Write-Host "Done!" -ForegroundColor Green
  Write-Host

  Write-Host "Copying files to publish folder..." -ForegroundColor Yellow
  Write-Debug "Source directory: '..\src\HttpMock\bin\Release\$TargetFramework\$TargetEnv\publish\'"
  Write-Debug "Destination directory: '$PublishFolder\$TargetEnv'"
  New-Item -ItemType Directory -Path "$PublishFolder\$TargetEnv" -ErrorAction Stop > $null
  Copy-Item -Path "..\src\HttpMock\bin\Release\$TargetFramework\$TargetEnv\publish\*" -Destination "$PublishFolder\$TargetEnv" -Recurse -Force
  Write-Host "Done!" -ForegroundColor Green
  Write-Host
}

function PublishApp {
  $PublishFolder = "$PSScriptRoot\publish"
  Write-Host "Initializing pubish folder: '$PublishFolder'..." -ForegroundColor Yellow

  # Remove the publish folder if it exists
  if (Test-Path -Path $PublishFolder) {
    try { Remove-Item -Path $PublishFolder -Recurse > $null } 
    catch { Write-Host "Failed to remove publish folder!" -ForegroundColor "Red" Exit -1 }
  }

  # Create a new publish folder
  New-Item -ItemType Directory -Path $PublishFolder -ErrorAction Stop > $null
  Write-Host "Done!" -ForegroundColor Green
  Write-Host

  # Publish for Linux x64
  # $TargetEnv = "linux-x64"
  # PrepareFilesForTargetEnvironment -TargetEnv $TargetEnv -PublishFolder $PublishFolder

  # Publish for Windows x64
  $TargetEnv = "win-x64"
  PrepareFilesForTargetEnvironment -TargetEnv $TargetEnv -PublishFolder $PublishFolder

  # Create a _run.bat file to start the application
  $DefaultPort = 58888
  $Quiet = 0
  New-Item -Path "$PublishFolder\$TargetEnv\_run.bat" -ItemType File -Value "HttpMock.exe --port $DefaultPort --quiet $Quiet" -Force > $null
}

PublishApp