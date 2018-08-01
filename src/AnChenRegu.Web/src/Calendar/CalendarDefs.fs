// ts2fable 0.6.1
module rec ReactBigCalendar
open System
open Fable.Core
open Fable.Import.JS
open Fable.Import.Browser
open Fable.Helpers.React
open Fable.Import.React

type [<AllowNullLiteral>] IExports =
    abstract BigCalendar: BigCalendarStatic

type stringOrDate =
    U2<string, DateTime>

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module stringOrDate =
    let ofString v: stringOrDate = v |> U2.Case1
    let isString (v: stringOrDate) = match v with U2.Case1 _ -> true | _ -> false
    let asString (v: stringOrDate) = match v with U2.Case1 o -> Some o | _ -> None
    let ofDateTime v: stringOrDate = v |> U2.Case2
    let isDateTime (v: stringOrDate) = match v with U2.Case2 _ -> true | _ -> false
    let asDateTime (v: stringOrDate) = match v with U2.Case2 o -> Some o | _ -> None

type [<StringEnum>] [<RequireQualifiedAccess>] View =
    | Month
    | Week
    | Work_week
    | Day
    | Agenda

type [<StringEnum>] [<RequireQualifiedAccess>] Navigate =
    | [<CompiledName "PREV">] PREV
    | [<CompiledName "NEXT">] NEXT
    | [<CompiledName "TODAY">] TODAY
    | [<CompiledName "DATE">] DATE

type Event =
    obj

type [<AllowNullLiteral>] Format =
    /// Format for the day of the month heading in the Month view.
    /// e.g. "01", "02", "03", etc
    abstract dateFormat: string option with get, set
    /// A day of the week format for Week and Day headings,
    /// e.g. "Wed 01/04"
    abstract dayFormat: string option with get, set
    /// Week day name format for the Month week day headings,
    /// e.g: "Sun", "Mon", "Tue", etc
    abstract weekdayFormat: string option with get, set
    /// The timestamp cell formats in Week and Time views, e.g. "4:00 AM"
    abstract timeGutterFormat: string option with get, set
    /// Toolbar header format for the Month view, e.g "2015 April"
    abstract monthHeaderFormat: string option with get, set
    /// Toolbar header format for the Week views, e.g. "Mar 29 - Apr 04"
    abstract dayRangeHeaderFormat: string option with get, set
    /// Toolbar header format for the Day view, e.g. "Wednesday Apr 01"
    abstract dayHeaderFormat: string option with get, set
    /// Toolbar header format for the Agenda view, e.g. "4/1/2015 — 5/1/2015"
    abstract agendaHeaderFormat: string option with get, set
    /// A time range format for selecting time slots, e.g "8:00am — 2:00pm"
    abstract selectRangeFormat: string option with get, set
    abstract agendaDateFormat: string option with get, set
    abstract agendaTimeFormat: string option with get, set
    abstract agendaTimeRangeFormat: string option with get, set
    /// Time range displayed on events.
    abstract eventTimeRangeFormat: string option with get, set
    /// An optional event time range for events that continue onto another day
    abstract eventTimeRangeStartFormat: string option with get, set
    /// An optional event time range for events that continue from another day
    abstract eventTimeRangeEndFormat: string option with get, set

type [<AllowNullLiteral>] HeaderProps =
    abstract culture: BigCalendarProps with get, set
    abstract date: DateTime with get, set
    abstract format: string with get, set
    abstract label: string with get, set
    abstract localizer: obj with get, set

type [<AllowNullLiteral>] Components =
    abstract ``event``: U4<React.SFC, React.Component, React.ComponentClass, JSX.Element> option with get, set
    abstract eventWrapper: U4<React.SFC, React.Component, React.ComponentClass, JSX.Element> option with get, set
    abstract dayWrapper: U4<React.SFC, React.Component, React.ComponentClass, JSX.Element> option with get, set
    abstract dateCellWrapper: U4<React.SFC, React.Component, React.ComponentClass, JSX.Element> option with get, set
    /// component used as a header for each column in the TimeGridHeader
    abstract header: React.ComponentType<HeaderProps> option with get, set
    abstract toolbar: U4<React.SFC, React.Component, React.ComponentClass, JSX.Element> option with get, set
    abstract agenda: obj option with get, set
    abstract day: obj option with get, set
    abstract week: obj option with get, set
    abstract month: obj option with get, set

