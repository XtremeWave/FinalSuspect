using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace FinalSuspect_Xtreme;

public static class ObjectHelper
{

    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> collection)
    {
        return collection.SelectMany(x => x);
    }
    /// <summary>
    /// 销毁对象的<see cref="TextTranslatorTMP"/>组件
    /// </summary>
    public static void DestroyTranslator(this GameObject obj)
    {
        if (obj == null) return;
        obj.ForEachChild((Il2CppSystem.Action<GameObject>)DestroyTranslator);
        TextTranslatorTMP[] translator = obj.GetComponentsInChildren<TextTranslatorTMP>(true);
        translator?.Do(Object.Destroy);
    }
    /// <summary>
    /// 销毁对象的 <see cref="TextTranslatorTMP"/> 组件
    /// </summary>
    public static void DestroyTranslator(this MonoBehaviour obj) => obj?.gameObject?.DestroyTranslator();


    public static GameObject CreateObject(string objName, Transform parent, Vector3 localPosition, int? layer = null)
    {
        var obj = new GameObject(objName);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = new Vector3(1f, 1f, 1f);
        if (layer.HasValue) obj.layer = layer.Value;
        else if (parent != null) obj.layer = parent.gameObject.layer;
        return obj;
    }
    public static T CreateObject<T>(string objName, Transform parent, Vector3 localPosition, int? layer = null) where T : Component
    {
        return CreateObject(objName, parent, localPosition, layer).AddComponent<T>();
    }
}
