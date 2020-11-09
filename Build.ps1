dotnet restore

dotnet pack .\src\StreamDeckEmulator -c release
dotnet pack .\src\Tocsoft.StreamDeck.Core -c release
dotnet pack .\src\Tocsoft.StreamDeck.ImageSharp -c release
dotnet pack .\src\Tocsoft.StreamDeck -c release