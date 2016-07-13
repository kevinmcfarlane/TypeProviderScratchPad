
#I @"..\packages\"
#r @"FSharp.Data.2.3.1\lib\net40\FSharp.Data.dll"
#load "../packages/FSharp.Charting.0.90.14/FSharp.Charting.fsx"

open FSharp.Data

//
// JSON Provider
//
type Questions = JsonProvider<"""https://api.stackexchange.com/2.2/questions?site=stackoverflow""">

// List 30 question titles (the default page size is 30) which are the most active questions tagged C# 
// that were asked on StackOverflow. 
// See http://api.stackexchange.com/docs/questions
let csQuestions = """https://api.stackexchange.com/2.2/questions?site=stackoverflow&tagged=C%23"""
Questions.Load(csQuestions).Items |> Seq.iter (fun q -> printfn "%s" q.Title)

// Infers int and string
type Simple = JsonProvider<""" { "name":"John", "age":94 } """>
let simple = Simple.Parse(""" { "name":"Tomas", "age":4 } """)
printfn "Age = %i" simple.Age
printfn "Name = %s" simple.Name

// Infers decimal
type Numbers = JsonProvider<""" [1, 2, 3, 3.14] """>
let nums = Numbers.Parse(""" [1.2, 45.1, 98.2, 5] """)
let total = nums |> Seq.sum
printfn "Total = %M" total

// Infers int and string
type Mixed = JsonProvider<""" [1, 2, "hello", "world"] """>
let mixed = Mixed.Parse(""" [4, 5, "hello", "world" ] """)

let sum = mixed.Numbers |> Seq.sum
let concat = mixed.Strings |> String.concat ", "

printfn "Sum = %i" sum
printfn "Concat = %s" concat

// The following example uses two records - one with name and age and the second with just name. 
// If a property is missing, then the provider infers it as optional.
type People = JsonProvider<""" 
  [ { "name":"John", "age":94 }, 
    { "name":"Tomas" } ] """>

for item in People.GetSamples() do 
  printf "%s " item.Name 
  item.Age |> Option.iter (printf "(%d)")
  printfn ""

//
// World Bank Provider
//
let data = WorldBankData.GetDataContext()

open FSharp.Charting

// Percentage of population in higher education over time (from 1971)
data.Countries.``United Kingdom``
    .Indicators.``Gross enrolment ratio, tertiary, both sexes (%)``
|> Chart.Line
|> Chart.WithXAxis(Title = "Year")
|> Chart.WithYAxis(Title = "%")
|> Chart.WithTitle(Text = "UK % of population in higher education", InsideArea = false)

// Annual percent growth rate of GDP (from 1961)
data.Countries.``United Kingdom``
    .Indicators.``GDP growth (annual %)``
|> Chart.Line
|> Chart.WithXAxis(Title = "Year")
|> Chart.WithYAxis(Title = "%")
|> Chart.WithTitle(Text = "UK GDP growth (annual %)", InsideArea = false)

// CO2 emissions per capita over time (from 1960)
data.Countries.``United Kingdom``
    .Indicators.``CO2 emissions (metric tons per capita)``
|> Chart.Line
|> Chart.WithXAxis(Title = "Year")
|> Chart.WithYAxis(Title = "Metric Tons")
|> Chart.WithTitle(Text = "UK CO2 emissions (metric tons per capita)", InsideArea = false)