type [<AllowNullLiteral>] Messages =
    abstract date: string option with get, set
    abstract time: string option with get, set
    abstract ``event``: string option with get, set
    abstract allDay: string option with get, set
    abstract week: string option with get, set
    abstract work_week: string option with get, set
    abstract day: string option with get, set
    abstract month: string option with get, set
    abstract previous: string option with get, set
    abstract next: string option with get, set
    abstract yesterday: string option with get, set
    abstract tomorrow: string option with get, set
    abstract today: string option with get, set
    abstract agenda: string option with get, set
    abstract showMore: (float -> string) option with get, set

type BigCalendarProps =
    BigCalendarProps<obj>

type [<AllowNullLiteral>] BigCalendarProps<'T> =
    inherit React.Props<BigCalendar<'T>>
    abstract date: stringOrDate option with get, set
    abstract now: DateTime option with get, set
    abstract view: View option with get, set
    abstract events: ResizeArray<'T> option with get, set
    abstract onNavigate: (DateTime -> Navigate -> unit) option with get, set
    abstract onView: (View -> unit) option with get, set
    abstract onDrillDown: (DateTime -> View -> unit) option with get, set
    abstract onSelectSlot: (obj -> unit) option with get, set
    abstract onDoubleClickEvent: ('T -> React.SyntheticEvent<HTMLElement> -> unit) option with get, set
    abstract onSelectEvent: ('T -> React.SyntheticEvent<HTMLElement> -> unit) option with get, set
    abstract onSelecting: (obj -> bool option) option with get, set
    abstract selected: obj option with get, set
    abstract views: U2<ResizeArray<View>, obj> option with get, set
    abstract drilldownView: View option with get, set
    abstract getDrilldownView: (DateTime -> View -> ResizeArray<View> -> unit) option with get, set
    abstract length: float option with get, set
    abstract toolbar: bool option with get, set
    abstract popup: bool option with get, set
    abstract popupOffset: U2<float, obj> option with get, set
    abstract selectable: U2<bool, string> option with get, set
    abstract longPressThreshold: float option with get, set
    abstract step: float option with get, set
    abstract timeslots: float option with get, set
    abstract rtl: bool option with get, set
    abstract eventPropGetter: ('T -> stringOrDate -> stringOrDate -> bool -> obj) option with get, set
    abstract slotPropGetter: (DateTime -> obj) option with get, set
    abstract dayPropGetter: (DateTime -> obj) option with get, set
    abstract showMultiDayTimes: bool option with get, set
    abstract min: stringOrDate option with get, set
    abstract max: stringOrDate option with get, set
    abstract scrollToTime: DateTime option with get, set
    abstract culture: string option with get, set
    abstract formats: Format option with get, set
    abstract components: Components option with get, set
    abstract messages: Messages option with get, set
    abstract titleAccessor: U2<obj, ('T -> string)> option with get, set
    abstract allDayAccessor: U2<obj, ('T -> bool)> option with get, set
    abstract startAccessor: U2<obj, ('T -> DateTime)> option with get, set
    abstract endAccessor: U2<obj, ('T -> DateTime)> option with get, set
    abstract resourceAccessor: U2<obj, ('T -> obj option)> option with get, set
    abstract resources: ResizeArray<obj option> option with get, set
    abstract resourceIdAccessor: U2<obj, ('T -> obj option)> option with get, set
    abstract resourceTitleAccessor: U2<obj, ('T -> string)> option with get, set
    abstract defaultView: View option with get, set
    abstract defaultDate: DateTime option with get, set
    abstract className: string option with get, set
    abstract elementProps: React.HTMLAttributes<HTMLElement> option with get, set

type BigCalendar =
    BigCalendar<obj>

type [<AllowNullLiteral>] BigCalendar<'T> =
    inherit React.Component<BigCalendarProps<'T>>

type [<AllowNullLiteral>] BigCalendarStatic =
    [<Emit "new $0($1...)">] abstract Create: unit -> BigCalendar<'T>
    /// Setup the localizer by providing the moment Object
    abstract momentLocalizer: momentInstance: obj -> unit
    /// Setup the localizer by providing the globalize Object
    abstract globalizeLocalizer: globalizeInstance: obj -> unit