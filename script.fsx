#r "System"
#r "System.Data.Common"
#load ".paket/load/net46/scripts/scripts.group.fsx"
#r "packages/scriptPatch/NETStandard.Library.NETFramework/build/net461/lib/netstandard.dll"
#r "packages/scripts/SqlFrags/lib/netstandard2.0/SqlFrags.dll"


open System
open System.IO
open Dapper
open System.Data.SQLite

let databaseFilename = "sample.sqlite"
let connectionStringFile = sprintf "Data Source=%s;Version=3;" databaseFilename  

if not <| File.Exists(databaseFilename) then
    SQLiteConnection.CreateFile(databaseFilename)

let createConn() = 
    let conn = new SQLiteConnection(connectionStringFile)
    conn.Open()
    conn

[<CLIMutable>]
type Leave = {
    Id: int
    LeaveType: string
    Applicant: string
    Reason: string
    StartTime: DateTime
    EndTime: DateTime
    Amount: string
    ApprBy: string
    MoreComments: string    
}

let l1 = 
    { Id = 0
      LeaveType = "SickDay"
      Applicant = "lemon@trees.com"
      Reason = "flu"
      StartTime = new DateTime(2018, 7, 22)
      EndTime = new DateTime(2018, 7, 23)
      Amount = ((new TimeSpan(8, 0, 0)).ToString())
      ApprBy = "apple@trees.com"
      MoreComments = null }

(*
open Dapper.FastCrud
OrmConfiguration.DefaultDialect = SqlDialect.SqLite
OrmConfiguration.RegisterEntity<Leave>()

let test() =
    use conn = createConn()
    conn.Insert(l1)

test()
*)

open SqlFrags.SqlGen

let Leaves = Table "Leaves"

let insertSql =
    [Leaves.Insert [
        "LeaveType", "@LeaveType"
        "Applicant", "@Applicant"
        "Reason", "@Reason"
        "StartTime", "@StartTime"
        "EndTime", "@EndTime"
        "Amount", "@Amount"
        "ApprBy", "@ApprBy"
        "MoreComments", "@MoreComments"
    ]] |> Frags.Emit SqlSyntax.Any

let test2() =
    use conn = createConn()
    conn.Execute(insertSql, l1)

test2()

let test3() =
    let sql = [ Leaves --> ["*"]; WhereS "Id = 1" ] |> Frags.Emit SqlSyntax.Any
    let conn = createConn()
    conn.Query<Leave>(sql)

test3()





