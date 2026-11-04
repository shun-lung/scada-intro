// 假 PLC：洗衣機點位，只開 30001~30010、40001~40010，其他格子回「不存在」
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NModbus;

class Range<T> : IPointSource<T>
{
    readonly T[] data;
    public Range(int n) { data = new T[n]; }
    public T[] ReadPoints(ushort start, ushort count)
    {
        if (start + count > data.Length) throw new InvalidModbusRequestException(SlaveExceptionCodes.IllegalDataAddress);
        lock (data) return data.Skip(start).Take(count).ToArray();
    }
    public void WritePoints(ushort start, T[] points)
    {
        if (start + points.Length > data.Length) throw new InvalidModbusRequestException(SlaveExceptionCodes.IllegalDataAddress);
        lock (data) Array.Copy(points, 0, data, start, points.Length);
    }
}
class Store : ISlaveDataStore
{
    public IPointSource<bool> CoilDiscretes { get; } = new Range<bool>(10);
    public IPointSource<bool> CoilInputs { get; } = new Range<bool>(10);
    public IPointSource<ushort> HoldingRegisters { get; } = new Range<ushort>(10);
    public IPointSource<ushort> InputRegisters { get; } = new Range<ushort>(10);
}
class P
{
    static void Main(string[] args)
    {
        int port = args.Length > 0 ? int.Parse(args[0]) : 5020;
        var store = new Store();
        store.InputRegisters.WritePoints(0, new ushort[] { 0, 8294, 13824 });          // 30001 水位(舊)、30002 水溫、30003 水位(新)
        float f = 25.3f; var b = BitConverter.GetBytes(f);
        ushort lo = BitConverter.ToUInt16(b, 0), hi = BitConverter.ToUInt16(b, 2);
        store.HoldingRegisters.WritePoints(0, new ushort[] { 1152, 0, 0, 0, 65535, hi, lo }); // 40001 轉速、40002 心跳、40005 = -1、40006~40007 = 25.3
        store.CoilInputs.WritePoints(0, new[] { true });                                      // 10001 開始鍵按著
        var factory = new ModbusFactory();
        var listener = new TcpListener(IPAddress.Loopback, port); listener.Start();
        var net = factory.CreateSlaveNetwork(listener);
        net.AddSlave(factory.CreateSlave(1, store));
        new Thread(() => { ushort hb = 0; while (true) { Thread.Sleep(1000); store.HoldingRegisters.WritePoints(1, new[] { ++hb }); } }) { IsBackground = true }.Start();
        Console.WriteLine($"假 PLC 啟動：127.0.0.1:{port}，站號 1");
        net.ListenAsync().Wait();
    }
}
