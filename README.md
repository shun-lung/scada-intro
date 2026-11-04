# SCADA 入門圖解

給會寫 C#、但不是電機也不是資工背景的人：PLC 的訊號（DI/DO/AI/AO）怎麼變成 C# 讀得到的數字，接不到時怎麼檢查。整套用同一台「洗脫烘洗衣機」當例子。

## 線上閱讀

- [SCADA 入門圖解](https://shun-lung.github.io/scada-intro/)：置物櫃比喻、口訣、17 點洗衣機、感測器／數位／類比、4–20 mA 換算、斷線判斷、30 題測驗
- [C# 接 PLC 實戰](https://shun-lung.github.io/scada-intro/csharp-plc.html)：點位表對不上怎麼辦、設定畫面、找會動的格子、C# 工程師開工 7 步

## 範例程式

- `code/washer.st`：OpenPLC 用的洗衣機練習程式（心跳 40002、模擬水位 40003）
- `code/csharp-test/`：C# 讀 PLC 的練習程式（NModbus），`dotnet run` 執行
- `code/plc-check/`：工具讀得到、C# 讀不到時用的對照程式（把測試工具的設定照抄到最上面 6 行）
- `code/fake-plc/`：練習用的假 PLC（洗衣機點位，127.0.0.1:5020，站號 1）。先在一個終端機 `dotnet run` 它，再把 plc-check 的 IP 改 127.0.0.1、Port 改 5020 來練習各種錯誤

## 說明

- 網頁裡的設定畫面是示意圖；`shots/` 是 OpenPLC（開源軟體）的實際操作截圖
- 影片旁白使用 AI 語音
