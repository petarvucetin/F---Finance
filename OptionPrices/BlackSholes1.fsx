let ncdf x =
  let b1 =  0.319381530
  let b2 = -0.356563782
  let b3 =  1.781477937
  let b4 = -1.821255978
  let b5 =  1.330274429
  let p  =  0.2316419
  let c  =  0.39894228
  match x with
  | x when x >= 0.0 ->
    let t = 1.0 / (1.0 + p * x)
    (1.0 - c * Math.Exp( -x * x / 2.0)* t * (t*(t*(t*(t*b5+b4)+b3)+b2)+b1))
  | _ ->
    let t = 1.0 / (1.0 - p * x)
    (c * Math.Exp( -x * x / 2.0)* t * (t*(t*(t*(t*b5+b4)+b3)+b2)+b1))

let call strike spot (rate:float) (now:float) (expiry:float) (vol:float) =
    let exp = expiry-now
    let d1 = (Math.Log(spot/strike) + ((rate+(vol*vol)/2.0)*exp))/(vol * Math.Sqrt(exp))
    let d2 = d1 - (vol * (Math.Sqrt(exp)))
    (spot * ncdf d1) - (strike * Math.Pow(Math.E, -rate*exp)*(ncdf d2)), ncdf d1

