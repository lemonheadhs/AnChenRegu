module AnChenRegu.Models
open System

type LeaveType =
    | AnnualVacation
    | PersonalAffair
    | SickDay
    | CompassionateLeave
    | WeddingLeave
    | MaternityLeave
    | PaternityLeave
    | Other

type TimePoint = {
    Start: DateTime
    End: DateTime
} with
    static member Create s e = 
        if s > e then None
        else Some { Start = s; End = e }

type LeaveRecord = {
    LeaveType: LeaveType
    Applicant: string
    Reason: string
    Period: TimePoint
    Amount: TimeSpan
    ApprBy: string
    MoreComments: string
    Id: int
}

type LateRecord = {
    Employee: string
    Period: TimePoint
    Amount: TimeSpan
    Id: int
}

type OvertimeWork = {
    Employee: string
    Period: TimePoint
    Reason: string
    Id: int
}

// 时间先后的约束
// LeaveRecord 只可以被它发生之前的 OvertimeWork核销
// 


type ClaimableHours = {
    Amount: TimeSpan
    Period: TimePoint
} with
static member (+) (a, b) = 
    let amount = a.Amount + b.Amount
    { 
        Amount = amount; 
        Period = { 
            Start = Seq.min [a.Period.Start; b.Period.Start]
            End = Seq.max [a.Period.End; b.Period.End] 
        } 
    }
static member Zero = { Amount = TimeSpan.Zero; Period = { Start = DateTime.MaxValue; End = DateTime.MinValue } }
end

[<CLIMutable>]
type Person = {
    Name: string
    DisplayName: string
    HireDate: DateTime
}

type AnnualVacationQuota = {
    Employee: string
    Amount: TimeSpan
    Year: int
    Id: int    
}

type LId =
    | LeaveId of int
    | LateId of int

type OId =
    | OvertimeWorkId of int
    | AnnualVacationId of int

type WriteOffHours = {
    LId: LId
    OId: OId
    Hours: ClaimableHours
}

type AccountingHours =
    | WriteOff of WriteOffHours
    | Claimable of ClaimableHours

type AccountingItem =
    | L of LeaveRecord
    | LA of LateRecord
    | O of OvertimeWork
    | A of AnnualVacationQuota


type AccountingAggregate = {
    Record: AccountingItem
    Hours: AccountingHours list
} with
    member this.IsEven =
        if this.Hours |> Seq.exists (function | Claimable _ -> true | _ -> false) then false
        else
            let writeOffs =
                this.Hours
                |> List.choose (function | WriteOff w -> Some w.Hours.Amount | _ -> None)
                |> List.reduce (+)
            let totalAmount = 
                match this.Record with
                | L l -> l.Amount
                | LA la -> la.Amount
                | O o -> o.Period.End - o.Period.Start
                | A a -> a.Amount
            totalAmount = writeOffs
end



