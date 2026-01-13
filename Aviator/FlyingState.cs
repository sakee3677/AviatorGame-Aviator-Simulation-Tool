// FlyingState.cs
using UnityEngine;

public class FlyingState : IState
{
    private readonly AviatorStateManager manager;
    private float elapsed = 0f;

    public FlyingState(AviatorStateManager mgr)
    {
        manager = mgr;
    }

    public void Enter()
    {
        manager.OBJdisplayMultiplierText.SetActive(true);
        elapsed = 0f;
        manager.StartFlying();
        manager.UpdateStatus("飛行中");
      
    }

    public void Exit()
    {
      
        // nothing special here
    }

    public void Tick(float deltaTime)
    {

        
        if (manager.HasActiveBet)
        {
            float plrprofit = manager.CurrentBet * manager.displayMultiplier;
            manager.UpdateButtonColor(manager.OrgColor);
            manager.UpdateButtonText("兌現" + plrprofit.ToString("F1"));
        }


            elapsed += deltaTime;
            // 增長倍數（你可以換成更複雜曲線）
            manager.IncreaseDisplayMultiplier(deltaTime);
      
        // 如果 display 大於或等於 crash point → 進入結算
        if (manager.displayMultiplier >= manager.CurrentCrashMultiplier - Mathf.Epsilon)
        {
            manager.StateMachine.ChangeState(new ResultState(manager));
        }
    }
}
