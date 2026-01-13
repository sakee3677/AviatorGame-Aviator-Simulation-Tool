using UnityEngine;

// 下注策略介面
public interface IBetStrategy
{
    float GetBetAmount(int round, float balance);
    float GetCashoutMultiplier(int round, float balance);
    void UpdateLastResult(bool win);
}

public abstract class BetStrategySO : ScriptableObject, IBetStrategy
{
    [Header("玩家設定")]//共用設定
    public int playerCount = 1; // 玩家數量 (在 Inspector 中可調整)
    public float initialBalance = 100f;

    [HideInInspector] public bool isBankrupt;
    [HideInInspector] public int bankruptRound = -1;

    public abstract float GetBetAmount(int round, float balance);
    public abstract float GetCashoutMultiplier(int round, float balance);
    public abstract void UpdateLastResult(bool win);
    public virtual void RecordCrashPoint(float crashPoint) { }
}

