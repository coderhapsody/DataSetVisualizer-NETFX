// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open Allegro.Visualizer
open System
open System.Data

type NullCoalesce =  

    static member Coalesce(a: 'a option, b: 'a Lazy) = 
        match a with 
        | Some a -> a 
        | _ -> b.Value

    static member Coalesce(a: 'a Nullable, b: 'a Lazy) = 
        if a.HasValue then a.Value
        else b.Value

    static member Coalesce(a: 'a when 'a:null, b: 'a Lazy) = 
        match a with 
        | null -> b.Value 
        | _ -> a

let inline nullCoalesceHelper< ^t, ^a, ^b, ^c when (^t or ^a) : (static member Coalesce : ^a * ^b -> ^c)> a b = 
        // calling the statically inferred member
        ((^t or ^a) : (static member Coalesce : ^a * ^b -> ^c) (a, b))

let inline (|??) a b = nullCoalesceHelper<NullCoalesce, _, _, _> a b

[<EntryPoint>]
let main argv = 
    use form = new VisualizerForm("test")
    
    use dataSet = new DataSet()
    use dataTable = new DataTable("FirstTable")
    dataTable.Columns.Add("name", typeof<string>) |> ignore
    dataTable.Columns.Add("age", typeof<int>) |> ignore
    dataTable.Columns.Add("salary", typeof<decimal>) |> ignore

    let row = dataTable.NewRow()
    row.Item("name") <- "John"
    row.Item("age") <- 25
    row.Item("salary") <- 25000000M
    dataTable.Rows.Add(row)


    use dataTable2 = new DataTable("SecondTable")
    dataTable2.Columns.Add("deptid", typeof<string>) |> ignore
    dataTable2.Columns.Add("deptname", typeof<string>) |> ignore
    let row2 = dataTable2.NewRow()
    row2.Item("deptid") <- "FIN"
    row2.Item("deptname") <- "Finance"
    dataTable2.Rows.Add(row2)

    dataSet.Tables.Add(dataTable)
    dataSet.Tables.Add(dataTable2)

    form.SetDataSource(dataSet)

    form.ShowDialog()
    0
