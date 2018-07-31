module AnChenRegu.Handlers
open System
open System.Collections.Generic
open Giraffe
open Giraffe.ModelBinding
open Microsoft.AspNetCore.Http
open Models
open AnChenRegu.DBAccess
open CustomTypes
open CustomTypes.TaskResult
open Microsoft.Extensions.Logging

let bindJsonWithCheck<'a> (ctx: HttpContext) =
    task {
        try
            let! result = ctx.BindJsonAsync<'a>()
            return Ok result
        with
            | ex -> return Error (ValidationError "fail to bind json model")
    }

let customErrorHandler (logger: ILogger) (err: Errors) =
    match err with
    | ValidationError msg -> RequestErrors.BAD_REQUEST msg
    | DbError ex -> 
        logger.LogError(EventId(), ex, "db fail")
        ServerErrors.INTERNAL_ERROR "db fail"
    | IOError ex -> 
        logger.LogError(EventId(), ex, "io fail")
        ServerErrors.INTERNAL_ERROR "io fail"
    | NetworkError ex -> 
        logger.LogError(EventId(), ex, "network fail")
        ServerErrors.INTERNAL_ERROR "network fail"

let saveLeaveRecord (next: HttpFunc) (ctx: HttpContext) =
    let connFactory = ctx.GetService<IConnectionFactory>()
    let logger = ctx.GetLogger("saveLeaveRecord")
    task {
        let! handler =
            taskResult {
                let! r = bindJsonWithCheck<LeaveRecord>(ctx)
                let! i = addLeaveRecord r connFactory
                return i
            } |> TaskResult.toTaskBi (fun _ -> Successful.OK "created") (customErrorHandler logger)
        return! handler next ctx
    }


let saveOvertimeWork (next: HttpFunc) (ctx: HttpContext) =
    let connFactory = ctx.GetService<IConnectionFactory>()
    let logger = ctx.GetLogger("saveOvertimeWork")
    task {
        let! handler =
            taskResult {
                let! r = bindJsonWithCheck<OvertimeWork>(ctx)
                let! i = addOvertimeWork r connFactory
                return i
            } |> TaskResult.toTaskBi (fun _ -> Successful.OK "created") (customErrorHandler logger)
        return! handler next ctx
    }

let saveLateRecord (next: HttpFunc) (ctx: HttpContext) =
    let connFactory = ctx.GetService<IConnectionFactory>()
    let logger = ctx.GetLogger("saveLateRecord")
    task {
        let! handler =
            taskResult {
                let! r = bindJsonWithCheck<LateRecord>(ctx)
                let! i = addLateRecord r connFactory
                return i
            } |> TaskResult.toTaskBi (fun _ -> Successful.OK "created") (customErrorHandler logger)
        return! handler next ctx
    }

let saveWriteOffHours (next: HttpFunc) (ctx: HttpContext) =
    let connFactory = ctx.GetService<IConnectionFactory>()
    let logger = ctx.GetLogger("saveWriteOffHours")
    task {
        let! handler =
            taskResult {
                let! r = bindJsonWithCheck<WriteOffHours>(ctx)
                let! i = addWriteOffHours r connFactory
                return i
            } |> TaskResult.toTaskBi (fun _ -> Successful.OK "created") (customErrorHandler logger)
        return! handler next ctx
    }

// --------------------

type Month =
    | Jan = 1
    | Feb = 2
    | Mar = 3
    | Appr = 4
    | May = 5
    | Jun = 6
    | Jul = 7
    | Aug = 8
    | Seb = 9
    | Oct = 10
    | Nov = 11
    | Des = 12

type Week =
    | One = 1
    | Two = 2
    | Three = 3
    | Four = 4
    | Five = 5
    | Six = 6

type MonthWeek = { m: Month option; w: Week option }

let weeksNumberOfMonth (d: DateTime) =
    let dayNum = DateTime.DaysInMonth(d.Year, d.Month)
    let dayOfWeek = DateTime(d.Year, d.Month, 1).DayOfWeek
    match dayNum, dayOfWeek with
    | 28, DayOfWeek.Sunday -> 4
    | 30, DayOfWeek.Saturday | 31, (DayOfWeek.Friday | DayOfWeek.Saturday) -> 6
    | _ -> 5 

let packets offset len (l: 'c seq) = 
    if len < 1 then failwith "bad parameter len" 
    let offset' = if offset < 1 then len else offset
    let total = Seq.length l
    let ns = 
        if offset' < total then
            seq { yield offset'; yield! Seq.replicate (total / len) len; yield total % len }
        else
            seq { yield total }
    let iterator = l.GetEnumerator()
    let take n (iter: IEnumerator<'c>)=
        let arr = ResizeArray<'c>(int n)
        let mutable i = 0
        while i < n && iter.MoveNext() do
            arr.Add(iter.Current)
            i <- i + 1
        List.ofSeq arr
    ns |> Seq.choose (fun n ->
            if n > 0 then
                Some (take n iterator)
            else None)

let daysOfWeek (m: Month) (w: Week) = 
    let firstDayOfMonth = DateTime(DateTime.Now.Year, int m, 1)
    if weeksNumberOfMonth firstDayOfMonth < (int w) then
        None
    else
        [ 0 .. DateTime.DaysInMonth(firstDayOfMonth.Year, firstDayOfMonth.Month) - 1 ]
        |> packets (7 - int firstDayOfMonth.DayOfWeek) 7 
        |> Seq.item ((int w) - 1) 
        |> Seq.map (fun n -> firstDayOfMonth.AddDays(double n))
        |> Some

let parseQueryMonthWeek (ctx: HttpContext) =
    let m = ctx.TryBindQueryString<MonthWeek>()
    match m with
    | Ok { m = M; w = W } ->
        match M, W with
        | None, _ -> enum<Month>DateTime.Now.Month, Week.One
        | Some month, None -> month, Week.One
        | Some month, Some week ->
            let firstDayOMonth = DateTime(DateTime.Now.Year, int month, 1)
            month,
            if weeksNumberOfMonth firstDayOMonth < (int week) then
                Week.One
            else
                week
    | Error _ -> enum<Month>DateTime.Now.Month, Week.One


