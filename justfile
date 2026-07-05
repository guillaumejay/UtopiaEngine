set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

# Lance l'app desktop (défaut)
default: run

# Lance l'app desktop Avalonia
run:
    dotnet run --project UI/UE.UI.Desktop

# Lance la version navigateur (WASM)
browser:
    dotnet run --project UI/UE.UI.Browser

# Build moteur + UI
build:
    dotnet build UtopiaEngine.Net.slnx
    dotnet build UI/UE.UI.slnx

# Tests moteur (NUnit)
test:
    dotnet test UtopiaEngine.Net.slnx

# Un seul test : just test-one ScoreTest
test-one filter:
    dotnet test UtopiaEngine.Net.slnx --filter "FullyQualifiedName~{{filter}}"
