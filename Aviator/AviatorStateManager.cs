// AviatorStateManager.cs
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using JetBrains.Annotations;
using UnityEngine.SocialPlatforms.Impl;


public class AviatorStateManager : MonoBehaviour
{

    [Header("UI")]
    public InputField betInput;
    public Button actionButton;
    public Text betText;
    public Text balanceText;
    public Text profitText;
    public Text realMultiplierText;    // 已算出的（最終）倍數，備於結果顯示
    public Text displayMultiplierText; // 正在跑給玩家看的倍數
    public Text statusText;
    public Text prepareTimerText;
    public GameObject OBJPrepareTimerText ;
    public GameObject OBJdisplayMultiplierText;
    public GameObject RecordBackGround;
    public int maxRecords;


    [Header("Game Settings")]
    public float prepareDuration = 5f;
    public float minMultiplier = 1.1f;
    public float maxMultiplier = 100f;
    public float alpha = 1f; // 指數分布參數（更大代表高倍更稀少）
    public float startBalance = 1000f;

    [Header("Animation Settings")]
    public float displayMultiplier = 1f;
    public float displayStartSpeed = 0.5f;
    public float displayAcceleration = 0.5f;
    public float displayMaxSpeed = 8f;


    [HideInInspector]
    public Color32 GreenColor = new Color32(72, 255, 78, 255);
    [HideInInspector]
    public Color32 RedColor = new Color32(229, 80, 72, 255);
    [HideInInspector]
    public Color32 OrgColor = new Color32(250, 162, 53, 255);


    // 狀態 & 內部變數
    public StateMachine StateMachine { get; private set; }
    public float CurrentCrashMultiplier { get; private set; } // 真實隨機 crash 點（real）
    public bool HasActiveBet { get; private set; }
    public float CurrentBet { get; private set; }
    public float NextBet { get; private set; }
    public float PlayerBalance { get; private set; }
    public float PlatformProfit { get; private set; }


    // 內部使用的動態值
    private float displaySpeed;
    private System.Random rng = new System.Random();

    private void Awake()
    {
        StateMachine = new StateMachine();
    }

    private void Start()
    {
      
        PlayerBalance = startBalance;
        PlatformProfit = 0f;
        HasActiveBet = false;
        CurrentBet = 0f;
        NextBet = 0f;

        actionButton.onClick.AddListener(OnActionButton);
        // 先進入 Prepare state
        StateMachine.ChangeState(new PrepareState(this));
        UpdateUI();
    }

    private void Update()
    {
        StateMachine.Tick(Time.deltaTime);
    }


    // 動作呼叫


    // 每次進入 Prepare 前呼叫（Reset 顯示相關）
    public void ResetRoundForPrepare()
    {
        // reset display values
        displayMultiplier = 1f;
        displaySpeed = displayStartSpeed;
        displayMultiplierText.text = displayMultiplier.ToString("F2") + "x";

        // 決定本輪投注：如果玩家在準備階段有輸入（HasActiveBet already true），就使用 CurrentBet
        // 否則若 NextBet > 0，我們嘗試以 NextBet 作為本輪投注（前提玩家有錢）
        if (!HasActiveBet && NextBet > 0f && PlayerBalance >= NextBet)//確認如準備時間無下注，則確認上輪在跑時有無下注
        {
            actionButton.GetComponent<Image>().color = OrgColor;
            CurrentBet = NextBet;
            PlayerBalance -= CurrentBet;
            HasActiveBet = true;
            betText.text = "本輪投注: " + CurrentBet.ToString("F2");
            UpdateUI();
        }
        else if (!HasActiveBet && NextBet <= 0f)//無下注
        {
            actionButton.GetComponent<Image>().color = GreenColor;
            CurrentBet = 0f;
            betText.text = "本輪投注: 0.00";
            UpdateUI();
        }

        // 如果玩家沒在準備階段輸入新注，但 NextBet 有值，我們可自動承接
        // (這裡不自動扣款，直到實際開始飛行才扣）
        // UI 顯示保持
    }

