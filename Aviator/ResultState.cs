// ResultState.cs
using UnityEngine;
using System.Collections;

public class ResultState : IState
{
    private readonly AviatorStateManager manager;
    private float timer = 0f;
    private const float showResultDuration = 2f;

    public ResultState(AviatorStateManager mgr)
    {
        manager = mgr;
    }

    public void Enter()
    {
        manager.actionButton.interactable = false;
        manager.HandleEndOfRound();
        manager.UpdateStatus("結算中");
        manager.displayMultiplierText.text= "飛走了\n"+manager.realMultiplierText.text;
        timer = showResultDuration;
        manager.AddMultipleRecord();
    }

    public void Exit()
    {
        manager.OBJdisplayMultiplierText.SetActive(false);
        // nothing
    }

    public void Tick(float deltaTime)
    {
        timer -= deltaTime;
        if (timer <= 0f)
        {
            // 回到準備階段
            manager.StateMachine.ChangeState(new PrepareState(manager));
        }
    }
}
