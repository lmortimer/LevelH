module AppHandlers

open System
open System.IO
open Microsoft.AspNetCore.Http

open Amazon.DynamoDBv2
open Giraffe
open FSharp.AWS.DynamoDB
open FSharp.Data

open Types;

(*
let client = new AmazonDynamoDBClient()
let table = TableContext.Create<TapList>(client, tableName = "hashigo-taps", createIfNotExists = true)
 
let getTapData = 
    Http.RequestString "https://www.hashigozake.co.nz/taplist.xml"

// required for local development

*)
open Amazon
open Amazon.Runtime
let client = new AmazonDynamoDBClient(new StoredProfileAWSCredentials(), RegionEndpoint.USEast1)
let table = TableContext.Create<TapList>(client, tableName = "hashigo-taps", createIfNotExists = false)

let getTapData =
    File.ReadAllText("resources/taplist.xml")

    
/// fetches the latest copy of the beer list, selects the beers which are now pouring, and inserts those into dynamo
let updateTapListHandler  =
    fun (next : HttpFunc) (ctx : HttpContext) ->

        let parsedTapList = BottleList.Parse(getTapData)

        let pouring = 
            parsedTapList.Beers.Products
            |> Array.filter (fun x -> x.Name.String.IsSome) // get rid of any empty elements
            |> Array.filter (fun x -> x.Pouring.String.Value = "Now")
            |> Array.map xmlToBeer

        let tapList = {
            Beer = pouring; 
            AddedOn = DateTimeOffset.Now;
            TTL = DateTimeOffset.Now.AddHours(48.0).ToUnixTimeSeconds()
        }
        
        table.PutItem tapList |> ignore
        
        text "Beer list updated" next ctx


let latestTapListHandler: HttpHandler = 
    fun (next: HttpFunc) (ctx: HttpContext) ->

        let latetTapList = 
            table.Scan() 
            |> Array.toList
            |> List.sortByDescending (fun x -> x.AddedOn)
            |> List.tryHead

        match latetTapList with
        | Some tapList -> json tapList next ctx
        | None -> json obj next ctx
        

let webApp:HttpHandler =
    choose [
        route "/tap" >=> choose [
            GET >=> latestTapListHandler
            POST >=> updateTapListHandler
        ]

        setStatusCode 404 >=> text "Not Found" ]