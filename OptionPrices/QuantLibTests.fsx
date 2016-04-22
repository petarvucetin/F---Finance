#r @"..\packages\NQuantLib.dll\lib\net\NQuantLib.dll"

open System.Net
open FSharp.Data
open System.Text.RegularExpressions
open System
open QuantLib 

let T = 3.0

let r, divYield, vol = 0.01, 0.03, 0.5

let stdev = vol* sqrt T

let discount = exp (-r*T)

let spot = 100.0

let forward = spot * exp ((r-divYield)*T)

let strikes = [|10.0..10.0..200.0|]


for strike in strikes do
    use payoff = new PlainVanillaPayoff(Option.Type.Call,strike)
    use bcalculator = new BlackCalculator(payoff,forward,stdev,discount)
    printfn "strike: %.5f, price: %.5f" strike (bcalculator.value())

