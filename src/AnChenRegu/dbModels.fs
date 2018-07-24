module AnChenRegu.DBModels
open System

[<CLIMutable>]
type TB_Leave = {
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

[<CLIMutable>]
type TB_Late = {
    Id: int
    Employee: string
    StartTime: DateTime
    EndTime: DateTime
    Amount: string
}

[<CLIMutable>]
type TB_OvertimeWork = {
    Id: int
    Employee: string
    StartTime: DateTime
    EndTime: DateTime
    Reason: string    
}

[<CLIMutable>]
type TB_AnnualVacationQuota = {
    Id: int
    Employee: string
    Amount: string
    Year: int
}

[<CLIMutable>]
type TB_WriteOffHours = {
    SourceId: int
    SourceKind: string
    DestId: int
    DestKind: string
    Amount: string
    StartTime: DateTime
    EndTime: DateTime
}
