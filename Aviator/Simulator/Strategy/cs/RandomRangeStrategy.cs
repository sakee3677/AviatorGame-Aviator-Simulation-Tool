using UnityEngine;

[CreateAssetMenu(fileName = "RandomRangeStrategy", menuName = "Aviator/Strategies/RandomRange")]
public class RandomRangeStrategy : BetStrategySO
{
    [Header("下注金額範圍")]
    public float minBet = 5f;     // 最小下注金額
    public float maxBet = 50f;    // 最大下注金額

    [Header("下注間隔 (幾回合才下注一次)")]
    public int minSkipRounds = 1; // 最少間隔幾回合
    public int maxSkipRounds = 5; // 最多間隔幾回合

    [Header("兌現倍率範圍")]
    public float minCashoutMultiplier = 1.2f;
    public float maxCashoutMultiplier = 5f;

    private System.Random rng = new System.Random();

    private int nextBetRound = 0;  // 下一次下注的回合數
    private float currentBet;      // 本輪隨機選的下注金額
    private float currentCashout;  // 本輪隨機選的兌現倍率

    public override float GetBetAmount(int round, float balance)
    {
        if (round < nextBetRound || balance <= 0)
        {
            currentBet = 0f;       // 沒下注就設0
            currentCashout = 0f;   // 沒下注就設0
            return 0f;
        }

        // 生成隨機下注金額 & 兌現倍率
        currentBet = Mathf.Min((float)(minBet + rng.NextDouble() * (maxBet - minBet)), balance);
        currentCashout = (float)(minCashoutMultiplier + rng.NextDouble() * (maxCashoutMultiplier - minCashoutMultiplier));

        // 決定下一次下注回合
        int skip = rng.Next(minSkipRounds, maxSkipRounds + 1);
        nextBetRound = round + skip;

        return currentBet;
    }

    public override float GetCashoutMultiplier(int round, float balance)
    {
        if (currentBet <= 0f)
            return 0f; // 沒下注就回傳0，不影響 profit
        return currentCashout;
    }


    public override void UpdateLastResult(bool win)
    {
        // 這個策略跟勝負無關，不需要更新狀態
    }
}
