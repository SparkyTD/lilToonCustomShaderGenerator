using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Koturn.LilToonCustomGenerator.Editor.Enums;


namespace Koturn.LilToonCustomGenerator.Editor.Windows
{
    public class PropertyReorderableList : ReorderableList
    {
        /// <summary>
        /// Width margin.
        /// </summary>
        private const float WidthMargin = 2.0f;
        private static readonly GUIContent[] _rangeMinMaxLabel =
        {
            new GUIContent("Min", "Minimum value of the range"),
            new GUIContent("Max", "Maximum value of the range")
        };

        private static readonly GUIContent[] _defaultVectorLabel =
        {
            new GUIContent("X"),
            new GUIContent("Y"),
            new GUIContent("Z"),
            new GUIContent("W"),
        };

        private static readonly GUIContent _defaultValueLabel = new GUIContent("Default");

        /// <summary>
        /// Cache array of min/max values of the range property.
        /// </summary>
        private readonly float[] _rangeMinMaxArray = new float[2];
        /// <summary>
        /// Cache array of default vector components.
        /// </summary>
        private readonly float[] _defaultVectorArray = new float[4];


        /// <summary>
        /// Shader property definition list.
        /// </summary>
        private readonly List<ShaderPropertyDefinition> _shaderPropDefList;

        public PropertyReorderableList(SerializedObject serializedObject, SerializedProperty serializedProperty, List<ShaderPropertyDefinition> shaderPropDefList)
            : base(serializedObject, serializedProperty, true, true, true, true)
        {
            _shaderPropDefList = shaderPropDefList;
            drawHeaderCallback = DrawHeader;
            elementHeightCallback = GetElementHeight;
            drawElementCallback = DrawElement;
            onAddCallback = OnAdd;
        }

        /// <summary>
        /// <para>Callback method for <see cref="ReorderableList.drawHeaderCallback"/>.</para>
        /// <para>Draw header of this <see cref="ReorderableList"/>.</para>
        /// </summary>
        /// <param name="rect"></param>
        public void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Shader Property Definitions");
        }

