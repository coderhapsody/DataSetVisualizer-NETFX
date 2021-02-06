namespace Allegro.Visualizer

open Microsoft.VisualStudio.DebuggerVisualizers
open System.Diagnostics
open System.Data
open Allegro.Visualizer.DebuggeeSide


type DataSetVisualizer() =
    inherit DialogDebuggerVisualizer()

    member private x.BuildDataSetToBeVisualized(object: obj) : DataSet option =        
        match object with
        | :? DataSet as dataSet -> 
            Some(dataSet)
        | :? DataTable as dataTable ->
            use ds = new DataSet()
            ds.EnforceConstraints <- false
            ds.Tables.Add(dataTable) |> ignore
            Some(ds)
        | :? DataRow as dataRow ->
            use ds = new DataSet()
            ds.EnforceConstraints <- false
            use dataTable = new DataTable()
            dataTable.ImportRow(dataRow)
            ds.Tables.Add(dataTable)
            Some(ds)
        | _ -> None

    member private x.ShowVisualizerForm (windowService: IDialogVisualizerService, dataSource: DataSet) =
        use visualizerForm = new VisualizerForm(dataSource.DataSetName)
        visualizerForm.SetDataSource(dataSource)       
        windowService.ShowDialog(visualizerForm) |> ignore

    override x.Show(windowService, objectProvider) =
        let thisObj = objectProvider.GetObject()
        let dataSource = x.BuildDataSetToBeVisualized(thisObj)
        match dataSource with
        | Some ds -> x.ShowVisualizerForm(windowService, ds)
        | None -> ()


[<assembly: DebuggerVisualizer(typeof<DataSetVisualizer>, typeof<VisualizerObjectSource>, Target = typeof<DataSet>, Description = "Allegro DataSet Visualizer")>] 
[<assembly: DebuggerVisualizer(typeof<DataSetVisualizer>, typeof<DataRowVisualizerObjectSource>, Target = typeof<DataTable>, Description = "Allegro DataTable Visualizer")>] 
[<assembly: DebuggerVisualizer(typeof<DataSetVisualizer>, typeof<DataRowVisualizerObjectSource>, Target = typeof<DataRow>, Description = "Allegro DataRow Visualizer")>]    
[<assembly: DebuggerVisualizer(typeof<DataSetVisualizer>, typeof<DataRowVisualizerObjectSource>, Target = typeof<DataRowCollection>, Description = "Allegro DataRowCollection Visualizer")>]
[<assembly: DebuggerVisualizer(typeof<DataSetVisualizer>, typeof<DataRowVisualizerObjectSource>, Target = typeof<DataView>, Description = "Allegro DataView Visualizer")>]
[<assembly: DebuggerVisualizer(typeof<DataSetVisualizer>, typeof<DataRowVisualizerObjectSource>, Target = typeof<DataRowView>, Description = "Allegro DataRowView Visualizer")>]
do()
