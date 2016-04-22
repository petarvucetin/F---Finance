//https://mhittesdorf.wordpress.com/2013/07/05/introducing-quantlib-option-theory-and-pricing/

#r "..\packages\FSharp.Data\lib\portable-net40+sl5+wp8+win8\FSharp.Data.dll"
#r @"..\packages\NQuantLib.dll\lib\net\NQuantLib.dll"
#r @"..\packages\MathNet.Numerics\lib\portable-net4+sl5+netcore45+wpa81+wp8+MonoAndroid1+MonoTouch1\MathNet.Numerics.dll"

#r "System.Net"

#load "Helpers.fsx"

open System.Net
open FSharp.Data
open System.Text.RegularExpressions
open FSharp.Data.JsonExtensions
open Helpers
open System
open MathNet.Numerics.Distributions

let spot = 100.0; //current price of the underlying stock (S)
let r = 0.03; //risk-free rate
let t = 0.5;  //half a year
let sigma = 0.20; //estimated volatility of underlying
let strike = 110.0; // strike price of the call (K)

let a = strike
let b = strike * 10.0

let mean = log(spot) + (r-0.5*sigma*sigma) * t
let stdev = sigma * sqrt(t)

let d = LogNormal.WithMuSigma(mean, stdev)

let payoff = new PlainVanillaPayoff(Option.Type.call, strike)(x) * boost::math::pdf(d, x)