module AnChenRegu.Handlers
open Giraffe
open Giraffe.ModelBinding
open Microsoft.AspNetCore.Http
open Models
open AnChenRegu.DBAccess

let bindJsonWithCheck<'a> (ctx: HttpContext) =
    try
        let result = ctx.BindJsonAsync<'a>()
        Result.Ok result
    with
        | ex -> Result.Error "fail to bind json model"

let saveLeaveRecord (next: HttpFunc) (ctx: HttpContext) =
    let connFactory = ctx.GetService<IConnectionFactory>()
    task {
        match bindJsonWithCheck<LeaveRecord> ctx with
        | Ok r ->
            let! record = r
            let! i = addLeaveRecord record connFactory 
            return! Successful.OK "created" next ctx
        | Error msg -> return! RequestErrors.BAD_REQUEST msg next ctx        
    }


let saveOvertimeWork (next: HttpFunc) (ctx: HttpContext) =
    let connFactory = ctx.GetService<IConnectionFactory>()
    task {
        match bindJsonWithCheck<OvertimeWork> ctx with
        | Ok r ->
            let! record = r
            let! i = addOvertimeWork record connFactory 
            return! Successful.OK "created" next ctx
        | Error msg -> return! RequestErrors.BAD_REQUEST msg next ctx  
    }

