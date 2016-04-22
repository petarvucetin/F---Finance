#r "..\packages\DiffSharp\lib\net46\DiffSharp.dll"
open DiffSharp.AD.ForwardG
 
// Cumulative Normal Distribution Function - attempt to write a generic version
let inline CDF(x:DualG) : DualG = 
    let (b1,b2,b3)  = (0.319381530, -0.356563782, 1.781477937)
    let (b4,b5)     = (-1.821255978, 1.330274429)
    let (p , c )    = (0.2316419  ,  0.39894228)
    let (zero, one) = (LanguagePrimitives.GenericZero, LanguagePrimitives.GenericOne)
    if x &gt; zero then
        let t:DualG = one / (one + p * x) 
        (one - c * exp( -x * x / 2.0)* t * (t*(t*(t*(t*b5+b4)+b3)+b2)+b1)) 
    else
        let t:DualG = 1.0 / (one - p * x) 
        (c * exp( -x * x / 2.0)* t * (t*(t*(t*(t*b5+b4)+b3)+b2)+b1))
    // - See more at: http://www.voyce.com/index.php/2009/06/26/black-scholes-option-pricing-using-fsharp-and-wpf/#sthash.i9UUX0Yh.dpuf
  
 
// call_put_flag: 'c' if call option; otherwise put option
// s: stock price
// x: strike price of option
// t: time to expiration in years
// r: risk free interest rate
// v: volatility
let black_scholes_call (x:float) (args:DualG[])  =
    let (spot, t, r, vol) = (args.[0], args.[1], args.[2], args.[3])
    let d1=(log(spot / x) + (r+vol*vol/2.0)*t)/(vol*sqrt(t))
    let d2=d1-vol*sqrt(t)
    let result = spot*CDF(d1)-x*exp(-r*t)*CDF(d2)
    result
  
 
  
// Example usage::
let b_s_functor = black_scholes_call 65.0
let (pv, greeks) = grad' b_s_functor [|60.0; 0.25; 0.08; 0.3|]
//let dd = black_scholes_call 65.0 60.0 0.25 0.08 0.3
printfn &quot;PV=%f&quot; pv
printfn &quot;Greeks (Delta;Theta;Rho;Vega) = %A&quot; greeks
printfn &quot;END&quot;