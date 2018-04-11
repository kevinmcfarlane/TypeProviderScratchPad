
#I @"..\packages\"
#r @"FSharp.Data.2.4.6\lib\net45\FSharp.Data.dll"
#load "../packages/FSharp.Charting.0.90.14/FSharp.Charting.fsx"

open FSharp.Data

//
// Querying StackOverflow with JSON Provider
//
type Questions = JsonProvider<"""https://api.stackexchange.com/2.2/questions?site=stackoverflow""">

// List 30 question titles (the default page size is 30) which are the most active questions tagged C# 
// that were asked on StackOverflow. 
// See http://api.stackexchange.com/docs/questions
let csQuestions = """https://api.stackexchange.com/2.2/questions?site=stackoverflow&tagged=C%23"""
Questions.Load(csQuestions).Items |> Seq.iter (fun q -> printfn "%s" q.Title)

//
// We can create a domain-specific language (DSL) to facilitate querying with parameters
//
let questionQuery = """https://api.stackexchange.com/2.2/questions?site=stackoverflow"""

let tagged tags query =
    let joinedTags = tags |> String.concat ";"
    sprintf "%s&tagged=%s" query joinedTags

let page p query = sprintf "%s&page=%i" query p
let pageSize s query = sprintf "%s&pagesize=%i" query s
let extractQuestions (query:string) = Questions.Load(query).Items

let ``C#`` = "C%23"
let ``F#`` = "F%23"

// List C# questions
let cs =
    questionQuery
    |> tagged [``C#``]
    |> pageSize 50
    |> extractQuestions
    |> Seq.iter (fun q -> printfn "%s" q.Title)

// List F# questions
let fs =
    questionQuery
    |> tagged [``F#``]
    |> pageSize 50
    |> extractQuestions
    |> Seq.iter (fun q -> printfn "%s" q.Title)

// List C# and F# questions
let combined =
    questionQuery
    |> tagged [``C#``; ``F#``]
    |> pageSize 50
    |> extractQuestions
    |> Seq.iter (fun q -> printfn "%s" q.Title)

//
// Type inference from JsonProvider "shape"
//
// Infer int and string
type Simple = JsonProvider<""" { "name":"John", "age":94 } """>
let simple = Simple.Parse(""" { "name":"Tomas", "age":4 } """)
printfn "Age = %i" simple.Age
printfn "Name = %s" simple.Name

// Infer decimal
type Numbers = JsonProvider<""" [1, 2, 3, 3.14] """>
let nums = Numbers.Parse(""" [1.2, 45.1, 98.2, 5] """)
let total = nums |> Seq.sum
printfn "Total = %M" total

// Infer int and string
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

//
// Data Exploration - prelude to machine learning
//
// Bicycle rentals
// - data from
// UCI Machine Learning Repository: Bike Sharing Dataset Data Set
// https://archive.ics.uci.edu/ml/datasets/bike+sharing+dataset
type Data = CsvProvider<"data/day.csv">
let dataset = Data.Load("data/day.csv")
let data1 = dataset.Rows

// Plot of day-by-day bicycle usage over time since the first observation
let all = Chart.Line [ for obs in data1 -> obs.Cnt ] // cnt: count of total rental bikes including both casual and registered

// Calculate some bicycle usage stats
let count = data1 |> Seq.map (fun x -> float x.Cnt)
let avg = count |> Seq.average
let min = count |> Seq.min
let max = count |> Seq.max

// Create a list of windows of three observations
// This chunks the series 1 .. 10 into blocks of three consecutive values: [[|1; 2; 3|]; [|2; 3; 4|];
// [|3; 4; 5|]; [|4; 5; 6|]; [|5; 6; 7|]; [|6; 7; 8|]; [|7; 8; 9|]; [|8; 9; 10|]]
let windowedExample =
    [ 1 .. 10 ]
    |> Seq.windowed 3
    |> Seq.toList

// Moving average with window size n
let movingAverage n (series:float seq) = 
    series 
    |> Seq.windowed n
    |> Seq.map (fun xs -> xs |> Seq.average)
    |> Seq.toList

// Plot original series overlaid with the moving averages over seven days and thirty days
// (The moving averages do hint at a longer trend, and clearly show a certain regularity in the fluctuations over the year, including seasonal spikes.)
Chart.Combine [
    Chart.Line count
    Chart.Line (movingAverage 7 count)
    Chart.Line (movingAverage 30 count) ]
