using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Koturn.LilToonCustomGenerator.Editor.Enums;


namespace Koturn.LilToonCustomGenerator.Editor
{
    [Serializable]
    public class V2FMember
    {
        /// <summary>
        /// Variable types in HLSL.
        /// </summary>
        public static string[] VariableTypeSelections { get; } =
        {
            "float",
            "float2",
            "float3",
            "float4",
            "half",
            "half2",
            "half3",
            "half4",
            "fixed",
            "fixed2",
            "fixed3",
            "fixed4",
            "bool",
            "lilBool",
            "int",
            "int2",
            "int3",
            "int4",
            "uint",
            "uint2",
            "uint3",
            "uint4"
        };

        /// <summary>
        /// Member name.
        /// </summary>
        public string name;
        /// <summary>
        /// Member type.
        /// </summary>
        public ShaderVariableType variableType;

        public string VariableTypeText => VariableTypeSelections[(int)variableType];

        public V2FMember(string name, ShaderVariableType variableType)
        {
            this.name = name;
            this.variableType = variableType;
        }
    }
}
