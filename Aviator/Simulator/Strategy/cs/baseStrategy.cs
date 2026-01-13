using UnityEngine;

[CreateAssetMenu(fileName = "baseStrategy", menuName = "Aviator/baseStrategy")]
public class baseStrategy : BetStrategySO
{
    public float betAmount = 10f;
    public float cashoutMultiplier = 2f;

    public override float GetBetAmount(int round, float balance)
    {
        return Mathf.Min(betAmount, balance);
    }

    public override float GetCashoutMultiplier(int round, float balance)
    {
        return cashoutMultiplier;
    }

    public override void UpdateLastResult(bool win)
    {
        // 固定策略不需要改變下注
    }
   
}