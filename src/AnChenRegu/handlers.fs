module AnChenRegu.Handlers
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

type MonthWeek = {
    name: string
}



