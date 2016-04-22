//http://www.google.com/finance/option_chain?q=AAPL&output=json

#r "..\packages\FSharp.Data\lib\portable-net40+sl5+wp8+win8\FSharp.Data.dll"
#r @"..\packages\NQuantLib.dll\lib\net\NQuantLib.dll"

#r "System.Net"

#load "Helpers.fsx"

open System.Net
open FSharp.Data
open System.Text.RegularExpressions
open FSharp.Data.JsonExtensions
open Helpers
open System

type optionType = 
    | Put
    | Call


type OptionInfo = {
        Type:optionType; 
        Strike:decimal; 
        Expire:int; 
        Bid:decimal; 
        Ask:decimal; 
        Mid:decimal;
        Volume:int;
        OpenInterest:int;
    }

let stockUrl stockSymbol = "https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.quotes%20where%20symbol%20%3D%20%22"+stockSymbol+"%22%0A%09%09&format=json&env=http%3A%2F%2Fdatatables.org%2Falltables.env&callback="

type stock = JsonProvider<"..\yahooStockInfo.json">

type option = JsonProvider<"..\googleOptions.json">

let decimalOrZero (v:option.DecimalOrString) = 
    match v.Number with
        | Some(v) -> v
        | None -> 0m

let intOrZero (v:option.IntOrString) = 
    match v.Number with
        | Some(v) -> v
        | None -> 0


let fixJSON data =
    let f = Regex.Replace (data, "(\w+:)(\d+\.?\d*)", @"$1""$2""") 
    Regex.Replace (f, @"(\w+):", "\"$1\":")

let optionData (symbol:string, year:int, month:int, day:int) = 
    let optionSpecificExpiry stockSymbol year month day =  "http://www.google.com/finance/option_chain?q="+stockSymbol+"&output=json&expy="+year.ToString()+"&expm="+month.ToString()+"&expd="+day.ToString()+"'"
    Http.RequestString ( (optionSpecificExpiry symbol year month day) , headers=["content-type", "application/json"] )
    |> fixJSON
    |> option.Parse

let converter (o:option.Put seq)  =
    o 
    |> Seq.map (fun f -> 
                    {
                        Type= Call; 
                        Strike=f.Strike; 
                        Expire=Int32.Parse(f.Expiry.ToString("yyMMdd")); 
                        Bid=(decimalOrZero f.B); 
                        Ask=f.A;
                        Volume= (intOrZero f.Vol); 
                        Mid = ((decimalOrZero f.B)+f.A) / 2.0m;
                        OpenInterest= f.Oi
                     })

let data symbol = 
    let allOptionsUrl stockSymbol =  "http://www.google.com/finance/option_chain?q="+stockSymbol+"&output=json"   
    let x = Http.RequestString( (allOptionsUrl symbol), headers=["content-type", "application/json"] )
            |> fixJSON
            |> option.Parse
    x.Expirations
        |> Seq.collect (fun f -> 
                             let x = optionData (symbol,f.Y,f.M,f.D) 

                             let c =  x.Calls |> converter
                             let p =  x.Puts |> converter
                             [c; p] |> Seq.concat)
        

        |> Seq.filter (fun f -> f.Volume > 500) 

let optionPrice = data "AAPL"
let stockInfo = stock.Load(stockUrl "AAPL")

let verticals ( z: (decimal * decimal * int)[] ) distance = 
   seq { for x in 0..distance..z.Length - 1 do
           if ( x + distance <  z.Length - 1 ) then
               let a,b,c = z.[x]
               let d,e,f = z.[x+distance]
               yield ( c, a, d ,  b + e / 2.0m )
       }

verticals optionPrice 2
