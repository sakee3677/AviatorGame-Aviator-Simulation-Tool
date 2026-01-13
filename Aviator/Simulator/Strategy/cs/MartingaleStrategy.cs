using UnityEngine;

//馬丁格爾策略
[CreateAssetMenu(fileName = "MartingaleStrategy", menuName = "Aviator/Strategies/Martingale")]
public class MartingaleStrategy : BetStrategySO
{
    [Header("策略參數")]
    public float initialBet = 1f;                 // 初始下注金額
    public float fixedCashoutMultiplier = 2f;     // 固定收手倍率（玩家在該倍率收手）

    // 私有變數，用於追蹤上一輪下注金額與勝負
    private float lastBet;   // 上一輪下注金額
    private bool lastWin = true;  // 上一輪是否贏

    // 計算本輪下注金額
    public override float GetBetAmount(int round, float balance)
    {
        // 第一輪使用初始下注
        if (round == 0)
        {
            lastBet = initialBet;
            return Mathf.Min(initialBet, balance); // 確保下注不超過目前餘額
        }

        // 如果上一輪贏了，下注回到初始金額
        // 如果上一輪輸了，下注翻倍（馬丁格爾核心邏輯），但不能超過餘額
        lastBet = lastWin ? initialBet : Mathf.Min(lastBet * 2f, balance);

        return Mathf.Min(lastBet, balance); // 最終下注額度
    }

    // 本輪收手倍率固定，返回策略設定的固定倍率
    public override float GetCashoutMultiplier(int round, float balance)
    {
        return fixedCashoutMultiplier;
    }

    // 更新策略狀態：告訴策略上一輪是否勝利
    // 用於決定下一輪的下注金額（輸則翻倍，贏則回到初始下注）
    public override void UpdateLastResult(bool win)
    {
        lastWin = win;
    }
}
