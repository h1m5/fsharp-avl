open System;
open System.IO;
open System.Collections.Generic;
open System.Threading;
open System.Net;
open System.Net.Sockets;

type MyWord =
    struct
        val key : string
        val meaning : string
        new (k, m) = {key = k; meaning = m;}
      end

type tree(*<'a when 'a : comparison>*)= 
    | Node of int * tree * MyWord * tree
    | Nil

let height = function
    | Node(h, _, _, _) -> h
    | Nil -> 0
    
let make l x r =
    let h = 1 + max (height l) (height r)
    Node(h, l, x ,r)

let rotRight = function
    | Node(_, Node(_, ll, lx, lr), x, r) ->
        let r' = make lr x r
        make ll lx r'
    | node -> node
    
let rotLeft = function
    | Node(_, l, x, Node(_, rl, rx, rr)) ->
        let l' = make l x rl
        make l' rx rr
    | node -> node
    
let doubleRotLeft = function
    | Node(h, l, x, r) ->
        let r' = rotRight r
        let node' = make l x r'
        rotLeft node'
    | node -> node
    
let doubleRotRight = function
    | Node(h, l, x, r) ->
        let l' = rotLeft l
        let node' = make l' x r
        rotRight node'
    | node -> node
    
let balanceFactor = function
    | Nil -> 0
    | Node(_, l, _, r) -> (height l) - (height r)
    
let balance = function
    (* left unbalanced *)
    | Node(h, l, x, r) as node when balanceFactor node >= 2 ->
        if balanceFactor l >= 1 then rotRight node      (* left left case *)
        else doubleRotRight node                        (* left right case *)
    (* right unbalanced *)
    | Node(h, l, x, r) as node when balanceFactor node <= -2 ->
        if balanceFactor r <= -1 then rotLeft node      (* right right case *)
        else doubleRotLeft node                         (* right left case *)
    | node -> node
   
let rec insert v = function
    | Nil -> Node(1, Nil, v, Nil)
    | Node(_, l, x, r) as node ->
        if v = x then node
        else
            let l', r' = if v.key < x.key then insert v l, r else l, insert v r
            let node' = make l' x r'
            balance <| node'
            
let rec contains v= function
    | Nil -> new MyWord(v,"Not found")
    | Node(_, l, x, r) ->
        if v = x.key then x
        else
            if v < x.key then contains v l
            else contains v r

[<Class>]
type AvlTree(Tree : tree) =
    class
        member this.Height = height Tree

        member this.Left =
            match Tree with
            | Node(_, l, _, _) -> new AvlTree(l)
            | Nil -> failwith "Empty tree"
    
        member this.Right =
            match Tree with
            | Node(_, _, _, r) -> new AvlTree(r)
            | Nil -> failwith "Empty tree"
        
        member this.Value =
            match Tree with
            | Node(_, _, x, _) -> x
            | Nil -> failwith "Empty tree"
        
        member this.Insert(x) = new AvlTree(insert x Tree)
    
        member this.Contains(v) = contains v Tree
    end

let reader(filename : string) = seq {
    use sr = new StreamReader(filename)
    while not sr.EndOfStream do
        let line = sr.ReadLine()
        yield line
    done
}

let mutable Dictionary = new AvlTree(Nil)
let mutable word = ""
let mutable meaning = ""
let mutable aWord : MyWord = new MyWord() 

// Read dictionary file and fill tree
reader("dict.txt") |>
    Seq.iteri(fun i line -> 
                            if (i%2=0) then word <- line
                            else meaning <- line

                            if (word <> "" && meaning <> "") then
                                Dictionary <- Dictionary.Insert(new MyWord(word, meaning))
                                word <- ""
                                meaning <- ""
                            //printfn "%d> %s" (i+1) line
                            )

let service (client:TcpClient) = 
    use stream = client.GetStream()
    use out = new StreamWriter(stream, AutoFlush = true)
    use inp = new StreamReader(stream)
    while not inp.EndOfStream do
        match inp.ReadLine() with
        | line -> printfn "< %s" line 
                  let req = Dictionary.Contains(line)
                  let res = sprintf "%s\n" req.meaning
                  out.WriteLine(res)
                  
    printfn "closed %A" client.Client.RemoteEndPoint
    client.Close |> ignore

let DictionaryService = 
    let socket = new TcpListener(IPAddress.Loopback, 543)
    do socket.Start()
    printfn "Dictionary service listening on %A" socket.Server.LocalEndPoint
    while true do
        let client = socket.AcceptTcpClient()
        printfn "connect from %A" client.Client.RemoteEndPoint
        let job = async { 
            use c = client in try service client with _ -> () }
        Async.Start job

Console.ReadLine() |> ignore