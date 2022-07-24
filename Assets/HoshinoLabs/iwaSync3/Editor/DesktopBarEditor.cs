using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    [CustomEditor(typeof(DesktopBar))]
    internal class DesktopBarEditor : IwaSync3EditorBase
    {
        DesktopBar _target;

        SerializedProperty _iwaSync3Property;

        SerializedProperty _desktopOnlyProperty;

        protected new void FindProperties()
        {
            base.FindProperties();

            _target = target as DesktopBar;

            _iwaSync3Property = serializedObject.FindProperty("iwaSync3");

            _desktopOnlyProperty = serializedObject.FindProperty("desktopOnly");
        }

        public override void OnInspectorGUI()
        {
            FindProperties();

            base.OnInspectorGUI();

            serializedObject.Update();

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Main", _italicStyle);
                var iwaSync3 = GetMainIwaSync3(null);
                if (iwaSync3)
                    EditorGUILayout.LabelField(_iwaSync3Property.displayName, "Automatically set by Script");
                else
                    EditorGUILayout.PropertyField(_iwaSync3Property);
            }

            EditorGUILayout.Space();

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Options", _italicStyle);
                EditorGUILayout.PropertyField(_desktopOnlyProperty);
            }

            if (serializedObject.ApplyModifiedProperties())
                ApplyModifiedProperties();
        }

        internal override void ApplyModifiedProperties()
        {
            FindProperties();

            var iwaSync3 = GetMainIwaSync3(_iwaSync3Property);
            if (iwaSync3 == null)
                return;
            var core = iwaSync3.GetUdonComponentInChildren<Udon.IwaSync3.VideoCore>(true);
            var controller = iwaSync3.GetUdonComponentInChildren<Udon.IwaSync3.VideoController>(true);

            var self = _target.GetUdonComponentInChildren<Udon.IwaSync3.DesktopBar>(true);
            self.SetPublicVariable("core", core);
            self.SetPublicVariable("controller", controller);
            self.SetPublicVariable("desktopOnly", _desktopOnlyProperty.boolValue);
            self.SetPublicVariable("_transform", self.transform.parent);
        }
    }
}
