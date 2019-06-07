module Common.Dynamo

open System
open FSharp.Data
open FSharp.AWS.DynamoDB
open Amazon.DynamoDBv2
open Amazon
open Amazon.Runtime
open Common.Types

type BottleList = XmlProvider<"../Common/resources/taplist.xml">

let client = new AmazonDynamoDBClient(new StoredProfileAWSCredentials(), RegionEndpoint.USEast1)
let table = TableContext.Create<TapList>(client, tableName = "hashigo-taps", createIfNotExists = true)

let xmlToBeer (item: BottleList.Product) =
    {
        Name = item.Name.String.Value;
        Volume = item.Volume.String.Value;
        Price = item.Price.String.Value;
        ABV = item.Abv.String.Value;
        Country = item.Country.Value;
        Description = item.Description.String.Value;
    }


let getTapData = 
    Http.RequestString "https://www.hashigozake.co.nz/taplist.xml"
    //"""<Products Timestamp="14:12 on 14/05/2019"><LastChange><Off>Hallertau Red IPA</Off><On>Brewaucracy Litany of Lies #4</On></LastChange><Beers><Product><Name></Name><Volume></Volume><Price></Price><Image></Image><ABV></ABV><Handpump>No</Handpump><Country></Country><Brewery></Brewery><Description></Description><QuestId>0</QuestId><KegVolume>0.000000</KegVolume><Pouring>Now</Pouring></Product><Product><Name>#100</Name><Volume>200ml/300ml</Volume><Price>$10/$12</Price><Image>nogne.png</Image><ABV>10%</ABV><Handpump>No</Handpump><Country>Norway</Country><Brewery>Nogne O</Brewery><Description>Barley Wine</Description><QuestId>0</QuestId><KegVolume>0.000000</KegVolume><Pouring>No</Pouring></Product><Product><Name>#500 IIPA</Name><Volume>200ml/300ml</Volume><Price>$10/$13</Price><Image>nogne.png</Image><ABV>10%</ABV><Handpump>No</Handpump><Country>Norway</Country><Brewery>Nogne O</Brewery><Description>Imperial IPA</Description><QuestId>0</QuestId><KegVolume>0.000000</KegVolume><Pouring>Now</Pouring></Product><Product><Name>#6 Session IPA</Name><Volume>300ml/473ml</Volume><Price>$8/$10.5</Price><Image>hallertau.png</Image><ABV>3.8%</ABV><Handpump>No</Handpump><Country>New Zealand</Country><Brewery>Hallerau</Brewery><Description></Description><QuestId>0</QuestId><KegVolume>0.000000</KegVolume><Pouring>No</Pouring></Product><Product><Name>'Murica</Name><Volume>300ml/440ml</Volume><Price>$9.5/$12</Price><Image>behemoth.png</Image><ABV>6%</ABV><Handpump>No</Handpump><Country>NZ</Country><Brewery>Behemoth</Brewery><Description>APA</Description><QuestId>0</QuestId><KegVolume>0.000000</KegVolume><Pouring>No</Pouring></Product></Beers></Products>"""
    
let fetchAndSaveTapList (): unit =
    let data = BottleList.Parse(getTapData)

    // IsSome to get rid of empty elements
    let pouring = 
        data.Beers.Products
        |> Array.filter (fun x -> x.Name.String.IsSome) // get rid of any empty elements
        |> Array.filter (fun x -> x.Pouring.String.Value = "Now")
        |> Array.map xmlToBeer

    let tapList = {Beer = pouring; AddedOn = DateTimeOffset.Now}
    
    printfn "Taplist before saving is"
    printfn "%A" tapList
    
    table.PutItem tapList |> ignore
    
let getLatestTapList =
    table.Scan()
    |> Array.toList
    |> List.sortByDescending (fun x -> x.AddedOn)
    |> List.tryHead