using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace HoshinoLabs.IwaSync3
{
    internal static class UdonBehaviourExtensions
    {
        internal static object GetPublicVariable(this UdonBehaviour self, string symbolName)
        {
            return null;
        }

        internal static void SetPublicVariable<T>(this UdonBehaviour self, string symbolName, T value)
        {
            self.publicVariables.RemoveVariable(symbolName);
            var type = typeof(UdonVariable<>).MakeGenericType(typeof(T));
            self.publicVariables.TryAddVariable((IUdonVariable)Activator.CreateInstance(type, symbolName, value));
        }
    }
}
