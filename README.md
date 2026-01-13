# Aviator Simulation & Strategy Analyzer (Aviator 博奕模擬與分析器)

> 一個整合了 **遊戲前端 (Playable Client)** 與 **高頻交易模擬後端 (High-Frequency Simulator)** 的demo專案。
> 
> **核心技術：** C# 數學模型模擬 / ScriptableObject 策略模組 / 蒙地卡羅模擬 (Monte Carlo) / CSV 數據分析

---

###  專案簡介 (Introduction)
本專案重現了經典的 Crash Game (Aviator) 玩法，並在此基礎上開發了一套 **策略回測工具**。
本專案側重於 **數值驗證** 與 **系統穩定性分析**。透過內建的模擬器，開發者可以驗證不同下注策略（如馬丁格爾）在數萬次賽局後的玩家資金曲線與破產機率。

透過這套工具，開發者或策劃人員可以：
1.  **自定義測試環境**：設定測試次數（如 10,000 次）、基礎賠率曲線。
2.  **掛載不同策略**：使用 ScriptableObject 模組化的 AI 下注策略（如馬丁格爾策略）。
3.  **分析營收數據**：計算玩家勝率 (RTP)、最大回撤 (Max Drawdown) 以及莊家優勢 (House Edge)。
4.  **視覺化報表**：自動導出 CSV，透過 Excel 進行圖表分析。

###  功能展示 (Showcase)

 ## 遊戲本體 (Game Client) 
 <img width="780" height="428" alt="image" src="https://github.com/user-attachments/assets/0009f253-f803-4f45-9951-f670f8a06dfe" />
 
 *實作指數攀升與狀態機控制流程*

---

 ## 策略模擬器 (Simulator) 

 <img width="1646" height="692" alt="image" src="https://github.com/user-attachments/assets/94ed4f68-594d-4efc-b913-1d6d80b1bb2c" />
 
*可配置模擬次數、策略模組與參數*

---

 ## 數據視覺化 (Data Viz)  

<img width="677" height="440" alt="image" src="https://github.com/user-attachments/assets/6c5f628e-65a6-4617-b797-17c548d3330b" />

 *將匯出 CSV 放入excel中進行資金回撤分析* 

---

###  核心技術實作 (Technical Implementation)

[AviatorStateManager.cs](./Assets/Scripts/Aviator/AviatorStateManager.cs)
#### 1. 數學模型與隨機性 (Math & RNG)
為了模擬真實賭場的「長尾效應」(大倍率稀有，小倍率常見)，我不使用一般的 `Random.Range`，而是實作了 **Pareto Distribution (帕累托分布)** 算法。

* **Code Highlight (`AviatorStateManager.cs`):**
```csharp
// 使用 Inverse Transform Sampling 產生符合 Alpha 參數的長尾分布
private float GenerateMultiplier()
{
    double u = rng.NextDouble();
    // alpha 參數控制高倍率出現的稀有度
    float multiplier = (float)(minMultiplier / Math.Pow(u, 1.0 / alpha));
    return Mathf.Min(multiplier, maxMultiplier);
}
```
#### 2. 狀態機架構 (State Machine Pattern)
在遊戲本體 (AviatorStateManager) 中，我使用狀態機來控管遊戲流程，確保資金扣除與結算邏輯不會因為網路延遲或玩家連點而發生錯誤。

流程： PrepareState (下注階段) → FlyingState (計算倍率/即時收網) → ResultState (結算與清理)。

#### 3. 策略模式與模組化設計 (Strategy Pattern & SO)
為了在不修改主程式的情況下測試新策略，我利用 Unity 的 ScriptableObject 建立策略資產。

測試器 (AviatorTester) 依賴抽象的 BetStrategySO，而非具體的實作。

這允許我快速切換測試 馬丁格爾 (Martingale)、反馬丁 或 隨機下注 等策略。

#### 4. 高效大數據模擬 (High-Performance Simulation)
模擬器 (AviatorTester.cs) 捨棄了 Time.deltaTime 的逐幀執行，改用純數值運算。

效能優化： 使用 StringBuilder 處理大量字串拼接，並採用批次寫入 CSV 的方式，能在數秒內完成 100,000 場賽局模擬。

風險控管指標： 系統會自動計算 最大回撤 (Max Drawdown)，評估系統風險。

---

###  如何使用 (How to Use)

遊玩模式
進入 GameScene。

在準備時間 (Prepare Time) 輸入金額並按下「下注」。

在飛機飛走前按下「兌現 (Cash Out)」。

測試模式 (Data Analysis)
需使用unity開啟專案

進入 SimulatorScene 並選中 AviatorTester 物件。

設定參數：

Simulations: 設定模擬次數 (推薦 10,000+)。

Alpha: 調整賭場優勢 (數值越大，高倍率越難出)。

掛載策略： 將 Project 視窗中的策略檔案拖入 Strategies 陣列。

點擊 RunSimulation。

可前往 Assets/ 資料夾查看生成的 .csv 檔案 
匯入https://docs.google.com/spreadsheets/d/1qHLrjkZV5cN15PmwWHaPb6pbaLb04_jKlgkVeF52ciU/edit?usp=sharing 
將資料匯入RawData工作表，並在dashboard查看圖表。
