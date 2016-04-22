//http://www.google.com/finance/option_chain?q=AAPL&output=json

#r "..\packages\FSharp.Data\lib\portable-net40+sl5+wp8+win8\FSharp.Data.dll"
#r @"..\packages\NQuantLib.dll\lib\net\NQuantLib.dll"

#r "System.Net"
#r "System.Windows.Forms"
#r "System.Drawing"
#r @"WindowsBase.dll"
#r @"PresentationCore.dll"
#r @"PresentationFramework.dll"

#load "Helpers.fsx"

open System.Net
open FSharp.Data
open System.Text.RegularExpressions
open FSharp.Data.JsonExtensions
open Helpers
open System
open System.Drawing
open System.Windows
open System.Windows.Forms
open System.Windows.Controls


type Wrapper(s:obj) = 
  member x.Value = s.ToString()

let grid<'T> (x:seq<'T>) =     
  let defaultFont = new Font( "Consolas", 16.0f)
  let form = new Form(Visible = true)    
  let data = new DataGridView(Dock = DockStyle.Fill)
  data.DefaultCellStyle.Font <- defaultFont
  form.Controls.Add(data)
  data.AutoGenerateColumns <- true
  if typeof<'T>.IsPrimitive || typeof<'T> = typeof<string> then
    data.DataSource <- [| for v in x -> Wrapper(box v) |]
  else 
    data.DataSource <- x |> Seq.toArray

let  wpfGrid<'T> (x:seq<'T>) = 
    let win = new Window(Title="Test DataGrid")
    win.FontSize <- 16.0
    win.FontFamily <- new Media.FontFamily("Consolas")
    let datagrid = DataGrid()
    datagrid.HeadersVisibility <- DataGridHeadersVisibility.Column
    datagrid.ItemsSource <- x |> Seq.toArray
    win.Content <- new ScrollViewer(Content=datagrid)
    win.Show()

type optionType = 
    | Put
    | Call


type OptionInfo = {
        Type:optionType; 
        Id:string;
        Strike:decimal; 
        Expire:int; 
        Bid:decimal; 
        Ask:decimal; 
        Mid:decimal;
        Volume:int;
        OpenInterest:int;
    }

let verticals ( optionList: OptionInfo seq) distance = 
    let z = optionList |> Seq.toArray 
    seq { for x in 0..distance..z.Length - 1 do
            if ( x + distance <  z.Length - 1 ) then
                yield ( z.[x].Expire, z.[x].Strike, z.[x+distance].Strike ,  (z.[x].Mid + z.[x+distance].Mid) / 2.0m )
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
    let f1 = Regex.Replace (data, "(\w+:)(\d+\.?\d*)", @"$1""$2""") 
    let f2 = Regex.Replace (f1, @"(\w+):", "\"$1\":")
    Regex.Replace (f2, @"""-""", """0""")

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
                        Id = f.S;
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

let optionPrice = data "AAPL"
let stockInfo = stock.Load(stockUrl "AAPL")

grid optionPrice 
//wpfGrid optionPrice 

grid (verticals optionPrice 2)
//wpfGrid (verticals optionPrice 2)
