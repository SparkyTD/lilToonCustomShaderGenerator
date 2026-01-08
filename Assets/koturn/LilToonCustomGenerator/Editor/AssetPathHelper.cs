using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Koturn.LilToonCustomGenerator.Editor.Json;


namespace Koturn.LilToonCustomGenerator.Editor.Windows
{
    public static class AssetPathHelper
    {
        /// <summary>
        /// パスの区切り文字を正規化します。
        /// </summary>
        /// <remarks>
        /// - バックスラッシュ(\)をスラッシュ(/)に変換
        /// - 連続する区切り文字(// や \\\ など)を単一のスラッシュ(/)に変換
        /// </remarks>
        /// <param name="path">正規化するパス文字列</param>
        /// <returns>
        /// 正規化されたパス文字列。
        /// 入力がnullまたは空文字の場合は、入力値をそのまま返します。
        /// </returns>
        /// <example>
        /// 以下のパスはすべて同じ文字列に正規化されます:
        /// <code>
        /// "Assets\\Path"  -> "Assets/Path"
        /// "Assets/Path"   -> "Assets/Path"
        /// "Assets//Path" -> "Assets/Path"
        /// </code>
        /// </example>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            // すべての区切り文字を'/'に変換
            path = path.Replace('\\', '/');

            // 連続する'/'を単一の'/'に置換（複数回の置換で///なども対応）
            while (path.Contains("//"))
            {
                path = path.Replace("//", "/");
            }

            return path;
        }

        /// <summary>
        /// Windowsのフルパスを Unity の Asset パスに変換します
        /// </summary>
        /// <param name="absPath">
        /// 変換元のパス
        /// - Windowsフルパス (例: "C:/Projects/MyGame/Assets/Textures/image.png")
        /// - アセットパス (例: "Assets/Textures/image.png")
        /// </param>
        /// <returns>Unityのアセットパス (例: "Assets/Textures/image.png")</returns>
        public static string AbsPathToAssetPath(string absPath)
        {
            if (string.IsNullOrEmpty(absPath)) return null;

            // パスを正規化
            absPath = NormalizePath(absPath);

            // すでにAssetsから始まる場合は、正規化したパスをそのまま返す
            if (absPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                return absPath;
            }

            // Application.dataPath は "プロジェクトパス/Assets" を返す
            string projectPath = NormalizePath(Application.dataPath);
            // "Assets" フォルダまでのパスを取得
            string assetsBasePath = projectPath.Substring(0, projectPath.Length - "Assets".Length);

            // フルパスから相対パスに変換
            if (absPath.StartsWith(assetsBasePath, StringComparison.OrdinalIgnoreCase))
            {
                return absPath.Substring(assetsBasePath.Length);
            }

            Debug.LogError("Invalid path: The specified path is not within the Unity project.");
            return null;
        }

        /// <summary>
        /// Unity の Asset パスを Windows のフルパスに変換します
        /// </summary>
        /// <param name="assetPath">
        /// 変換元のパス
        /// - アセットパス (例: "Assets/Textures/image.png")
        /// - Windowsフルパス (例: "C:/Projects/MyGame/Assets/Textures/image.png")
        /// </param>
        /// <returns>Windowsのフルパス (例: "C:/Projects/MyGame/Assets/Textures/image.png")</returns>
        public static string AssetPathToAbsPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return null;

            // 入力パスを正規化
            assetPath = NormalizePath(assetPath);

            // すでにフルパスの場合は、正規化したパスをそのまま返す
            string projectPath = NormalizePath(Application.dataPath);
            string projectRoot = projectPath.Substring(0, projectPath.Length - "Assets".Length);
            if (assetPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                return assetPath;
            }

            // アセットパスが "Assets/" で始まっていない場合はエラー
            if (!assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("Invalid asset path: The path must start with 'Assets/'");
                return null;
            }

            // プロジェクトルートパスとアセットパスを結合して正規化
            return NormalizePath(Path.Combine(projectRoot, assetPath));
        }

    }
}
