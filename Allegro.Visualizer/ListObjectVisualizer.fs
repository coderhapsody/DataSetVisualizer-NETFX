namespace Allegro.Visualizer

open Microsoft.VisualStudio.DebuggerVisualizers
open System.Collections
open System.Collections.Generic
open System.Diagnostics
open System.Data
open System
open System.Reflection


type ListObjectVisualizer() = 
    inherit DialogDebuggerVisualizer()

    member x.GetPropertyInformation(properties: PropertyInfo[]) =
        let propInfo = query {
            for prop in properties do
                select (prop.Name + ": " +
                        prop.PropertyType.Name + " " + 
                        (if prop.PropertyType.GetGenericArguments() <> null && prop.PropertyType.GetGenericArguments().Length > 0 then prop.PropertyType.GetGenericArguments().[0].Name else ""))
        }
        Seq.toArray propInfo

    override x.Show(windowService, objectProvider) =       
        let list = objectProvider.GetObject() :?> IList
        use form = new VisualizerForm(list.GetType().FullName)
        let properties = list.GetType().GetGenericArguments().[0].GetProperties()
        let propInfo = x.GetPropertyInformation(properties)
        form.SetDataSource(list, propInfo)
        windowService.ShowDialog(form) |> ignore


[<assembly: DebuggerVisualizer(typeof<ListObjectVisualizer>, typeof<VisualizerObjectSource>, Target = typeof<List<_>>, Description = "List Object Visualizer")>]
do()