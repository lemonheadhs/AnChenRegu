#load ".paket/load/net46/scripts/Fable.JsonConverter.fsx"
#load "src/AnChenRegu/models.fs"

open System
open Newtonsoft.Json
open AnChenRegu.Models


type Lemon = {
    Id: int
    Name: string
    DateOfBirth: DateTime option
    Period: TimeSpan
}

let lemon = 
    { Id = 12
      Name = "Chandler"
      DateOfBirth = DateTime(1990, 4, 8) |> Some 
      Period = TimeSpan(2000L) }

let fableJson x =
    JsonConvert.SerializeObject(x, [| Fable.JsonConverter() :> JsonConverter |])

let test =
    { LeaveType = SickDay
      Applicant = "Ami"
      Reason = "flu"
      Period = { Start = DateTime.Now.AddHours(-12.); End = DateTime.Now.AddHours(-4.) }
      Amount = TimeSpan(8, 0, 0)
      ApprBy = "Jack"
      MoreComments = null
      Id = 0 }

fableJson test
