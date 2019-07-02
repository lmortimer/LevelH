﻿module AppHandlers

open System
open Microsoft.AspNetCore.Http

open Amazon.DynamoDBv2
open Amazon
open Amazon.Runtime
open Giraffe
open FSharp.AWS.DynamoDB
open FSharp.Data

open Common.Types;


let client = new AmazonDynamoDBClient()
let table = TableContext.Create<TapList>(client, tableName = "hashigo-taps", createIfNotExists = true)

// required for local development
//let client = new AmazonDynamoDBClient(new StoredProfileAWSCredentials(), RegionEndpoint.USEast1)
//let table = TableContext.Create<TapList>(client, tableName = "hashigo-taps", createIfNotExists = false)


let getTapData = 
    Http.RequestString "https://www.hashigozake.co.nz/taplist.xml"
    //"""<Products Timestamp="14:12 on 14/05/2019"><LastChange><Off>Hallertau Red IPA</Off><On>Brewaucracy Litany of Lies #4</On></LastChange><Beers><Product><Name></Name><Volume></Volume><Price></Price><Image></Image><ABV></ABV><Handpump>No</Handpump><Country></Country><Brewery></Brewery><Description></Description><QuestId>0</QuestId><KegVolume>0.000000</KegVolume><Pouring>Now</Pouring></Product><Product><Name>#100</Name><Volume>200ml/300ml</Volume><Price>$10/$12</Price><Image>nogne.png</Image><ABV>10%</ABV><Handpump>No</Handpump><Country>Norway</Country><Brewery>Nogne O</Brewery><Description>Barley Wine</Description><QuestId>0</QuestId><KegVolume>0.000000</KegVolume><Pouring>No</Pouring></Product><Product><Name>#500 IIPA</Name><Volume>200ml/300ml</Volume><Price>$10/$13</Price><Image>nogne.png</Image><ABV>10%</ABV><Handpump>No</Handpump><Country>Norway</Country><Brewery>Nogne O</Brewery><Description>Imperial IPA</Description><QuestId>0</QuestId><KegVolume>0.000000</KegVolume><Pouring>Now</Pouring></Product><Product><Name>#6 Session IPA</Name><Volume>300ml/473ml</Volume><Price>$8/$10.5</Price><Image>hallertau.png</Image><ABV>3.8%</ABV><Handpump>No</Handpump><Country>New Zealand</Country><Brewery>Hallerau</Brewery><Description></Description><QuestId>0</QuestId><KegVolume>0.000000</KegVolume><Pouring>No</Pouring></Product><Product><Name>'Murica</Name><Volume>300ml/440ml</Volume><Price>$9.5/$12</Price><Image>behemoth.png</Image><ABV>6%</ABV><Handpump>No</Handpump><Country>NZ</Country><Brewery>Behemoth</Brewery><Description>APA</Description><QuestId>0</QuestId><KegVolume>0.000000</KegVolume><Pouring>No</Pouring></Product></Beers></Products>"""
    

let indexHandler  =
    fun (next : HttpFunc) (ctx : HttpContext) ->

        let data = BottleList.Parse(getTapData)

       // IsSome to get rid of empty elements
        let pouring = 
            data.Beers.Products
            |> Array.filter (fun x -> x.Name.String.IsSome) // get rid of any empty elements
            |> Array.filter (fun x -> x.Pouring.String.Value = "Now")
            |> Array.map xmlToBeer

        let tapList = {Beer = pouring; AddedOn = DateTimeOffset.Now}
        
        table.PutItem tapList |> ignore
        
        text "Beer list updated" next ctx


let tapHandler: HttpHandler = 
    fun (next: HttpFunc) (ctx: HttpContext) ->

        let data = 
            table.Scan() 
            |> Array.toList
            |> List.sortByDescending (fun x -> x.AddedOn)

        json data next ctx
        

let webApp:HttpHandler =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler
                route "/tap" >=> tapHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]