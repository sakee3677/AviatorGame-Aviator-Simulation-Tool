using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;


public class AviatorTester : MonoBehaviour
{


    [Header("模擬次數")]
    public int simulations = 100000;

    [Header("倍率參數")]
    public float minMultiplier = 1.01f;
    public float maxMultiplier = 50000f;
    public float alpha = 2.0f;//控制算法尾端分布 越大高倍率越稀有 越小整體倍率越平緩

    [Header("策略選擇 (ScriptableObject)")]
    public BetStrategySO[] strategies; // 策略陣列

    [Header("UI 設定")]
    public Transform resultContainer;       
    public GameObject resultCardPrefab;     

    private System.Random rng = new System.Random();

    public void RunSimulation()
    {
        if (strategies == null || strategies.Length == 0)//未設定策略的方法
        {
            Debug.LogError("未設定策略");
            return;
        }
        ClearOldResults();
        foreach (var strategySO in strategies)
        {
            SimulateStrategy(strategySO);
        }
    }

    void SimulateStrategy(BetStrategySO strategySO)
    {
        List<double> operatorBalanceHistory = new List<double>();
        List<double> avgPlayerBalanceHistory = new List<double>();

        int totalPlayers = strategySO.playerCount; // 模擬玩家數量
        double totalProfit_AllPlayers = 0;
        double totalBet_AllPlayers = 0;
        double operatorProfit_AllPlayers = 0;
        int bankruptPlayers = 0;
        double highestMultiplier = 0;
        strategySO.isBankrupt = false;
        strategySO.bankruptRound = 0;
        // --- 廠商最大回撤相關 ---
        double operatorCapital = 0;
        double operatorPeak = 0;
        double operatorMDD = 0;

        // 每位玩家的資金初始化
        float[] playerBalances = new float[totalPlayers];
        for (int p = 0; p < totalPlayers; p++)
        {
            playerBalances[p] = strategySO.initialBalance;
        }

        // --- 進行模擬迴圈 ---
        for (int i = 0; i < simulations; i++) // 每一回合
        {
            for (int p = 0; p < totalPlayers; p++)
            {
                float balance = playerBalances[p];

                // ---- 破產 ----
                if (balance <= 0 && !strategySO.isBankrupt)
                {
                    strategySO.isBankrupt = true;
                    strategySO.bankruptRound = i + 1;
                    bankruptPlayers++;
                    continue; // 玩家沒錢 → 不再下注
                }

                // ---- 玩家下注邏輯 ----
                float betAmount = strategySO.GetBetAmount(i, balance);
                float cashoutMultiplier = strategySO.GetCashoutMultiplier(i, balance);
                float crashPoint = GenerateMultiplier();

                if (crashPoint > highestMultiplier) highestMultiplier = crashPoint;
                strategySO.RecordCrashPoint(crashPoint);

                totalBet_AllPlayers += betAmount;

                // 本回合收益計算
                double profitThisRound = (cashoutMultiplier <= crashPoint)
                    ? betAmount * (cashoutMultiplier - 1)
                    : -betAmount;

                bool playerWon = profitThisRound > 0;

                playerBalances[p] += (float)profitThisRound; // 更新玩家資金
                totalProfit_AllPlayers += profitThisRound;

                strategySO.UpdateLastResult(playerWon);

                // 廠商收益
                operatorCapital += -profitThisRound;
                operatorProfit_AllPlayers += -profitThisRound;

                // 更新 peak & MDD
                if (operatorCapital > operatorPeak)
                    operatorPeak = operatorCapital;

                if (operatorPeak > 0)
                {
                    double drawdown = (operatorPeak - operatorCapital) / operatorPeak;
                    if (drawdown > operatorMDD)
                        operatorMDD = drawdown;
                }
            }

            // ---- 回合結束後統計平均玩家餘額 ----
            double sumBalances = 0;
            for (int p = 0; p < totalPlayers; p++)
            {
                sumBalances += playerBalances[p];
            }
            double avgBalance = sumBalances / totalPlayers;

            operatorBalanceHistory.Add(operatorCapital);
            avgPlayerBalanceHistory.Add(avgBalance);
        }

        // ---- 統計結果 ----
        double avgROI = (totalProfit_AllPlayers / totalBet_AllPlayers) * 100;
        double operatorProfit = operatorProfit_AllPlayers;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== Aviator 多玩家模擬結果 =====");
        sb.AppendLine($"策略: {strategySO.GetType().Name}");
        sb.AppendLine($"玩家數: {strategySO.playerCount}");
        sb.AppendLine($"模擬次數: {simulations}");
        sb.AppendLine($"初始資金: {strategySO.initialBalance}");
        sb.AppendLine($"平均 ROI (玩家): {avgROI:F2}%");
        sb.AppendLine($"廠商總營利: {operatorProfit:F2}");
        sb.AppendLine($"廠商資金最大回撤: {operatorMDD * 100:F2}%");
        sb.AppendLine($"最高爆點倍率: {highestMultiplier:F2}x");
        sb.AppendLine($"破產玩家數: {bankruptPlayers}/{strategySO.playerCount}");
        sb.AppendLine("================================");

        string resultText = sb.ToString();
        ShowResult(resultText);

        Debug.Log(resultText);

        // 匯出曲線數據
        ExportCSV(strategySO.GetType().Name + "_CurveData", strategySO.name, operatorBalanceHistory, avgPlayerBalanceHistory);
    }


    public void ShowResult(string ResultString)
    {
       
        if (resultCardPrefab == null || resultContainer == null) return;

        
        GameObject newCard = Instantiate(resultCardPrefab, resultContainer);

        
        Text textComponent = newCard.GetComponentInChildren<Text>();

        if (textComponent != null)
        {
            textComponent.text = ResultString;
        }
    }
    void ClearOldResults()
    {
        // 遍歷 container 下的所有子物件並刪除
        foreach (Transform child in resultContainer)
        {
            Destroy(child.gameObject);
        }
    }
    float GenerateMultiplier()
    {
        double u = rng.NextDouble();
        float multiplier = (float)(minMultiplier / Math.Pow(u, 1.0 / alpha));
        return Mathf.Min(multiplier, maxMultiplier);
    }

    void ExportCSV(string filename, string strategyName, List<double> operatorHistory, List<double> playerHistory)
    {
        StringBuilder sb = new StringBuilder();


        sb.AppendLine(strategyName);


        sb.AppendLine("Round,OperatorBalance,AvgPlayerBalance");

        for (int i = 0; i < operatorHistory.Count; i++)
        {

            sb.AppendLine($"{i + 1},{operatorHistory[i]},{playerHistory[i]}");
        }

        string path = Application.dataPath + "/" + filename + ".csv";
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"曲線資料已輸出到 {path}");
    }
}