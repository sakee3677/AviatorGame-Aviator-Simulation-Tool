using UnityEngine;

// 連續兩次低倍率 (<1.5x) 才下注策略
[CreateAssetMenu(fileName = "TwoLowCrashEntryStrategy", menuName = "Aviator/Strategies/TwoLowCrashEntry")]
public class TwoLowCrashEntryStrategy : BetStrategySO
{
    [Header("策略參數")]
    public float betAmount = 1f;                  // 固定下注金額
    public float fixedCashoutMultiplier = 2f;     // 固定收手倍率
    public float lowCrashThreshold = 1.5f;        // 判斷低倍率的標準

    private int lowCrashStreak = 0;  // 連續低倍率次數
    private bool shouldBet = false;  // 下一局是否入場

    public override float GetBetAmount(int round, float balance)
    {
        if (shouldBet)
        {
            shouldBet = false; // 入場一次後重置
            return Mathf.Min(betAmount, balance);
        }
        return 0; // 不下注
    }

    public override float GetCashoutMultiplier(int round, float balance)
    {
        return fixedCashoutMultiplier;
    }

    public override void UpdateLastResult(bool win)
    {
        // 這個策略的下注邏輯完全依靠 crashPoint，不用 win
    }

    // 重點：由 Tester 傳入爆點倍率
    public override void RecordCrashPoint(float crashPoint)
    {
        if (crashPoint < lowCrashThreshold)
        {
            lowCrashStreak++;
            if (lowCrashStreak >= 5)
            {
                shouldBet = true;  // 下一輪下注
                lowCrashStreak = 0;
            }
        }
        else
        {
            lowCrashStreak = 0; // 連續紀錄斷掉
        }
    }
}
