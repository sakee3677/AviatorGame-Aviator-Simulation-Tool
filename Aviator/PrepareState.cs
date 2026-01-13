// PrepareState.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PrepareState : IState
{
    private readonly AviatorStateManager manager;
    private float timer;

    public PrepareState(AviatorStateManager mgr)
    {
        manager = mgr;
    }

    public void Enter()
    {
        manager.actionButton.interactable = true;
        manager.OBJPrepareTimerText.SetActive(true);
        timer = manager.prepareDuration;
        manager.ResetRoundForPrepare();
        manager.UpdateStatus("準備階段：請下注");
        if (manager.NextBet <= 0f)
        {
            manager.betInput.interactable = true;
            manager.UpdateButtonColor(manager.GreenColor);
            manager.UpdateButtonText("下注");
        }
        else if(manager.NextBet > 0f)
        {
            manager.betInput.interactable = false;
            manager.UpdateButtonColor(manager.RedColor);
            manager.UpdateButtonText("取消");
        }
        manager.UpdatePrepareTimer(timer);
    }

    public void Exit()
    {
        manager.OBJPrepareTimerText.SetActive(false);
        manager.UpdatePrepareTimer(0);
    }

    public void Tick(float deltaTime)
    {
        timer -= deltaTime;
        if (timer < 0) timer = 0;
        manager.UpdatePrepareTimer(timer);

        if (timer <= 0f)
        {
            // move to flying
            manager.StateMachine.ChangeState(new FlyingState(manager));
        }
    }
}
