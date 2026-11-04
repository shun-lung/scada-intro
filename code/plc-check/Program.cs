// 工具讀得到、C# 讀不到時用：把測試工具的設定照抄過來，看 C# 讀到什麼
using System;
using System.Net.Sockets;
using NModbus;

// ① 測試工具怎麼填，這裡就怎麼抄
string ip = "192.168.0.10";   // IP 位址（練習用假 PLC：127.0.0.1）
int port = 502;            // Port（練習用假 PLC：5020）
byte unitId = 1;           // 站號
int kind = 3;              // 櫃子：0、1、3、4
ushort start = 0;          // 起始位址（從 0 數：30001 → 0）
ushort count = 3;          // 讀幾格

// ② 連線：2 秒連不上就放棄
using var tcp = new TcpClient();
try
{
    if (!tcp.ConnectAsync(ip, port).Wait(2000))
    {
        Console.WriteLine("✗ 2 秒連不上：IP 不對或網路不通");
        return;
    }
}
catch (AggregateException)
{
    Console.WriteLine("✗ 連線被拒絕：Port 不對，或 PLC 不接受更多連線（測試工具還連著？）");
    return;
}
var plc = new ModbusFactory().CreateMaster(tcp);
plc.Transport.ReadTimeout = 2000;
plc.Transport.Retries = 0;

// ③ 照櫃子種類選方法，讀回來一格一格印
try
{
    if (kind == 0 || kind == 1)
    {
        bool[] bits = kind == 0
            ? plc.ReadCoils(unitId, start, count)
            : plc.ReadInputs(unitId, start, count);
        for (int i = 0; i < bits.Length; i++)
            Console.WriteLine($"{kind * 10000 + start + i + 1:00000}  {bits[i]}");
    }
    else
    {
        ushort[] regs = kind == 3
            ? plc.ReadInputRegisters(unitId, start, count)
            : plc.ReadHoldingRegisters(unitId, start, count);
        for (int i = 0; i < regs.Length; i++)
            Console.WriteLine($"{kind * 10000 + start + i + 1}  原始 {regs[i],5}  有正負號 {(short)regs[i],6}");
    }
}
// ④ 出錯時，說清楚是哪一種錯
catch (SlaveException ex)
{
    Console.WriteLine($"✗ PLC 回答「不行」，例外碼 {ex.SlaveExceptionCode}");
    Console.WriteLine("  碼 2：這一格不存在 → 對照工具的櫃子種類和起始位址");
    Console.WriteLine("  碼 1：不支援這個動作 → 櫃子種類選錯，或這一櫃不能寫");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ PLC 沒回答：{ex.GetType().Name}");
    Console.WriteLine("  連得上卻沒回答 → 站號跟工具不一樣");
}
