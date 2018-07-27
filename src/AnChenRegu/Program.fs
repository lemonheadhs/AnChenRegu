module AnChenRegu.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Handlers
open DBAccess
open CustomTypes
open Microsoft.Extensions.Configuration

// ---------------------------------
// Models
// ---------------------------------

type Message =
    {
        Text : string
    }

// ---------------------------------
// Views
// ---------------------------------

module Views =
    open GiraffeViewEngine

    let layout (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText "AnChenRegu" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] content
        ]

    let partial () =
        h1 [] [ encodedText "AnChenRegu" ]

    let index (model : Message) =
        [
            partial()
            p [] [ encodedText model.Text ]
        ] |> layout

// ---------------------------------
// Web app
// ---------------------------------

let indexHandler (name : string) =
    let greetings = sprintf "Hello %s, from Giraffe!" name
    let model     = { Text = greetings }
    let view      = Views.index model
    htmlView view

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler "world"
                routef "/hello/%s" indexHandler
                subRoute "/api" 
                    (choose [
                        subRoute "/overview" (text "")
                        subRoute "/employee/name" (text "")
                        subRoute "/employee/name/leaveRecord" (text "")
                        subRoute "/employee/name/leaveRecord/id" (text "")
                        subRoute "/employee/name/overtimeWork" (text "")
                        subRoute "/employee/name/overtimeWork/id" (text "")
                    ])
            ]
        POST >=> subRoute "/api"
            (choose [
                subRoute "/admin"
                    (choose [
                        route "/leaveRecord/add" >=> saveLeaveRecord
                        route "/overtimeWork/add" >=> saveOvertimeWork
                        route "/lateRecord/add" >=> saveLateRecord
                        route "/writeOffHours/add" >=> saveWriteOffHours
                    ])
            ])
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let appConfigSource (ctx: WebHostBuilderContext) (config: IConfigurationBuilder) =
    let env = ctx.HostingEnvironment

    config.AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
          .AddJsonFile("appsettings." + env.EnvironmentName + ".json", optional = true, reloadOnChange = true)
          .Build() |> ignore
    

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (ctx: WebHostBuilderContext) (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore
    services.AddOptions() |> ignore
    services.Configure<DatabaseConfig>("database", ctx.Configuration) |> ignore
    services.AddTransient<IConnectionFactory, ConnectionFactory>() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .ConfigureAppConfiguration(appConfigSource)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0