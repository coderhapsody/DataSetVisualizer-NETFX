namespace Allegro.Visualizer.DebuggeeSide

open System.IO
open System.Runtime.Serialization.Formatters.Binary;
open System.Reflection


type StreamSerializer() =
    static member ObjectToStream (outgoingData: Stream, data:obj)  =
        let mutable stream = outgoingData 
        if(stream = null) then
            stream <- new MemoryStream()
        let formatter = BinaryFormatter()
        formatter.Serialize(outgoingData, data)
        stream

    static member StreamToObject (outgoingData) =
        let formatter = BinaryFormatter()
        formatter.Deserialize(outgoingData)


