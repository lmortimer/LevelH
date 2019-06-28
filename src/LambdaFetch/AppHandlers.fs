module AppHandlers

open System
open Microsoft.Extensions.Logging
open Giraffe
open Microsoft.AspNetCore.Http
open Common.Dynamo;
open Common.Types;
open FSharp.AWS.DynamoDB
open Amazon.DynamoDBv2
open Amazon
open Amazon.Runtime

let client = new AmazonDynamoDBClient()
let table = TableContext.Create<TapList>(client, tableName = "hashigo-taps", createIfNotExists = true)

let indexHandler  =
    fun (next : HttpFunc) (ctx : HttpContext) ->

        text "Serverless Giraffe Web API" next ctx


let tapHandler: HttpHandler = 
    fun (next: HttpFunc) (ctx: HttpContext) ->

        let data = 
            table.Scan() 
            |> Array.toList
            |> List.sortByDescending (fun x -> x.AddedOn)
            |> List.tryHead

        match data with
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