    public void StartFlying()
    {
       
          // 生成本輪的 crash multiplier（伺服器端應產生並簽名）
        CurrentCrashMultiplier = GenerateMultiplier();

        // 設定 real multiplier 顯示，讓玩家在結果階段看到（或可設為「?」直到結果）
        realMultiplierText.text = CurrentCrashMultiplier.ToString("F2") + "x";

        NextBet = 0f;//清除下次下注金額
        // init display values
        displayMultiplier = 1f;
        displaySpeed = displayStartSpeed;
        displayMultiplierText.text = displayMultiplier.ToString("F2") + "x";
    }

    public void IncreaseDisplayMultiplier(float dt)
    {
      
        // 越靠近 crash，增加速率（簡單的加速邏輯）
        displaySpeed = Mathf.Min(displaySpeed + displayAcceleration * dt, displayMaxSpeed);
        displayMultiplier += displaySpeed * dt;

        if (displayMultiplier > CurrentCrashMultiplier)
            displayMultiplier = CurrentCrashMultiplier;

        displayMultiplierText.text = displayMultiplier.ToString("F2") + "x";
    }

    // 在 Result State 觸發：處理結算
    public void HandleEndOfRound()
    {
        realMultiplierText.text = CurrentCrashMultiplier.ToString("F2") + "x";

        // 若玩家有下注且沒有收網 → 輸掉注額
        if (HasActiveBet)
        {
            // 若玩家尚未 cash out（HasActiveBet true, CurrentBet > 0）
            // 我們把下注金額加到平台利潤

            PlatformProfit += CurrentBet;
            UpdateStatus($"飛走了！玩家輸掉 {CurrentBet:F2}");
            // 清掉本輪 bet
            CurrentBet = 0f;
            HasActiveBet = false;
        }
        UpdateUI();
    }

    // 玩家按按鈕（不同 state 有不同意義）
    private void OnActionButton()
    {
        var state = StateMachine.CurrentState;
      
        if (state is PrepareState )  // 如過是Prepare state 則 TryPlaceBetFromInput();下注
        {
            Debug.Log("Prepare state 下注");
            TryPlaceBetFromInput();
        }
        else // 假定其它狀態是 Flying 或 Result
        {
            // 假定其它狀態是 Flying 或 Result  Flying: 有下注 -> 收網 ;  無下注 -> 設定 nextBet（下輪投注）
     
            if (state is FlyingState)
            {
              
                if (HasActiveBet)//有下注>收網
                {
                    Debug.Log("兌現");
                    betInput.interactable = true;
                    float payout = CurrentBet * displayMultiplier;
                    float PlatformLose = CurrentBet * displayMultiplier - CurrentBet;
                    PlayerBalance += payout;
                    PlatformProfit -= PlatformLose;
                    UpdateStatus($"已兌現: {payout:F2}");
                    // 清除 bet
                    CurrentBet = 0f;
                    NextBet = 0f;
                    HasActiveBet = false;
                    UpdateButtonColor(GreenColor);
                    UpdateButtonText("下注");
                    UpdateUI();
                }
                else if(!HasActiveBet && NextBet<=0)   // 無下注，設定下一輪投注金額（但不扣款，直到下一輪飛行時才扣）
                {
                    Debug.Log("投注下一輪金額");
                    float parsed;
                    if (float.TryParse(betInput.text, out parsed) && parsed > 0f)
                    {
                        actionButton.GetComponent<Image>().color = RedColor;
                        NextBet = parsed;
                        betInput.interactable = false;
                        UpdateButtonText("取消\n下一回合投注金額" + NextBet.ToString("F1")+"USD");
                    }
                    else
                    {
                        UpdateStatus("下輪投注金額不合法");
                    }
                }
                else if (!HasActiveBet && NextBet >= 0) // 已有下注下一輪金額，在按一次取消
                {
                    Debug.Log("取消投注下一輪金額");
                    float parsed;
                    if (float.TryParse(betInput.text, out parsed) && parsed > 0f)
                    {
                        actionButton.GetComponent<Image>().color = GreenColor;

                        betInput.interactable = true;
                        NextBet -= parsed;
                        UpdateButtonColor(GreenColor);
                        UpdateButtonText("下注");
                        UpdateUI();
                    }
                }
            }
            else
            {
                // Result state -> 按鈕暫時無效
                UpdateStatus("結算中，請稍等");
            }
        }
    }

