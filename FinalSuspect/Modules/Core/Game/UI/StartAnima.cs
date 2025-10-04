using FinalSuspect.Attributes;
using FinalSuspect.Helpers;

namespace FinalSuspect.Modules.Core.Game.UI;

public class StartAnima
{
    [GameModuleInitializer]
    public static void OnInitialization()
    {
        var flash = ObjectHelper.CreateSpriteRenderer("Flash", "eye.png", 0.01f, new Vector3(0, 0, -80));
        flash.color = Color.white;
        flash.transform.SetParent(HudManager.Instance.gameObject.transform);
        _ = new LateTask(() => { Object.Destroy(flash); }, 1f, "Destroy Flash");
    }
}