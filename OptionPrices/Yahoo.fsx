//http://www.tryfsharp.org/Learn/financial-computing

#load "..\packages\FSharp.Charting\FSharp.Charting.fsx"
#r "..\packages\FSharp.Data\lib\portable-net40+sl5+wp8+win8\FSharp.Data.dll"
#r "..\packages\MathNet.Numerics\lib\portable-net4+sl5+netcore45+wpa81+wp8+MonoAndroid1+MonoTouch1\MathNet.Numerics.dll"

open System
open FSharp.Data
open FSharp.Data.CsvExtensions
open MathNet.Numerics.Statistics
open MathNet.Numerics.Distributions

type OptionType =
    | Call 
    | Put 

/// Represents information about European option
type OptionInfo = 
    { ExercisePrice : float
      TimeToExpiry : float 
      Kind : OptionType }

/// Represents price and statistics about stock 
type StockInfo = 
    { Volatility : float
      CurrentPrice : float }

let urlFor ticker (startDate:DateTime) (endDate:DateTime) = 
  let root = "http://ichart.finance.yahoo.com/table.csv"
  sprintf "%s?s=%s&a=%i&b=%i&c=%i&d=%i&e=%i&f=%i" root ticker 
          (startDate.Month - 1) startDate.Day startDate.Year 
          (endDate.Month - 1) endDate.Day endDate.Year

[<Literal>]
let schemaUrl = "http://ichart.finance.yahoo.com/table.csv?s=MSFT"

type Stocks = CsvProvider<schemaUrl>

let stockData ticker startDate endDate = 
    Stocks.Load((urlFor ticker startDate endDate))

let randomPrice drift volatility dt initial (dist:Normal) = 
    // Calculate parameters of the exponential
    let driftExp = (drift - 0.5 * pown volatility 2) * dt
    let randExp = volatility * (sqrt dt)

    // Recursive loop that actually generates the price
    let rec loop price = seq {
        yield price
        let price = price * exp (driftExp + randExp * dist.Sample()) 
        yield! loop price }

    // Return path starting at 'initial'
    loop initial

//let recentPrices symbol =
//  let data = stockData symbol (DateTime(2012,1,1)) DateTime.Now
//  [ for row in data.Rows -> row.Date.DayOfYear, Convert.ToDouble(row.Close) ]

//let prices = recentPrices "AAPL"

let ytdPrices = stockData "MSFT" (DateTime.Now - TimeSpan.FromDays(252.)) DateTime.Now
let first = ytdPrices.Rows |> Seq.minBy (fun itm -> itm.Date)
let last = ytdPrices.Rows |> Seq.maxBy (fun itm -> itm.Date)
let firstClose = Convert.ToDouble(first.Close)
let lastClose = Convert.ToDouble(last.Close)


let logRatios = 
    ytdPrices.Rows 
    |> Seq.sortBy (fun v -> v.Date)
    |> Seq.pairwise
    |> Seq.map (fun (prev, next) -> log ( Convert.ToDouble(next.Close) / Convert.ToDouble(prev.Close)))

let normal = Normal()

let blackScholes rate stock option =
    // We can only calculate if the option concerns the future
    if option.TimeToExpiry > 0.0 then
        // Calculate d1 and d2 and pass them to cumulative distribution
        let d1 = 
            ( log(stock.CurrentPrice / option.ExercisePrice) + 
                (rate + 0.5 * pown stock.Volatility 2) * option.TimeToExpiry ) /
            ( stock.Volatility * sqrt option.TimeToExpiry )
        let d2 = d1 - stock.Volatility * sqrt option.TimeToExpiry
        let N1 = normal.CumulativeDistribution(d1)
        let N2 = normal.CumulativeDistribution(d2)

        // Calculate the call option (and derived put option) price
        let e = option.ExercisePrice * exp (-rate * option.TimeToExpiry)
        let call = stock.CurrentPrice * N1 - e * N2
        match option.Kind with
        | Call -> call
        | Put -> call + e - stock.CurrentPrice
    else
        // If the option has expired, calculate payoff directly
        match option.Kind with
            | Call -> max (stock.CurrentPrice - option.ExercisePrice) 0.0
            | Put -> max (option.ExercisePrice - stock.CurrentPrice) 0.0
// Use Math.NET to obtain descriptive statistics
let stats = DescriptiveStatistics(logRatios)

// Represents one day in a year with 252 trading days
let tau = 1.0 / 252.0

// Calculate volatility and drift from the above equations
let volatility = sqrt (stats.Variance / tau)
let drift = (stats.Mean / tau) + (pown volatility 2) / 2.0

let msftOption = { ExercisePrice = 30.0; TimeToExpiry = 1.0; Kind = Call }

blackScholes 0.05 {CurrentPrice=lastClose; Volatility=volatility} msftOption