        /// <summary>
        /// <para>Callback method for <see cref="ReorderableList.elementHeight"/>.</para>
        /// <para>Returns height of the element of the specified index.</para>
        /// </summary>
        /// <param name="index">Element index. (unused)</param>
        /// <returns>Height of the element of the specified index.</returns>
        private float GetElementHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight * 2.0f + 8.0f;
        }

        /// <summary>
        /// <para>Callback method for <see cref="ReorderableList.drawElementCallback"/>.</para>
        /// <para>Draw single element.</para>
        /// </summary>
        /// <param name="rect">Draw target <see cref="Rect"/>.</param>
        /// <param name="index">Element index.</param>
        /// <param name="isActive">True if the element is active, otherwise false.</param>
        /// <param name="isFocused">True if the element is focused, otherwise false.</param>
        public void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = serializedProperty.GetArrayElementAtIndex(index);

            var line = EditorGUIUtility.singleLineHeight;
            var padding = 2.0f;

            //
            // First line.
            //
            rect.y += padding;

            var row1 = new Rect(rect.x, rect.y, rect.width, line);
            var nameWidth = row1.width * 0.3f;
            var descWidth = row1.width * 0.7f;

            EditorGUI.PropertyField(
                new Rect(row1.x, row1.y, nameWidth - WidthMargin, line),
                element.FindPropertyRelative(nameof(ShaderPropertyDefinition.name)),
                new GUIContent("Property Name"));

            EditorGUI.PropertyField(
                new Rect(row1.x + nameWidth, row1.y, descWidth, line),
                element.FindPropertyRelative(nameof(ShaderPropertyDefinition.description)));

            //
            // Second line.
            //
            rect.y += line + padding;
            var row2 = new Rect(rect.x, rect.y, rect.width, line);

            var col1 = row2.width * 0.4f;
            var col2 = row2.width * 0.2f;
            var col3 = row2.width * 0.4f;

            var propPropertyType = element.FindPropertyRelative(nameof(ShaderPropertyDefinition.propertyType));
            var propUniformType = element.FindPropertyRelative(nameof(ShaderPropertyDefinition.uniformType));
            using (var ccScope = new EditorGUI.ChangeCheckScope())
            {
                if ((ShaderPropertyType)propPropertyType.intValue == ShaderPropertyType.Range)
                {
                    propPropertyType.intValue = EditorGUI.Popup(
                        new Rect(row2.x, row2.y, col1 * 0.5f - WidthMargin, line),
                        "Variable type",
                        propPropertyType.intValue,
                        ShaderPropertyDefinition.PropertyTypeSelections);

                    var propRangeMinMax = element.FindPropertyRelative(nameof(ShaderPropertyDefinition.rangeMinMax));
                    var rangeMinMax = propRangeMinMax.vector2Value;
                    var rangeMinMaxArray = _rangeMinMaxArray;

                    rangeMinMaxArray[0] = rangeMinMax.x;
                    rangeMinMaxArray[1] = rangeMinMax.y;

                    EditorGUI.MultiFloatField(
                        new Rect(row2.x + col1 * 0.5f, row2.y, col1 * 0.5f - WidthMargin, line),
                        _rangeMinMaxLabel,
                        rangeMinMaxArray);

                    propRangeMinMax.vector2Value = new Vector2(rangeMinMaxArray[0], rangeMinMaxArray[1]);
                }
                else
                {
                    propPropertyType.intValue = EditorGUI.Popup(
                        new Rect(row2.x, row2.y, col1 - WidthMargin, line),
                        "Variable type",
                        propPropertyType.intValue,
                        ShaderPropertyDefinition.PropertyTypeSelections);
                }

                if (ccScope.changed)
                {
                    switch ((ShaderPropertyType)propPropertyType.intValue)
                    {
                        case ShaderPropertyType.Float:
                        case ShaderPropertyType.Range:
                            propUniformType.intValue = (int)ShaderVariableType.Float;
                            break;
                        case ShaderPropertyType.Int:
                            propUniformType.intValue = (int)ShaderVariableType.Int;
                            break;
                        case ShaderPropertyType.Vector:
                        case ShaderPropertyType.Color:
                            propUniformType.intValue = (int)ShaderVariableType.Float4;
                            break;
                        case ShaderPropertyType.Texture2D:
                            propUniformType.intValue = (int)ShaderVariableType.Texture2D;
                            break;
                        case ShaderPropertyType.Texture3D:
                            propUniformType.intValue = (int)ShaderVariableType.Texture3D;
                            break;
                        case ShaderPropertyType.TextureCube:
                            propUniformType.intValue = (int)ShaderVariableType.TextureCube;
                            break;
                    }
                }
            }

            var availableTypeNames = ShaderPropertyDefinition.GetAvailableVariableTypeNames((ShaderPropertyType)propPropertyType.intValue);
            var availableTypeIndex = EditorGUI.Popup(
                new Rect(row2.x + col1, row2.y, col2 - WidthMargin, line),
                "Variable type",
                Array.IndexOf(availableTypeNames, ShaderPropertyDefinition.VariableTypeSelections[propUniformType.intValue]),
                availableTypeNames);
            propUniformType.intValue = Array.IndexOf(ShaderPropertyDefinition.VariableTypeSelections, availableTypeNames[availableTypeIndex]);

            var rectDefaultValue = new Rect(row2.x + col1 + col2, row2.y, col3, line);
            switch ((ShaderPropertyType)propPropertyType.intValue)
            {
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    EditorGUI.PropertyField(
                        rectDefaultValue,
                        element.FindPropertyRelative(nameof(ShaderPropertyDefinition.defaultFloat)),
                        _defaultValueLabel);
                    break;
                case ShaderPropertyType.Int:
                    EditorGUI.PropertyField(
                        rectDefaultValue,
                        element.FindPropertyRelative(nameof(ShaderPropertyDefinition.defaultInt)));
                    break;
                case ShaderPropertyType.Vector:
                    var propDefaultVector = element.FindPropertyRelative(nameof(ShaderPropertyDefinition.defaultVector));
                    var defaultVector = propDefaultVector.vector4Value;
                    var defaultVectorArray = _defaultVectorArray;

                    defaultVectorArray[0] = defaultVector.x;
                    defaultVectorArray[1] = defaultVector.y;
                    defaultVectorArray[2] = defaultVector.z;
                    defaultVectorArray[3] = defaultVector.w;

                    EditorGUI.MultiFloatField(
                        rectDefaultValue,
                        _defaultVectorLabel,
                        defaultVectorArray);

                    propDefaultVector.vector4Value = new Vector4(defaultVectorArray[0], defaultVectorArray[1], defaultVectorArray[2], defaultVectorArray[3]);
                    break;
                case ShaderPropertyType.Color:
                    EditorGUI.PropertyField(
                        rectDefaultValue,
                        element.FindPropertyRelative(nameof(ShaderPropertyDefinition.defaultColor)));
                    break;
                case ShaderPropertyType.Texture2D:
                case ShaderPropertyType.Texture3D:
                case ShaderPropertyType.TextureCube:
                    var propDefaultTextureIndex = element.FindPropertyRelative(nameof(ShaderPropertyDefinition.defaultTextureIndex));
                    propDefaultTextureIndex.intValue = EditorGUI.Popup(
                        rectDefaultValue,
                        "Default",
                        propDefaultTextureIndex.intValue,
                        ShaderPropertyDefinition.DefaultTextureNames);
                    break;
            }
        }

        /// <summary>
        /// <para>Callback method for <see cref="ReorderableList.onAddCallback"/>.</para>
        /// <para>Add new item to <see cref="_shaderPropDefList"/>.</para>
        /// </summary>
        /// <param name="reorderableList">Source <see cref="ReorderableList"/>. (Unused)</param>
        public void OnAdd(ReorderableList reorderableList)
        {
            var propName = "_CustomProperty";
            for (int i = 1; i < 256; i++)
            {
                var isFound = false;
                foreach (var shaderProp in _shaderPropDefList)
                {
                    if (shaderProp.name == propName)
                    {
                        isFound = true;
                        break;
                    }
                }
                if (!isFound)
                {
                    break;
                }
                propName = "_CustomProperty" + i;
            }
            _shaderPropDefList.Add(new ShaderPropertyDefinition(
                propName,
                "",
                ShaderPropertyType.Float,
                ShaderVariableType.Float));
        }
    }
}
