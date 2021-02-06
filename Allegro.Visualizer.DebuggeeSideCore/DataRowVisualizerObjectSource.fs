namespace Allegro.Visualizer.DebuggeeSide

open Microsoft.VisualStudio.DebuggerVisualizers
open System
open System.IO
open System.Data

[<Serializable>]
type DataRowVisualizerObjectSource() =
    inherit VisualizerObjectSource()

    member private x.RemoveAllConstraints(table: DataTable) =
        table.Constraints.Clear()
        for column:DataColumn in table.Columns do
            column.AllowDBNull <- true     

    member private x.ProcessDataTable (dataTable: DataTable, outgoingData: Stream) =
        use table = dataTable.Clone()
        x.RemoveAllConstraints(table)
        try
            table.Merge(dataTable)
        with
        | _ ->  try
                    dataTable.Select() |> Array.iter table.ImportRow
                with
                | e -> printfn "%A" e
        outgoingData = StreamSerializer.ObjectToStream(outgoingData, table) |> ignore

    member private x.ProcessDataRow (dataRow: DataRow, outgoingData: Stream) =
        let mutable table: DataTable = null
        if dataRow.Table = null then
            table <- new DataTable()
            for i = 0 to dataRow.ItemArray.Length do
                let column = table.Columns.Add(String.Format("Col{0}", i.ToString()), typeof<String>)
                column.AllowDBNull <- true
        else
            table <- dataRow.Table.Clone()
            x.RemoveAllConstraints(table)
        try 
            table.ImportRow(dataRow)
        with
           e -> printfn "%A" e
        outgoingData = StreamSerializer.ObjectToStream(outgoingData, table) |> ignore

    member private x.ProcessDataView (view: DataView, outgoingData: Stream)  =
        let table = view.Table.Clone()
        x.RemoveAllConstraints(table)
        for row in view.Table.Rows do
            table.ImportRow(row)
        outgoingData = StreamSerializer.ObjectToStream(outgoingData, table) |> ignore        

    member private x.ProcessDataRowView (dataRowView: DataRowView, outgoingData: Stream) =
        let table = dataRowView.Row.Table.Clone()
        x.RemoveAllConstraints(table)
        table.ImportRow(dataRowView.Row)
        outgoingData = StreamSerializer.ObjectToStream(outgoingData, table) |> ignore

    member private x.ProcessDataRowCollection (dataRowCollection: DataRowCollection, outgoingData: Stream) =
        let mutable table: DataTable = null
        if dataRowCollection <> null && dataRowCollection.Count > 0 then
            if (dataRowCollection.[0].Table = null) then
                table <- new DataTable();
                for i = 0 to dataRowCollection.[0].ItemArray.Length do
                    table.Columns.Add(String.Format("Col{0}", i.ToString()), typeof<String>) |> ignore
            else
                table <- dataRowCollection.[0].Table.Clone();
                x.RemoveAllConstraints(table)
            for dataRow in dataRowCollection do
                table.ImportRow(dataRow)
        outgoingData = StreamSerializer.ObjectToStream(outgoingData, table) |> ignore

    override x.GetData(target:obj, outgoingData:Stream) =
        if target <> null then 
            match target with
            | :? DataTable as dataTable -> x.ProcessDataTable(dataTable, outgoingData)
            | :? DataRow as dataRow -> x.ProcessDataRow(dataRow, outgoingData)
            | :? DataView as view -> x.ProcessDataView(view, outgoingData)
            | :? DataRowView as dataRowView -> x.ProcessDataRowView(dataRowView, outgoingData)
            | :? DataRowCollection as dataRowCollection -> x.ProcessDataRowCollection(dataRowCollection, outgoingData)
            | _ -> ()
