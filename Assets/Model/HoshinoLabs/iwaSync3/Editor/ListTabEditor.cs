using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace HoshinoLabs.IwaSync3
{
    [CustomEditor(typeof(ListTab))]
    internal class ListTabEditor : IwaSync3EditorBase
    {
        ListTab _target;

        SerializedProperty _iwaSync3Property;

        SerializedProperty _allowSwitchOffProperty;
        SerializedProperty _multiViewProperty;

        SerializedProperty _tabsProperty;

        ReorderableList _tabsList;

        protected override void FindProperties()
        {
            base.FindProperties();

            _target = target as ListTab;

            _iwaSync3Property = serializedObject.FindProperty("iwaSync3");

            _allowSwitchOffProperty = serializedObject.FindProperty("allowSwitchOff");
            _multiViewProperty = serializedObject.FindProperty("multiView");

            var tabsProperty = serializedObject.FindProperty("tabs");
            if (_tabsList == null || _tabsProperty.serializedObject != _tabsProperty.serializedObject)
            {
                _tabsProperty = tabsProperty;
                _tabsList = new ReorderableList(serializedObject, tabsProperty)
                {
                    drawHeaderCallback = (rect) =>
                    {
                        EditorGUI.LabelField(rect, tabsProperty.displayName);
                    },
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        EditorGUI.PropertyField(rect, tabsProperty.GetArrayElementAtIndex(index));
                    },
                    elementHeightCallback = (index) =>
                    {
                        return EditorGUI.GetPropertyHeight(tabsProperty.GetArrayElementAtIndex(index)) + EditorGUIUtility.standardVerticalSpacing;
                    },
                    onReorderCallback = (list) =>
                    {
                        ApplyModifiedProperties();
                    }
                };
            }
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
                EditorGUILayout.PropertyField(_allowSwitchOffProperty);
                EditorGUILayout.PropertyField(_multiViewProperty);
            }

            EditorGUILayout.Space();

            _tabsList.DoLayoutList();

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

            var lists = Enumerable.Range(0, _tabsProperty.arraySize)
                .Select(x => _tabsProperty.GetArrayElementAtIndex(x))
                .Select(x => (ListBase)x.FindPropertyRelative("list").objectReferenceValue)
                .Select(x => x?.gameObject)
                .ToArray();
            var tabs = lists
                .Select(x => x == null ? false : x.activeSelf)
                .ToArray();

            if (!_multiViewProperty.boolValue)
            {
                var active = false;
                for (var i = 0; i < tabs.Length; i++)
                {
                    active = !active && tabs[i];
                    tabs[i] = active;
                }
            }

            var self = _target.GetUdonComponentInChildren<Udon.IwaSync3.ListTab>(true);
            self.SetPublicVariable("core", core);
            self.SetPublicVariable("controller", controller);
            self.SetPublicVariable("allowSwitchOff", _allowSwitchOffProperty.boolValue);
            self.SetPublicVariable("multiView", _multiViewProperty.boolValue);
            self.SetPublicVariable("_transform", self.transform.parent);
            self.SetPublicVariable("_lists", lists);
            self.SetPublicVariable("_tabs", tabs);

            var content = self.transform.Find("Canvas/Panel/Scroll View/Scroll View/Viewport/Content");
            var template = content.Find("Template");
            template.gameObject.SetActive(false);

            for (var i = content.childCount - 1; 0 < i; i--)
            {
                var item = content.GetChild(i);
                if (item == template)
                    continue;
                DestroyImmediate(item.gameObject);
            }

            for (var i = 0; i < _tabsProperty.arraySize; i++)
            {
                var track = _tabsProperty.GetArrayElementAtIndex(i);

                var obj = Instantiate(template.gameObject, content, false);
                obj.SetActive(true);
                var toggleText = obj.transform.Find("Toggle/Label").GetComponent<Text>();
                toggleText.text = track.FindPropertyRelative("title").stringValue;
                GameObjectUtility.EnsureUniqueNameForSibling(obj);
            }
        }
    }
}
