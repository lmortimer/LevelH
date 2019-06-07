module AppHandlers

open System
open Microsoft.Extensions.Logging
open Giraffe
open Microsoft.AspNetCore.Http
open Common.Dynamo;

let indexHandler  =
    fun (next : HttpFunc) (ctx : HttpContext) ->

        text "Serverless Giraffe Web API" next ctx

let tapHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        
        match getLatestTapList with
        | Some x -> json x next ctx
        | None -> (RequestErrors.NOT_FOUND "Not found" next ctx)
                
let webApp:HttpHandler =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler
                route "/tap" >=> tapHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

