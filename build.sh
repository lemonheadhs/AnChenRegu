if [ ! -e "paket.lock" ]
then
    exec mono .paket/paket.exe install
fi
dotnet restore src/AnChenRegu
dotnet build src/AnChenRegu

