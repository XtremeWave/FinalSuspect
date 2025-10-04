using TMPro;

namespace FinalSuspect.Templates;

public static class TMPTemplate
{
    private static TextMeshPro _baseTMP;

    public static void SetBase(TextMeshPro tmp)
    {
        if (_baseTMP) return;

        _baseTMP = Object.Instantiate(tmp);
        Object.Destroy(_baseTMP.GetComponent<AspectPosition>());
        Object.DontDestroyOnLoad(_baseTMP);
        _baseTMP.gameObject.SetActive(false);
        _baseTMP.gameObject.name = "TMPTemplateBase";
    }

    public static TextMeshPro Create(
        string name,
        string text = null,
        Color? color = null,
        float? fontSize = null,
        TextAlignmentOptions? alignment = null,
        bool setActive = false,
        Transform parent = null
    )
    {
        var replicatedObject = !parent
            ? Object.Instantiate(_baseTMP)
            : Object.Instantiate(_baseTMP, parent);
        replicatedObject.text = text ?? "";
        replicatedObject.color = color ?? Color.white;
        replicatedObject.fontSize =
            replicatedObject.fontSizeMax =
                replicatedObject.fontSizeMin = fontSize ?? _baseTMP.fontSize;
        replicatedObject.alignment = alignment ?? TextAlignmentOptions.Center;

        replicatedObject.gameObject.SetActive(setActive);
        replicatedObject.gameObject.name = name;

        return replicatedObject;
    }
}