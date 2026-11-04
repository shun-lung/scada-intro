// SCADA 自學：用 C# 實際連 OpenPLC（洗衣機練習程式）
// 執行：dotnet run
using System;
using System.Net.Sockets;
using System.Threading;
using NModbus;

// 連到 PLC：本機的 OpenPLC，Modbus TCP 的 502 號門
using var tcp = new TcpClient("127.0.0.1", 502);
var plc = new ModbusFactory().CreateMaster(tcp);
plc.Transport.ReadTimeout = 1000;   // 1 秒沒回答就放棄

// 1. 先對心跳：40002 → 位址 1
Console.WriteLine("== 1. 先對心跳（40002）==");
for (int i = 0; i < 3; i++)
{
    ushort hb = plc.ReadHoldingRegisters(1, 1, 1)[0];
    Console.WriteLine($"心跳 {hb}");
    Thread.Sleep(1000);
}

// 2. 找會動的格子：每秒讀 40001～40010，印出有變的
Console.WriteLine("== 2. 掃描 40001～40010，找會動的格子 ==");
ushort[] last = null;
for (int round = 0; round < 4; round++)
{
    ushort[] now = plc.ReadHoldingRegisters(1, 0, 10);
    if (last != null)
        for (int i = 0; i < now.Length; i++)
            if (now[i] != last[i])
                Console.WriteLine($"{40001 + i} 變了：{last[i]} → {now[i]}");
    last = now;
    Thread.Sleep(1000);
}

// 3. 換算：40003 模擬水位，0～27648 對應 0～50 cm（兩把尺）
Console.WriteLine("== 3. 換算水位（40003）==");
ushort raw = plc.ReadHoldingRegisters(1, 2, 1)[0];
double cm = raw / 27648.0 * 50;
Console.WriteLine($"原始數字 {raw} → 水位 {cm:0.0} cm");

// 4. 讀開關：00001～00008（0 號櫃，線圈）
Console.WriteLine("== 4. 讀開關 00001～00008 ==");
bool[] coils = plc.ReadCoils(1, 0, 8);
Console.WriteLine($"00001 進水閥 {coils[0]}，00005 綠燈 {coils[4]}，00007 紅燈 {coils[6]}");

// 5. 讀 30001：OpenPLC 沒接感測器，所以一直是 0
Console.WriteLine("== 5. 讀 30001（沒接線）==");
Console.WriteLine($"30001 = {plc.ReadInputRegisters(1, 0, 1)[0]}");
