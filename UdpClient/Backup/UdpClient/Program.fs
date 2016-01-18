open System;
open System.Net;
open System.Net.Sockets;
let rand : Random = new Random();
let mas : byte[] = [|0uy..99uy|];
for x = 0 to mas.Length - 1 do
 mas.[x] <- Convert.ToByte(rand.Next(0, 255));
Console.WriteLine("Исходные данные:");
for x = 0 to mas.Length - 1 do
  Console.Write(mas.[x].ToString() + " ");
Console.WriteLine("\nНажмите Enter, чтобы начать работу");
Console.ReadLine();
let client : UdpClient = new UdpClient();
let mutable ip : IPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 543);
let byt : byte[] = [|Convert.ToByte(mas.Length)|];
let count : byte[] = [|Convert.ToByte(mas.Length)|];
client.Send(count, 1, ip)
client.Receive(ref ip)
client.Send(mas, mas.Length, ip)
let newMas = client.Receive(ref ip)
Console.WriteLine("Полученные данные");
for x = 0 to newMas.Length - 1 do
  Console.Write(newMas.[x].ToString() + " ")
Console.ReadLine();