module AnChenRegu.DBAccess

open DBModels
open Dapper
open SqlFrags.SqlGen
open Microsoft.Data.Sqlite
open System.Data
open FSharp.Control.Tasks.ContextInsensitive
open Models
open DBModels
open CustomTypes

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
        task { 
            try
                let! i = conn.ExecuteAsync(insertSql, x)
                return Ok i
            with
                | ex -> return Error (DbError ex)
        }
        

let update<'a> excludes whereClause =
    let tableName = getRecordTypeName<'a>
    let fields = getRecordPropNames<'a> |> List.except excludes
    let pairs = fields |> List.map (fun f -> f, "@" + f)
    let table = Table tableName
    let updateSql =
        [ table.Update pairs; whereClause ] |> Frags.Emit SqlSyntax.Any
    fun (x: 'a) (factory: IConnectionFactory) ->
        use conn = factory.Create()
        task { 
            try
                let! i = conn.ExecuteAsync(updateSql, x)
                return Ok i
            with
                | ex -> return Error (DbError ex)
        }

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
                    | 1 -> Ok (Seq.head l)
                    | _ -> Error (DbError (exn "fail to find only one record"))
                return r
            with
                | ex -> return Error (DbError ex)            
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
                return Ok r
            with
                | ex -> return Error (DbError ex)
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

// ---------------------------
// LateRecord Store
// ---------------------------

let private insertLateRecord (x: TB_Late) = insert<TB_Late> ["Id"] x

let addLateRecord (x:LateRecord) =
    let dbRec =
        { Employee = x.Employee
          StartTime = x.Period.Start
          EndTime = x.Period.End
          Amount = x.Amount.ToString()
          Id = 0 }
    insertLateRecord dbRec

// ---------------------------
// LateRecord Store
// ---------------------------

let private insertWriteOffHours (x: TB_WriteOffHours) = insert<TB_WriteOffHours> [] x

let addWriteOffHours (x:WriteOffHours) =
    let srcInfo =
        match x.OId with
        | OvertimeWorkId id -> id, "OvertimeWork"
        | AnnualVacationId id -> id, "AnnualVacation"
    let destInfo =
        match x.LId with
        | LeaveId id -> id, "Leave"
        | LateId id -> id, "Late"
    let dbRec =
        { SourceId = fst srcInfo
          SourceKind = snd srcInfo
          DestId = fst destInfo
          DestKind = snd destInfo
          StartTime = x.Hours.Period.Start
          EndTime = x.Hours.Period.End
          Amount = (x.Hours.Amount.ToString()) }
    insertWriteOffHours dbRec







