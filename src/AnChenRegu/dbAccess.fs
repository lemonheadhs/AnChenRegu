module AnChenRegu.DBAccess

open DBModels
open Dapper
open SqlFrags.SqlGen
open Microsoft.Data.Sqlite
open System.Data

let createAndOpenConn connStr =
    let conn = new SqliteConnection(connStr)
    conn.Open()
    conn :> IDbConnection

let getRecordTypeName<'T> = 
    let tInfo = typeof<'T>
    tInfo.Name

let getRecordPropNames<'T> =
    let tInfo = typeof<'T>
    tInfo.GetProperties()
    |> Seq.map (fun f -> f.Name)
    |> Seq.toList

type IConnectionFactory =
    abstract member Create: unit -> IDbConnection


let insert excludes (x: 'a) (factory: IConnectionFactory) =
    let tableName = getRecordTypeName<'a>
    let fields = getRecordPropNames<'a> |> List.except excludes
    let pairs = fields |> List.map (fun f -> f, "@" + f)
    let table = Table tableName
    let insertSql =
        [ table.Insert fields ] |> Frags.Emit SqlSyntax.Any
    use conn = factory.Create()
    conn.Execute(insertSql, x)


