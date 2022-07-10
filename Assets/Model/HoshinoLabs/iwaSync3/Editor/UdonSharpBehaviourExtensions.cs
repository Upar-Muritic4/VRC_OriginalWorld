using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEngine;
using VRC.Udon;

namespace HoshinoLabs.IwaSync3
{
    internal static class UdonSharpBehaviourExtensions
    {
        #region Utility
        static UdonBehaviour ConvertToUdonSharpComponent(UdonBehaviour[] behaviours, Type type)
            => behaviours.Where(x => UdonSharpEditorUtility.GetUdonSharpBehaviourType(x) == type).FirstOrDefault();

        static UdonBehaviour ConvertToUdonSharpComponent<T>(UdonBehaviour[] behaviours) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponent(behaviours, typeof(T));

        static UdonBehaviour[] ConvertToUdonSharpComponents(UdonBehaviour[] behaviours, Type type)
            => behaviours.Where(x => UdonSharpEditorUtility.GetUdonSharpBehaviourType(x) == type).ToArray();

        static UdonBehaviour[] ConvertToUdonSharpComponents<T>(UdonBehaviour[] behaviours) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponents(behaviours, typeof(T));
        #endregion

        #region GetComponent
        internal static UdonBehaviour GetUdonComponent<T>(this GameObject gameObject) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponent<T>(gameObject.GetComponents<UdonBehaviour>());

        internal static UdonBehaviour GetUdonComponent(this GameObject gameObject, Type type)
            => ConvertToUdonSharpComponent(gameObject.GetComponents<UdonBehaviour>(), type);

        internal static UdonBehaviour GetUdonComponent<T>(this Component component) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponent<T>(component.GetComponents<UdonBehaviour>());

        internal static UdonBehaviour GetUdonComponent(this Component component, Type type)
            => ConvertToUdonSharpComponent(component.GetComponents<UdonBehaviour>(), type);
        #endregion

        #region GetComponents
        internal static UdonBehaviour[] GetUdonComponents<T>(this GameObject gameObject) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponents<T>(gameObject.GetComponents<UdonBehaviour>());

        internal static UdonBehaviour[] GetUdonComponents(this GameObject gameObject, Type type)
            => ConvertToUdonSharpComponents(gameObject.GetComponents<UdonBehaviour>(), type);

        internal static UdonBehaviour[] GetUdonComponents<T>(this Component component) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponents<T>(component.GetComponents<UdonBehaviour>());

        internal static UdonBehaviour[] GetUdonComponents(this Component component, Type type)
            => ConvertToUdonSharpComponents(component.GetComponents<UdonBehaviour>(), type);
        #endregion

        #region GetComponentInChildren
        internal static UdonBehaviour GetUdonComponentInChildren<T>(this GameObject gameObject) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponent<T>(gameObject.GetComponentsInChildren<UdonBehaviour>());

        internal static UdonBehaviour GetUdonComponentInChildren(this GameObject gameObject, Type type)
            => ConvertToUdonSharpComponent(gameObject.GetComponentsInChildren<UdonBehaviour>(), type);

        internal static UdonBehaviour GetUdonComponentInChildren<T>(this GameObject gameObject, bool includeInactive) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponent<T>(gameObject.GetComponentsInChildren<UdonBehaviour>(includeInactive));

        internal static UdonBehaviour GetUdonComponentInChildren(this GameObject gameObject, Type type, bool includeInactive)
            => ConvertToUdonSharpComponent(gameObject.GetComponentsInChildren<UdonBehaviour>(includeInactive), type);

        internal static UdonBehaviour GetUdonComponentInChildren<T>(this Component component) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponent<T>(component.GetComponentsInChildren<UdonBehaviour>());

        internal static UdonBehaviour GetUdonComponentInChildren(this Component component, Type type)
            => ConvertToUdonSharpComponent(component.GetComponentsInChildren<UdonBehaviour>(), type);

        internal static UdonBehaviour GetUdonComponentInChildren<T>(this Component component, bool includeInactive) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponent<T>(component.GetComponentsInChildren<UdonBehaviour>(includeInactive));

        internal static UdonBehaviour GetUdonComponentInChildren(this Component component, Type type, bool includeInactive)
            => ConvertToUdonSharpComponent(component.GetComponentsInChildren<UdonBehaviour>(includeInactive), type);
        #endregion

