using Il2CppSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.Helpers;

public static class ObjectHelper
{
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> collection)
    {
        return collection.SelectMany(x => x);
    }

    /// <summary>
    ///     销毁对象的<see cref="TextTranslatorTMP" />组件
    /// </summary>
    public static void DestroyTranslator(this GameObject obj)
    {
        if (!obj) return;
        obj.ForEachChild((Action<GameObject>)(x => DestroyTranslator(x)));
        TextTranslatorTMP[] translator = obj.GetComponentsInChildren<TextTranslatorTMP>(true);
        translator?.Do(Object.Destroy);
    }

    /// <summary>
    ///     销毁对象的 <see cref="TextTranslatorTMP" /> 组件
    /// </summary>
    public static void DestroyTranslator(this MonoBehaviour obj)
    {
        obj?.gameObject.DestroyTranslator();
    }

    private static GameObject CreateObject(string objName, Transform parent, Vector3 localPosition, int? layer = null)
    {
        var obj = new GameObject(objName);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = new Vector3(1f, 1f, 1f);
        if (layer.HasValue) obj.layer = layer.Value;
        else if (parent) obj.layer = parent.gameObject.layer;
        return obj;
    }

    private static T CreateObject<T>(string objName, Transform parent, Vector3 localPosition, int? layer = null)
        where T : Component
    {
        return CreateObject(objName, parent, localPosition, layer).AddComponent<T>();
    }

    public static SpriteRenderer CreateSpriteRenderer(string name, string spriteName, float pixelsPerUnit,
        Vector3 position, Transform parent = null)
    {
        var renderer = CreateObject<SpriteRenderer>(name, null, position);
        if (parent != null)
            renderer.gameObject.transform.SetParent(parent);
        renderer.gameObject.transform.localPosition = position;
        renderer.sprite = LoadSprite(spriteName, pixelsPerUnit);
        renderer.color = Color.clear;

        return renderer;
    }
}