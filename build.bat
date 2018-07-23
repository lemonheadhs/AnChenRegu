IF NOT EXIST paket.lock (
    START /WAIT .paket/paket.exe install
)
dotnet restore src/AnChenRegu
dotnet build src/AnChenRegu