    private void TryPlaceBetFromInput()
    {
        
        float parsed;
        if (float.TryParse(betInput.text, out parsed) && parsed > 0f && parsed <= PlayerBalance && !HasActiveBet && NextBet<=0) // 在準備階段直接下注
        {
           
            Debug.Log("已下注");
            betInput.interactable = false;
            CurrentBet = parsed;
            PlayerBalance -= CurrentBet;
            HasActiveBet = true;
           // NextBet = CurrentBet; // 預設下次也可延續
            UpdateStatus($"下注成功：{CurrentBet:F2}");
            UpdateButtonText("取消");
            betText.text = $"本輪投注: {CurrentBet:F2}";
            UpdateButtonColor(RedColor);
            UpdateUI();
        }
        else if(float.TryParse(betInput.text, out parsed) && parsed > 0f && parsed <= PlayerBalance && HasActiveBet && NextBet <= 0)//如果已下注再次按下按鈕取消本次下注
        {
            Debug.Log("取消下注");
            betInput.interactable = true;
            PlayerBalance += CurrentBet;
            CurrentBet -= parsed;
            HasActiveBet = false;
            UpdateButtonText("下注");
            UpdateButtonColor(GreenColor);
            UpdateUI();
        }
        else if (HasActiveBet && NextBet > 0)//在上一輪，已有設定下一輪投注金額的情況
        {
            Debug.Log("取消下注");
            betInput.interactable = true;
            PlayerBalance += CurrentBet;
            CurrentBet -= NextBet;
            NextBet = 0f;
            HasActiveBet = false;
            UpdateButtonText("下注");
            UpdateButtonColor(GreenColor);
            UpdateUI();
        }
        else
        {
            UpdateStatus("下注金額不合法或餘額不足");
        }
    }

    // UI helper
    public void UpdateUI()
    {
        balanceText.text = $"玩家餘額: {PlayerBalance:F2}";
        profitText.text = $"平台利潤: {PlatformProfit:F2}";
        betText.text = $"本輪投注: {CurrentBet:F2}";
        displayMultiplierText.text = $"{displayMultiplier:F2}x";
        realMultiplierText.text = CurrentCrashMultiplier > 0 ? $"{CurrentCrashMultiplier:F2}x" : "?";
    }

    public void UpdateStatus(string s)
    {
        statusText.text = s;
    }

    public void UpdateButtonText(string s)
    {
        var t = actionButton.GetComponentInChildren<Text>();
        if (t != null) t.text = s;
    }

    public void UpdatePrepareTimer(float secondsLeft)
    {
        prepareTimerText.text = $"倒數: {secondsLeft:F1}s";
    }
    public void UpdateButtonColor(Color32 colors)
    {
        actionButton.GetComponent<Image>().color = colors;
    }

    public void AddMultipleRecord()
    {
        GameObject Record = new GameObject(realMultiplierText.text);
        Record.transform.SetParent(RecordBackGround.transform, false);
        Text text = Record.AddComponent<Text>();
        Record.transform.SetAsFirstSibling();

        text.text = realMultiplierText.text;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 26;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        if (RecordBackGround.transform.childCount > maxRecords)
        {
            Transform lastChild = RecordBackGround.transform.GetChild(RecordBackGround.transform.childCount - 1);
            Destroy(lastChild.gameObject);
        }
    }

    // 倍數生成（指數分布）
    private float GenerateMultiplier()
    {
        // Pareto 分布生成倍率
        double u = rng.NextDouble();
        float multiplier = (float)(minMultiplier / Math.Pow(u, 1.0 / alpha));
        return Mathf.Min(multiplier, maxMultiplier);
    }
}
