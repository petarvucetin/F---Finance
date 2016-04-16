//http://www.google.com/finance/option_chain?q=AAPL&output=json

#r "..\packages\FSharp.Data\lib\portable-net40+sl5+wp8+win8\FSharp.Data.dll"
#r "System.Net"

#load "Helpers.fsx"

open System.Net
open FSharp.Data
open System.Text.RegularExpressions
open FSharp.Data.JsonExtensions
open Helpers

//type optionPrices = JsonProvider<"http://www.google.com/finance/option_chain?q=AAPL&output=json">
//

let stockUrl stockSymbol = "https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.quotes%20where%20symbol%20%3D%20%22"+stockSymbol+"%22%0A%09%09&format=json&env=http%3A%2F%2Fdatatables.org%2Falltables.env&callback="
let allOptionsUrl stockSymbol =  "http://www.google.com/finance/option_chain?q="+stockSymbol+"&output=json"
let options2Url stockSymbol year month day =  "http://www.google.com/finance/option_chain?q="+stockSymbol+"&output=json&expy="+year+"&expm="+month+"&expd="+day+"'"

let data = 
  Http.RequestString
    ( (allOptionsUrl "AAPL"), 
      headers=["content-type", "application/json"] )

let fixJSON data =
    let f = Regex.Replace (data, "(\w+:)(\d+\.?\d*)", @"$1""$2""") 
    Regex.Replace (f, @"(\w+):", "\"$1\":")

type stock = JsonProvider<"..\yahooStockInfo.json">

type option = JsonProvider<"..\googleOptions.json">


let volFilter (f:option.IntOrString) = 
    match f.Number with
        | Some f -> f > 500
        | None -> false

let decimalOrZero (v:option.DecimalOrString) = 
    match v.Number with
        | Some(v) -> v
        | None -> 0m


let financials stockSymbol = 
    let optionPrice = option.Parse(fixJSON data)
    let stockInfo = stock.Load(stockUrl "aapl")
    let calls = optionPrice.Calls
                |> Seq.filter (fun f-> volFilter f.Vol) 
                |> Seq.map (fun f -> (f.Strike, (Helpers.TryParser.parseInt (f.Expiry.ToString("yyMMdd"))), (decimalOrZero f.B), f.A) )
    let puts = optionPrice.Puts
                |> Seq.filter (fun f-> volFilter f.Vol) 
                |> Seq.map (fun f -> (f.Strike, (Helpers.TryParser.parseInt (f.Expiry.ToString("yyMMdd"))), (decimalOrZero f.B), f.A) )
    (optionPrice.UnderlyingPrice, calls, puts,stockInfo)

financials "AAPl"
