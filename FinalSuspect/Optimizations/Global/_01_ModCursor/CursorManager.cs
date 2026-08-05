namespace FinalSuspect.Optimizations._1_ModCursor;

public static class CursorManager
{
    public static void SetCursor()
    {
        try
        {
            var sprite = LoadSprite("Cursor.png");
            Cursor.SetCursor(ConfigManager.UseModCursor.Value ? sprite.texture : null, Vector2.zero, CursorMode.Auto);
        }
        catch
        {
            ConfigManager.UseModCursor.Value = false;
        }
    }
}