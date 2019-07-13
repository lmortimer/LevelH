module Types

open System
open FSharp.AWS.DynamoDB
open FSharp.Data

type BottleList = XmlProvider<"resources/taplist.xml">

type Beer = {
    [<HashKey>]
    Name: string
    Volume: string
    Price: string
    ABV: string
    Country: string
    Description: string
}

type TapList = {
    [<HashKey>]
    AddedOn: DateTimeOffset
    TTL: int64
    Beer: Beer[]
}

let xmlToBeer (item: BottleList.Product) =
    {
        Name = item.Name.String.Value;
        Volume = item.Volume.String.Value;
        Price = item.Price.String.Value;
        ABV = item.Abv.String.Value;
        Country = item.Country.Value;
        Description = item.Description.String.Value;
    }