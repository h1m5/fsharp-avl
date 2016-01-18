open System;
open System.Net;
open System.Net.Sockets;
open System.Text
open System.IO

let rec asyncSendInput (stream : NetworkStream) =
    async {
        let input = Console.Read()
        let byt: byte[] = [|Convert.ToByte(input)|]
        byt |> Array.iter stream.WriteByte
        return! asyncSendInput stream
    }

let rec asyncPrintResponse (stream : NetworkStream) =
    async {
        let response = stream.ReadByte() |> Char.ConvertFromUtf32
        Console.Write(response)
        return! asyncPrintResponse stream
    }

[<EntryPoint>]
let main args =
    let client = new System.Net.Sockets.TcpClient()
    client.Connect("127.0.0.1", 543)
    let stream = client.GetStream()
    printfn "Enter search word to retrieve meaning from dictionary."
    asyncSendInput stream |> Async.Start
    asyncPrintResponse stream |> Async.RunSynchronously
    0

    