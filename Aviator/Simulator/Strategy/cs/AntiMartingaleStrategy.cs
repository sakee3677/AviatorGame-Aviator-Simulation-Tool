using UnityEngine;

// 反馬丁格爾策略 (Anti-Martingale / Paroli)
[CreateAssetMenu(fileName = "AntiMartingaleStrategy", menuName = "Aviator/Strategies/AntiMartingale")]
public class AntiMartingaleStrategy : BetStrategySO
{
    [Header("策略參數")]
    public float initialBet = 1f;                 // 初始下注金額
    public float fixedCashoutMultiplier = 2f;     // 固定收手倍率

    private float lastBet;   // 上一輪下注金額
    private bool lastWin = false; // 上一輪是否贏

    public override float GetBetAmount(int round, float balance)
    {
        if (round == 0)
        {
            lastBet = initialBet;
            return Mathf.Min(initialBet, balance);
        }

        // 贏了 → 加倍下注
        // 輸了 → 回到初始下注
        lastBet = lastWin ? Mathf.Min(lastBet * 2f, balance) : initialBet;

        return Mathf.Min(lastBet, balance);
    }

    public override float GetCashoutMultiplier(int round, float balance)
    {
        return fixedCashoutMultiplier;
    }

    public override void UpdateLastResult(bool win)
    {
        lastWin = win;
    }
}
