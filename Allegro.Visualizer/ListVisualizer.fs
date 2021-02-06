namespace Allegro.Visualizer

open Microsoft.VisualStudio.DebuggerVisualizers
open System.Collections
open System.Collections.Generic
open System.Diagnostics
open System.Data
open System
open System.Reflection


type ListVisualizer() = 
    inherit DialogDebuggerVisualizer()

    member private x.CreateDataTable(properties: PropertyInfo[]) =
        let inline GetUnderlyingType(t: PropertyInfo) =
            let underlyingType = Nullable.GetUnderlyingType(t.PropertyType)
            if underlyingType <> null then Some(underlyingType) else None

        use table = new DataTable()
        for property in properties do
            match GetUnderlyingType(property) with
            | Some underlyingType -> 
                let column = new DataColumn(property.Name, underlyingType)                
                column
            | None ->
                let column = new DataColumn(property.Name, property.PropertyType)
                column
            |> table.Columns.Add   
        table

    member private x.PopulateDataRows (properties: PropertyInfo[]) (list: IList) (table: DataTable) =
        for item in list do
            let row = table.NewRow()            
            for property in properties do
                try
                    let propValue = 
                        let x = property.GetValue(item) 
                        if x <> null then Some(x) else None
                    match propValue with
                    | Some x -> row.[property.Name] <- x
                    | None -> row.[property.Name] <- DBNull.Value
                with
                | _ -> ()
            table.Rows.Add(row)
        table

    override x.Show(windowService, objectProvider) =       
        let list = objectProvider.GetObject() :?> IList
        let listType = list.GetType()
        use form = new VisualizerForm(listType.FullName)
        let properties = list.GetType().GetGenericArguments().[0].GetProperties()

        let table = 
            x.CreateDataTable(properties)
            |> x.PopulateDataRows properties list

        table.TableName <- list.GetType().GetGenericArguments().[0].Name

        use ds = new DataSet()
        ds.Tables.Add(table)
        form.SetDataSource(ds)
        windowService.ShowDialog(form) |> ignore


[<assembly: DebuggerVisualizer(typeof<ListVisualizer>, typeof<VisualizerObjectSource>, Target = typeof<List<_>>, Description = "List Visualizer")>]
do()