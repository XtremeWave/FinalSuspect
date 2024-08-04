using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace FinalSuspect_Xtreme;

public static class ObjectHelper
{
    /// <summary>
    /// 销毁对象的<see cref="TextTranslatorTMP"/>组件
    /// </summary>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> collection)
    {
        return collection.SelectMany(x => x);
    }
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
}
