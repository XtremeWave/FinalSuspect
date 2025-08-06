using TMPro;
using UnityEngine;

namespace FinalSuspect.Modules.Core.Plugin;

public class ErrorText : MonoBehaviour
{
    public TextMeshPro Text;
    public Camera Camera;
    public Vector3 TextOffset = new(0, 0.3f, -1000f);

    public bool CheatDetected;
    public bool SBDetected;
    private readonly List<ErrorData> AllErrors = [];

    public void Update()
    {
        AllErrors.ForEach(err => err.IncreaseTimer());
        var ToRemove = AllErrors.Where(err => err.ErrorLevel <= 1 && 30f < err.Timer);
        var errorDatas = ToRemove.ToList();
        if (!errorDatas.Any()) return;
        AllErrors.RemoveAll(errorDatas.Contains);
        UpdateText();
    }

    public void LateUpdate()
    {
        if (!Text.enabled) return;

        if (!Camera)
            Camera = !HudManager.InstanceExists ? Camera.main : HudManager.Instance.PlayerCam.GetComponent<Camera>();
        if (Camera)
            transform.position =
                AspectPosition.ComputeWorldPosition(Camera, AspectPosition.EdgeAlignments.Top, TextOffset);
    }

    public static void Create(TextMeshPro baseText)
    {
        var Text = Instantiate(baseText);
        Text.fontSizeMax = Text.fontSizeMin = 2f;
        var instance = Text.gameObject.AddComponent<ErrorText>();
        instance.Text = Text;
        instance.name = "ErrorText";

        Text.enabled = false;
        Text.text = "NO ERROR";
        Text.color = Color.red;
        Text.outlineColor = Color.black;
        Text.alignment = TextAlignmentOptions.Top;
    }

    public void AddError(ErrorCode code)
    {
        var error = new ErrorData(code);
        //if (0 < error.ErrorLevel)
        //    Error($"エラー発生: {error}: {error.Message}", "ErrorText");

        if (AllErrors.All(e => e.Code != code))
            //まだ出ていないエラー
            AllErrors.Add(error);

        UpdateText();
    }

    public void UpdateText()
    {
        var text = "";
        var maxLevel = 0;
        foreach (var err in AllErrors)
        {
            text += $"{err}: {err.Message}\n";
            if (maxLevel < err.ErrorLevel) maxLevel = err.ErrorLevel;
        }

        if (maxLevel == 0)
        {
            Text.enabled = false;
        }
        else
        {
            text += $"{GetString($"ErrorLevel{maxLevel}")}";
            if (CheatDetected)
                text = SBDetected ? GetString("CheatDetected.HighLevel") : GetString("FAC.CheatDetected.LowLevel");
            Text.enabled = true;
        }

        if (IsInGame && maxLevel != 3 && !CheatDetected)
            text += $"\n{GetString("TerminateCommand")}: Shift+L+Enter";
        Text.text = text;
    }

    public void Clear()
    {
        AllErrors.RemoveAll(err => err.ErrorLevel != 3);
        UpdateText();
    }

    private class ErrorData
    {
        public readonly ErrorCode Code;
        public readonly int ErrorLevel;
        private readonly int ErrorType1;
        private readonly int ErrorType2;

        public ErrorData(ErrorCode code)
        {
            Code = code;
            ErrorType1 = (int)code / 10000;
            ErrorType2 = (int)code / 10 - ErrorType1 * 1000; // xxxyyy - xxx000
            ErrorLevel = (int)code - (int)code / 10 * 10;
            Timer = 0f;
        }

        public float Timer { get; private set; }
        public string Message => GetString(ToString());

        public override string ToString()
        {
            // ERR-xxx-yyy-z
            return $"ERR-{ErrorType1:000}-{ErrorType2:000}-{ErrorLevel:0}";
        }

        public void IncreaseTimer()
        {
            Timer += Time.deltaTime;
        }
    }

    #region Singleton

    public static ErrorText Instance { get; private set; }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    #endregion
}

public enum ErrorCode
{
    //xxxyyyz: ERR-xxx-yyy-z
    //  xxx: 错误大类
    //  yyy: 错误细类
    //  z:   严重等级
    //    0: 无需处理 (不显示)
    //    1: 运行异常需终止对局 (短暂显示)
    //    2: 建议终止对局 (终止后隐藏)
    //    3: 用户无法处理 (需持续显示)
    // =============
    // 001 主系统
    Main_DictionaryError = 0010003, // 001-000-3 主字典错误
    OptionIDDuplicate = 001_010_3, // 001-010-3 选项ID重复(仅调试版本生效)

    // 002 兼容支持
    UnsupportedVersion = 002_000_1, // 002-000-1 AmongUs版本过旧

    // ==========
    // 000 Test
    NoError = 0000000, // 000-000-0 No Error
    TestError0 = 0009000, // 000-900-0 Test Error 0
    TestError1 = 0009101, // 000-910-1 Test Error 1
    TestError2 = 0009202, // 000-920-2 Test Error 2
    TestError3 = 0009303, // 000-930-3 Test Error 3
    CheatDetected = 000_666_2, // 000-666-2 疑似存在作弊玩家
    SBDetected = 000_666_1 // 000-666-1 傻逼外挂司马东西
}