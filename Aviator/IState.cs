// IState.cs
public interface IState
{
    void Enter();
    void Exit();
    void Tick(float deltaTime); // 因為 State 在 Manager 的 Update 被呼叫
}