        #region GetComponentsInChildren
        internal static UdonBehaviour[] GetUdonComponentsInChildren<T>(this GameObject gameObject) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponents<T>(gameObject.GetComponentsInChildren<UdonBehaviour>());

        internal static UdonBehaviour[] GetUdonComponentsInChildren(this GameObject gameObject, Type type)
            => ConvertToUdonSharpComponents(gameObject.GetComponentsInChildren<UdonBehaviour>(), type);

        internal static UdonBehaviour[] GetUdonComponentsInChildren<T>(this GameObject gameObject, bool includeInactive) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponents<T>(gameObject.GetComponentsInChildren<UdonBehaviour>(includeInactive));

        internal static UdonBehaviour[] GetUdonComponentsInChildren(this GameObject gameObject, Type type, bool includeInactive)
            => ConvertToUdonSharpComponents(gameObject.GetComponentsInChildren<UdonBehaviour>(includeInactive), type);

        internal static UdonBehaviour[] GetUdonComponentsInChildren<T>(this Component component) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponents<T>(component.GetComponentsInChildren<UdonBehaviour>());

        internal static UdonBehaviour[] GetUdonComponentsInChildren(this Component component, Type type)
            => ConvertToUdonSharpComponents(component.GetComponentsInChildren<UdonBehaviour>(), type);

        internal static UdonBehaviour[] GetUdonComponentsInChildren<T>(this Component component, bool includeInactive) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponents<T>(component.GetComponentsInChildren<UdonBehaviour>(includeInactive));

        internal static UdonBehaviour[] GetUdonComponentsInChildren(this Component component, Type type, bool includeInactive)
            => ConvertToUdonSharpComponents(component.GetComponentsInChildren<UdonBehaviour>(includeInactive), type);
        #endregion

        #region GetComponentInParent
        internal static UdonBehaviour GetUdonComponentInParent<T>(this GameObject gameObject) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponent<T>(gameObject.GetComponentsInParent<UdonBehaviour>());

        internal static UdonBehaviour GetUdonComponentInParent(this GameObject gameObject, Type type)
            => ConvertToUdonSharpComponent(gameObject.GetComponentsInParent<UdonBehaviour>(), type);

        internal static UdonBehaviour GetUdonComponentInParent<T>(this Component component) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponent<T>(component.GetComponentsInParent<UdonBehaviour>());

        internal static UdonBehaviour GetUdonComponentInParent(this Component component, Type type)
            => ConvertToUdonSharpComponent(component.GetComponentsInParent<UdonBehaviour>(), type);
        #endregion

        #region GetComponentsInParent
        internal static UdonBehaviour[] GetUdonComponentsInParent<T>(this GameObject gameObject) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponents<T>(gameObject.GetComponentsInParent<UdonBehaviour>());

        internal static UdonBehaviour[] GetUdonComponentsInParent(this GameObject gameObject, Type type)
            => ConvertToUdonSharpComponents(gameObject.GetComponentsInParent<UdonBehaviour>(), type);

        internal static UdonBehaviour[] GetUdonComponentsInParent<T>(this GameObject gameObject, bool includeInactive) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponents<T>(gameObject.GetComponentsInParent<UdonBehaviour>(includeInactive));

        internal static UdonBehaviour[] GetUdonComponentsInParent(this GameObject gameObject, Type type, bool includeInactive)
            => ConvertToUdonSharpComponents(gameObject.GetComponentsInParent<UdonBehaviour>(includeInactive), type);

        internal static UdonBehaviour[] GetUdonComponentsInParent<T>(this Component component) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponents<T>(component.GetComponentsInParent<UdonBehaviour>());

        internal static UdonBehaviour[] GetUdonComponentsInParent(this Component component, Type type)
            => ConvertToUdonSharpComponents(component.GetComponentsInParent<UdonBehaviour>(), type);

        internal static UdonBehaviour[] GetUdonComponentsInParent<T>(this Component component, bool includeInactive) where T : UdonSharpBehaviour
            => ConvertToUdonSharpComponents<T>(component.GetComponentsInParent<UdonBehaviour>(includeInactive));

        internal static UdonBehaviour[] GetUdonComponentsInParent(this Component component, Type type, bool includeInactive)
            => ConvertToUdonSharpComponents(component.GetComponentsInParent<UdonBehaviour>(includeInactive), type);
        #endregion
    }
}
