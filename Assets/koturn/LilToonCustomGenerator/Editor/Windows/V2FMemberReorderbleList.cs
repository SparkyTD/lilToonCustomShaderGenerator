using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Koturn.LilToonCustomGenerator.Editor.Enums;


namespace Koturn.LilToonCustomGenerator.Editor.Windows
{
    internal class V2FMemberReorderbleList : ReorderableList
    {
        /// <summary>
        /// Width margin.
        /// </summary>
        private const float WidthMargin = 2.0f;

        /// <summary>
        /// Shader property definition list.
        /// </summary>
        private readonly List<V2FMember> _memberList;

        public V2FMemberReorderbleList(SerializedObject serializedObject, SerializedProperty serializedProperty, List<V2FMember> memberList)
            : base(serializedObject, serializedProperty, true, true, true, true)
        {
            _memberList = memberList;
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
            EditorGUI.LabelField(rect, "v2f members");
        }

        /// <summary>
        /// <para>Callback method for <see cref="ReorderableList.elementHeight"/>.</para>
        /// <para>Returns height of the element of the specified index.</para>
        /// </summary>
        /// <param name="index">Element index. (unused)</param>
        /// <returns>Height of the element of the specified index.</returns>
        private float GetElementHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight + 4.0f;
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
            var nameWidth = row1.width * 0.5f;
            var descWidth = row1.width * 0.5f;

            EditorGUI.PropertyField(
                new Rect(row1.x, row1.y, nameWidth - WidthMargin, line),
                element.FindPropertyRelative(nameof(V2FMember.name)),
                new GUIContent("Member Name"));

            var propVariableType = element.FindPropertyRelative(nameof(V2FMember.variableType));
            propVariableType.intValue = EditorGUI.Popup(
                new Rect(row1.x + nameWidth, row1.y, descWidth, line),
                "Variable type",
                propVariableType.intValue,
                V2FMember.VariableTypeSelections);
        }

        /// <summary>
        /// <para>Callback method for <see cref="ReorderableList.onAddCallback"/>.</para>
        /// <para>Add new item to <see cref="_shaderPropDefList"/>.</para>
        /// </summary>
        /// <param name="reorderableList">Source <see cref="ReorderableList"/>. (Unused)</param>
        public void OnAdd(ReorderableList reorderableList)
        {
            var memberName = "member";
            for (int i = 1; i < 256; i++)
            {
                var isFound = false;
                foreach (var member in _memberList)
                {
                    if (member.name == memberName)
                    {
                        isFound = true;
                        break;
                    }
                }
                if (!isFound)
                {
                    break;
                }
                memberName = "member" + i;
            }
            _memberList.Add(new V2FMember(memberName, ShaderVariableType.Float));
        }
    }
}
