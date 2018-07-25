module AnChenRegu.DBAccess

open DBModels
open Dapper
open SqlFrags.SqlGen
open Microsoft.Data.Sqlite
open System.Data
open FSharp.Control.Tasks.ContextInsensitive
open Models
open DBModels

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


let insert<'a> excludes =
    let tableName = getRecordTypeName<'a>
    let fields = getRecordPropNames<'a> |> List.except excludes
    let pairs = fields |> List.map (fun f -> f, "@" + f)
    let table = Table tableName
    let insertSql =
        [ table.Insert pairs ] |> Frags.Emit SqlSyntax.Any
    fun (x: 'a) (factory: IConnectionFactory) ->
        use conn = factory.Create()
        conn.ExecuteAsync(insertSql, x)

let update<'a> excludes whereClause =
    let tableName = getRecordTypeName<'a>
    let fields = getRecordPropNames<'a> |> List.except excludes
    let pairs = fields |> List.map (fun f -> f, "@" + f)
    let table = Table tableName
    let updateSql =
        [ table.Update pairs; whereClause ] |> Frags.Emit SqlSyntax.Any
    fun (x: 'a) (factory: IConnectionFactory) ->
        use conn = factory.Create()
        conn.ExecuteAsync(updateSql, x)

let delete<'a> whereClause =
    let tableName = getRecordTypeName<'a>
    let table = Table tableName
    let deleteSql =
        [ table.Delete; whereClause ] |> Frags.Emit SqlSyntax.Any
    fun (x: obj) (factory: IConnectionFactory) ->
        use conn = factory.Create()
        conn.ExecuteAsync(deleteSql, x)

let querySingle<'a> whereClause =
    let tableName = getRecordTypeName<'a>
    let table = Table tableName
    let selectSql =
        [ table --> ["*"]; whereClause ] |> Frags.Emit SqlSyntax.Any
    fun (x: obj) (factory: IConnectionFactory) ->
        use conn = factory.Create()
        task {
            try
                let! l = conn.QueryAsync<'a>(selectSql, x)                
                let r = 
                    match l |> Seq.length with
                    | 1 -> Some (Seq.head l)
                    | _ -> None
                return r
            with
                | ex -> return None            
        }

let query<'a> whereClause =
    let tableName = getRecordTypeName<'a>
    let table = Table tableName
    let selectSql =
        [ table --> ["*"]; whereClause ] |> Frags.Emit SqlSyntax.Any
    fun (x: obj) (factory: IConnectionFactory) ->
        use conn = factory.Create()
        task {
            try
                let! r = conn.QueryAsync<'a>(selectSql, x)
                return r
            with
                | ex -> return Seq.empty
        }


// ---------------------------
// LeaveRecord Store
// ---------------------------

let private insertLeave (x: TB_Leave) = insert<TB_Leave> ["Id"] x

let private translate x = 
    match x with
    | AnnualVacation -> "AnnualVacation"
    | PersonalAffair -> "PersonalAffair"
    | SickDay -> "SickDay"
    | CompassionateLeave -> "CompassionateLeave"
    | WeddingLeave -> "WeddingLeave"
    | MaternityLeave -> "MaternityLeave"
    | PaternityLeave -> "PaternityLeave"
    | Other -> "Other"

let private translate' x =
    match x with
    | "AnnualVacation" -> AnnualVacation
    | "PersonalAffair" -> PersonalAffair
    | "SickDay" -> SickDay
    | "CompassionateLeave" -> CompassionateLeave
    | "WeddingLeave" -> WeddingLeave
    | "MaternityLeave" -> MaternityLeave
    | "PaternityLeave" -> PaternityLeave
    | _ -> Other

let addLeaveRecord (x: LeaveRecord) =
    let typeStr = translate x.LeaveType
    let dbRec =
        { LeaveType = typeStr
          Applicant = x.Applicant
          Reason = x.Reason
          StartTime = x.Period.Start
          EndTime = x.Period.End
          Amount = x.Amount.ToString()
          ApprBy = x.ApprBy
          MoreComments = x.MoreComments
          Id = 0 }
    insertLeave dbRec


// ---------------------------
// OvertimeWork Store
// ---------------------------

let private insertOvertimeWork (x: TB_OvertimeWork) = insert<TB_OvertimeWork> ["Id"] x

let addOvertimeWork (x: OvertimeWork) =
    let dbRec = 
        { Employee = x.Employee
          StartTime = x.Period.Start
          EndTime = x.Period.End
          Reason = x.Reason
          Id = 0 }
    insertOvertimeWork dbRec

