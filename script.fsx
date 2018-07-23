#r "System"
#load ".paket/load/net451/scripts/scripts.group.fsx"

open System
open Dapper
open System.Data.SQLite

let databaseFilename = "sample.sqlite"
let connectionStringFile = sprintf "Data Source=%s;Version=3;" databaseFilename  

SQLiteConnection.CreateFile(databaseFilename)


