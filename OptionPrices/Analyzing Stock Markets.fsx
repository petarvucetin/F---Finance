
#load "..\packages\FSharp.Charting\FSharp.Charting.fsx"
#r "..\packages\FSharp.Data\lib\portable-net40+sl5+wp8+win8\FSharp.Data.dll"
#r "..\packages\MathNet.Numerics\lib\portable-net4+sl5+netcore45+wpa81+wp8+MonoAndroid1+MonoTouch1\MathNet.Numerics.dll"

open System
open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra.Double

open FSharp.Data
open FSharp.Data.CsvExtensions

// Create type for accessing Yahoo Finance
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

// Names of stock market indicators
let names = 
  [|"AORD"; "FCHI"; "FTSE"; "GSPTSE"; "MERV"; "MXX"; "NDX"|]

// Download indicator values between 2011 and current date
let indicators = 
  [| for name in names ->
       name, stockData ("^" + name) (DateTime(2012,1,1)) DateTime.Now |]
       
  
// Dates when data for all indices are available
let commonDates = 
  [ for name, index in indicators -> 
      // Return a set with available dates for the current index
      set [ for item in index.Rows -> item.Date ] ]
  |> Set.intersectMany

let matrix = new DenseMatrix( names.Length, (indicators.Length)) //??

// Create a matrix with historical data for available dates
let historicalData = 
  [ for name, index in indicators ->
      [ for item in index.Rows do
          if commonDates.Contains item.Date then
            yield item.Close ] ]
  |> matrix //<- where is this??

// Split the data into training set and validation set
let observedData, futureData = 
  let trainingT = historicalData.ColumnCount * 2 / 3
  historicalData.[0 .. , .. trainingT], 
  historicalData.[0 .. , trainingT + 1 .. ]

// Size of the observedData matrix for future use
let observedCount = observedData.ColumnCount  
let indexCount = observedData.RowCount
