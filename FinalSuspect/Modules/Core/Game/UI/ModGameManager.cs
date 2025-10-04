using System;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FinalSuspect.Modules.Core.Game.UI;

public static class ModGameManager
{
    public static GameObject CreateFunctionalButton(
        GameObject template,
        Transform parent,
        string name,
        Vector3 localPosition,
        string iconSpriteName,
        Color inactiveColor,
        Vector3 edgeDistance,
        Action onClickAction)
    {
        var button = Object.Instantiate(template, parent);
        button.name = name;
        button.transform.localPosition = localPosition;
        button.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        button.SetActive(true);

        Object.Destroy(button.GetComponent<ConditionalHide>());

        var inactiveSprite = button.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();
        inactiveSprite.color = inactiveColor;

        var iconSprite = button.transform.FindChild("Icon").GetComponent<SpriteRenderer>();
        iconSprite.sprite = LoadSprite(iconSpriteName, 100f);

        var aspectPos = button.AddComponent<AspectPosition>();
        aspectPos.updateAlways = true;
        aspectPos.Alignment = AspectPosition.EdgeAlignments.Right;
        aspectPos.DistanceFromEdge = edgeDistance;

        var passiveButton = button.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener(new Action(onClickAction));

        return button;
    }
}