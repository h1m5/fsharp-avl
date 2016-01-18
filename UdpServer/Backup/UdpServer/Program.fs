open System;
open System.Collections.Generic;
open System.Threading;
open System.Net;
open System.Net.Sockets;
let rec QuickSort (mas : byte[], l : int, r : int) =
let mutable i = l
let mutable j = r 
let selectedIndex : int = l + (r - l) / 2
let p = mas.[selectedIndex]
while i <= j do
 while mas.[i] < p do
  i <- i + 1
 while mas.[j] > p do
  j <- j - 1
 if i <= j then
  let temp = mas.[i]
  mas.[i] <- mas.[j]
  mas.[j] <- temp
  i <- i + 1
  j <- j - 1
if i < r then 
 QuickSort (mas, i, r);
if l < j then 
 QuickSort (mas, l, j);
;;

let BeginOfTime : DateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)

let ip : IPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 543)
let mutable myCount : int = 0;
let mutable point : EndPoint = ip :> EndPoint;
let mutable startTimer : float = 0.0;
let mutable endTimer : float = 0.0;
let mutable mas : byte[] = [|1uy..1uy|]
let mutable server : Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
server.Bind(ip);
Console.WriteLine("Сервер запущен")
while true do
 let byt : byte[] = [|1uy..10uy|] // 10 заменить на 1, посмотреть, все ли будет работать
 server.ReceiveFrom(byt, &point)
 myCount <- Convert.ToInt32(byt.[0]);
 let b : byte = (Convert.ToByte(byt.[0] - 1uy));
 mas <- [|0uy..b|];
 server.SendTo(mas, mas.Length, SocketFlags.None, point)
 server.ReceiveFrom(mas, &point)
 let list : List<byte> = new List<byte>()
 let o : Object = new Object()
 let timeNow : TimeSpan = DateTime.UtcNow - BeginOfTime;
 startTimer <- timeNow.TotalMilliseconds;
 for x = 0 to 9 do
  let thread : Thread = new Thread ( fun () -> 
  let subMas : byte[] = [|0uy..(Convert.ToByte(mas.Length - 1)) / 10uy|]
  for y = 0 to subMas.Length - 1 do
    subMas.[y] <- mas.[10 * x + y]
  QuickSort(subMas, 0, subMas.Length - 1)
  lock o ( fun () ->
  list.AddRange(subMas)
  )
  )
  thread.Start()
 while (not(list.Count = 100)) do
   ()
 let timeNow : TimeSpan = DateTime.UtcNow - BeginOfTime;
 endTimer <- timeNow.TotalMilliseconds;
 Console.WriteLine("Время выполнения сортировки - " + (endTimer - startTimer).ToString())
 let result : byte[] = list.ToArray()
 list.Clear()
 Array.Sort(result)
 server.SendTo(result, result.Length, SocketFlags.None, point)
Console.ReadLine();