namespace Common.Types

open System
open FSharp.AWS.DynamoDB

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
    [<RangeKey>]
    AddedOn: DateTimeOffset
    Beer: Beer[]
}