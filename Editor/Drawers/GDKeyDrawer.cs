﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace ME.GD.Editor {

    [CustomPropertyDrawer(typeof(GDFloat))]
    public class GDFloatDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var gdKeyValue = property.FindPropertyRelative("key").stringValue;
            var gdKeyValueRuntime = property.FindPropertyRelative("runtimeValue")?.floatValue;
            var gdKey = new GDKey() { key = gdKeyValue, runtimeValue = gdKeyValueRuntime.ToString() };
            GDKeyDrawer.DrawGUI(position, label, this.fieldInfo, GDValueType.Float, gdKey, property.hasMultipleDifferentValues, (key) => {

                property.serializedObject.Update();
                property.FindPropertyRelative("key").stringValue = key.key;
                property.serializedObject.ApplyModifiedProperties();
    
            });

        }

    }

    [CustomPropertyDrawer(typeof(GDEnum<>))]
    public class GDEnumDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var gdKeyValue = property.FindPropertyRelative("key").stringValue;
            var gdKeyValueRuntime = property.FindPropertyRelative("runtimeValue")?.enumValueIndex;
            var gdKey = new GDKey() { key = gdKeyValue, runtimeValue = gdKeyValueRuntime.ToString() };
            GDKeyDrawer.DrawGUI(position, label, this.fieldInfo, GDValueType.Enum, gdKey, property.hasMultipleDifferentValues, (key) => {

                property.serializedObject.Update();
                property.FindPropertyRelative("key").stringValue = key.key;
                property.serializedObject.ApplyModifiedProperties();
    
            });

        }

    }

    [CustomPropertyDrawer(typeof(GDInt))]
    public class GDIntDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var gdKeyValue = property.FindPropertyRelative("key").stringValue;
            var gdKeyValueRuntime = property.FindPropertyRelative("runtimeValue")?.intValue;
            var gdKey = new GDKey() { key = gdKeyValue, runtimeValue = gdKeyValueRuntime.ToString() };
            GDKeyDrawer.DrawGUI(position, label, this.fieldInfo, GDValueType.Integer, gdKey, property.hasMultipleDifferentValues, (key) => {

                property.serializedObject.Update();
                property.FindPropertyRelative("key").stringValue = key.key;
                property.serializedObject.ApplyModifiedProperties();
    
            });

        }

    }

    [CustomPropertyDrawer(typeof(GDString))]
    public class GDStringDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var gdKeyValue = property.FindPropertyRelative("key").stringValue;
            var gdKeyValueRuntime = property.FindPropertyRelative("runtimeValue")?.stringValue;
            var gdKey = new GDKey() { key = gdKeyValue, runtimeValue = gdKeyValueRuntime };
            GDKeyDrawer.DrawGUI(position, label, this.fieldInfo, GDValueType.String, gdKey, property.hasMultipleDifferentValues, (key) => {

                property.serializedObject.Update();
                property.FindPropertyRelative("key").stringValue = key.key;
                property.serializedObject.ApplyModifiedProperties();
    
            });

        }

    }

    [CustomPropertyDrawer(typeof(GDKey))]
    public class GDKeyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var gdKeyValue = property.FindPropertyRelative("key").stringValue;
            var gdKeyValueRuntime = property.FindPropertyRelative("runtimeValue")?.stringValue;
            var gdKey = new GDKey() { key = gdKeyValue, runtimeValue = gdKeyValueRuntime };
            gdKey = GDKeyDrawer.DrawGUI(position, label, this.fieldInfo, gdKey, property.hasMultipleDifferentValues, (key) => {

                property.serializedObject.Update();
                property.FindPropertyRelative("key").stringValue = key.key;
                property.serializedObject.ApplyModifiedProperties();
    
            });
            
        }

        public static GDKey DrawGUI(Rect position, GUIContent label, System.Reflection.FieldInfo fieldInfo, GDKey value, bool hasMultipleDifferentValues, System.Action<GDKey> onChanged) {
            
            var fieldTypeAttrs = fieldInfo.GetCustomAttributes(typeof(GDFieldTypeAttribute), false);
            var fieldTypeAttr = (fieldTypeAttrs.Length > 0 ? fieldTypeAttrs[0] as GDFieldTypeAttribute : null);
            var attrType = (fieldTypeAttr != null ? fieldTypeAttr.fieldType : GDValueType.Unknown);

            return GDKeyDrawer.DrawGUI(position, label, fieldInfo, attrType, value, hasMultipleDifferentValues, onChanged);

        }

        public static GDKey DrawGUI(Rect position, GUIContent label, System.Reflection.FieldInfo fieldInfo, GDValueType valueType, GDKey value, bool hasMultipleDifferentValues, System.Action<GDKey> onChanged) {
            
            var mainData = GDSystem.active.GetData();
            var attrType = valueType;
            const float offsetY = 2f;

            var labelField = new Rect(position.x + 2f, position.y + offsetY, EditorGUIUtility.labelWidth, position.height);
            var contentField = new Rect(position.x + EditorGUIUtility.labelWidth + 4f, position.y + offsetY, position.width - EditorGUIUtility.labelWidth - 8f, position.height);
            EditorGUI.LabelField(labelField, label);
            var style = new GUIStyle(EditorStyles.textField);
            style.richText = true;
            if (GUI.Button(contentField, new GUIContent(hasMultipleDifferentValues == true ? "-" : GDSystemEditor.GetKeyCaption(value, mainData)), style) == true) {

                var rect = contentField;
                var vector = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
                rect.x = vector.x;
                rect.y = vector.y;
                var popup = new Popup() { title = "Key", autoClose = true, screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 400f) };
                popup.Item("None", null, (pItem) => {

                    value.key = string.Empty;
                    onChanged.Invoke(value);
                    
                }, searchable: false, order: -1);
                for (int i = 0; i < mainData.items.Count; ++i) {

                    var item = mainData.items[i];
                    var isSupported = false;
                    if (attrType == GDValueType.Enum && (item.type == GDValueType.String || item.type == GDValueType.Integer)) {

                        isSupported = true;

                    } else if (item.type == GDValueType.String && attrType == GDValueType.String) {

                        isSupported = true;

                    } else if (item.type == GDValueType.Integer && attrType == GDValueType.Integer) {

                        isSupported = true;

                    } else if ((item.type == GDValueType.Float || item.type == GDValueType.Integer) && attrType == GDValueType.Float) {

                        isSupported = true;

                    }
                    
                    if (isSupported == true) {
                        
                        var gdKey = new GDKey() { key = item.key };
                        popup.Item(GDSystemEditor.GetKeyCaption(gdKey, mainData), () => {

                            popup.searchText = string.Empty;
                            value.key = item.key;
                            onChanged.Invoke(value);
                            
                        });
                        
                    }
                    
                }
                popup.Show();
                popup.OnClose(() => {

                    if (string.IsNullOrEmpty(popup.searchText) == false) {
                        
                        value.key = popup.searchText;
                        onChanged.Invoke(value);
                        
                    }
                    
                });
                
            }

            return value;

        }

    }

}
