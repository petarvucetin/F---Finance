#r @"..\packages\NQuantLib.dll\lib\net\NQuantLib.dll"

open System.Net
open FSharp.Data
open System.Text.RegularExpressions
open System
open QuantLib 




//dates
let valuation_date = new QuantLib.Date(4,Month.April,2016) 
let expiry_date = new QuantLib.Date(17,Month.May,2016)

//terms and conditions
let strike_price = 108.0
let put_or_call = Option.Type.Call

//#market data
let interest_rate = 0.037910
//#see idivs.org for expected dividend yields
let dividend_rate = 0.52
let volatility_rate = 0.3142
let underlying_price = new SimpleQuote(107.48)


//###################################################
//##2)
//#Date setup
//###################################################
Settings.instance().setEvaluationDate(valuation_date)

//#Asumptions
let calendar = new UnitedStates()
let day_counter = new ActualActual()


//###################################################
//##3)
//#Curve setup
//###################################################
let interest_curve = new FlatForward(valuation_date, interest_rate, day_counter )

let dividend_curve = new FlatForward(valuation_date, dividend_rate, day_counter )

let volatility_curve = new BlackConstantVol(valuation_date, calendar, volatility_rate, day_counter )


//###################################################
//##4)
//#Option setup
//###################################################
let exercise = new EuropeanExercise(expiry_date)  
//let exercise = new AmericanExercise(expiry_date)  
let payoff = new PlainVanillaPayoff(put_or_call, strike_price)

//#Option Setup
let option = new VanillaOption(payoff, exercise)

//#Collate market data together
let u = new QuoteHandle(underlying_price) 
let d = new YieldTermStructureHandle(dividend_curve)
let r = new YieldTermStructureHandle(interest_curve)
let v = new BlackVolTermStructureHandle(volatility_curve)  
let oprocess = new BlackScholesMertonProcess(u, d, r, v)

//#Set pricing engine
//let engine = new AnalyticEuropeanEngine(oprocess )
let engine = new FDAmericanEngine(oprocess )

option.setPricingEngine(engine)


//###################################################
//##5)
//##Collate results
//###################################################
printfn "NPV: %f" (option.NPV())
printfn "Delta: %f" (option.delta())
printfn "Gamma: %f" (option.gamma())
printfn "Vega: %f" (option.vega())
printfn "Theta: %f" (option.theta()) 
printfn "Rho: %f" (option.rho())
printfn "Dividend Rho: %f" (option.dividendRho())
printfn "Theta per Day: %f" (option.thetaPerDay())
printfn "Strike Sensitivity: %f" (option.strikeSensitivity())