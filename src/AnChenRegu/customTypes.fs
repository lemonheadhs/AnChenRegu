module AnChenRegu.CustomTypes
open System
open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive

type Errors = 
    | ValidationError of string
    | DbError of Exception
    | IOError of Exception
    | NetworkError of Exception

module TaskResult =

    type TaskResult<'a, 'b> = Task<Result<'a, 'b>>

    let bind (v: TaskResult<'a, 'b>) (f: 'a -> TaskResult<'c, 'b>) =
        task {
            let! r = v
            match r with
            | Error er -> return Error er
            | Ok c -> return! f c
        }

    let map (f: 'a -> 'c) (v: TaskResult<'a, 'b>) =
        task {
            let! r = v
            match r with
            | Error er -> return Error er
            | Ok c -> return Ok (f c)
        }

    let mapError (f: 'b -> 'c) (v: TaskResult<'a, 'b>) =
        task {
            let! r = v
            match r with
            | Error er -> return Error (f er)
            | Ok c -> return Ok c
        }

    let toTaskBi (f1: 'a -> 'c) (f2: 'b -> 'c) (v: TaskResult<'a, 'b>) =
        task {
            let! r = v
            match r with
            | Error er -> return f2 er
            | Ok c -> return f1 c
        }

    let returnError v =
        task { return Error v }

    type TaskResultBuilder() =
        member __.Bind(v, f) = bind v f
        member __.Return v = task { return Ok v }

    let taskResult = TaskResultBuilder()
