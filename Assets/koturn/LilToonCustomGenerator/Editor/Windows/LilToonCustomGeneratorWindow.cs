using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Koturn.LilToonCustomGenerator.Editor.Enums;
using Koturn.LilToonCustomGenerator.Editor.Json;
using Koturn.LilToonCustomGenerator.Editor.Internals;
using System.Linq;


namespace Koturn.LilToonCustomGenerator.Editor.Windows
{
    public class LilToonCustomGeneratorWindow : EditorWindow
    {
        private static readonly string[] _newLineSelections =
        {
            "LF",
            "CR",
            "CR + LF"
        };
        /// <summary>
        /// <see cref="SerializedObject"/> of this instance.
        /// </summary>
        private SerializedObject _serializedObject;
        /// <summary>
        /// <see cref="ReorderableList"/> for <<see cref="_shaderPropDefList"/>.
        /// </summary>
        private PropertyReorderableList _propertyReorderableList;
        /// <summary>
        /// <see cref="ReorderableList"/> for <<see cref="_v2fMemberList"/>.
        /// </summary>
        private V2FMemberReorderbleList _v2fMemberReorderableList;
        /// <summary>
        /// Shader property definition list.
        /// </summary>
        [SerializeField]
        private List<ShaderPropertyDefinition> _shaderPropDefList = new List<ShaderPropertyDefinition>();
        /// <summary>
        /// Shader property definition list.
        /// </summary>
        [SerializeField]
        private List<V2FMember> _v2fMemberList = new List<V2FMember>();
        /// <summary>
        /// Current scroll position.
        /// </summary>
        private Vector2 _scrollPosition;
        /// <summary>
        /// Json root instance.
        /// </summary>
        private JsonRoot _jsonRoot;
        /// <summary>
        /// Template name array.
        /// </summary>
        private string[] _templateNames;
        /// <summary>
        /// Template index.
        /// </summary>
        private int _templateIndex;
        /// <summary>
        /// Custom shader name.
        /// </summary>
        private string _shaderName = "MyCustomShader";
        /// <summary>
        /// Custom shader title displaying on the inspector.
        /// </summary>
        private string _shaderTitle = "My Custom Shader";
        /// <summary>
        /// Namespace and assembly name.
        /// </summary>
        private string _namespace = "LilToonCustom.Editor";
        /// <summary>
        /// Inspector class name.
        /// </summary>
        private string _inspectorName = "CustomInspector";
        /// <summary>
        /// New line type.
        /// </summary>
        private NewLineType _newLineType;
        private bool _shouldGenerateVersionDetectionHeader = false;
        /// <summary>
        /// True to generate <c>Editor/lang_custom.tsv</c>.
        /// </summary>
        private bool _shouldGenerateLangTsv = true;
        /// <summary>
        /// True to emit shader conversion menu.
        /// </summary>
        private bool _shouldGenerateConvertMenu = true;
        /// <summary>
        /// True to emit cache clear menu.
        /// </summary>
        private bool _shouldGenerateCacheClearMenu = false;
        /// <summary>
        /// True to generate <c>Editor/AssemblyInfo.cs</c>.
        /// </summary>
        private bool _shouldGenerateAssemblyInfo = true;
        /// <summary>
        /// True to emit geometry shader template to <c>Shaders/custom_insert_post.hlsl</c>.
        /// </summary>
        private bool _shouldEmitGeometryShader = false;
        /// <summary>
        /// True to override geometry shader of fur shaders.
        /// </summary>
        private bool _shouldOverrideFurGeometry = false;
        /// <summary>
        /// True to override geometry shader of one pass outline shaders.
        /// </summary>
        private bool _shouldOverrideOnePassOutlineGeometry = false;
        /// <summary>
        /// True to generate InsertPost tag and <c>Shaders/custom_insert_post.hlsl</c>.
        /// </summary>
        private bool _shouldGenerateInsertPost = false;
        /// <summary>
        /// <para>True to declare following variables.</para>
        /// <para>
        /// <list>
        ///   <item>
        ///     <term><c>_VRChatCameraMode</c></term>
        ///     <description>0: Rendering normally, 1: Rendering in VR handheld camera, 2: Rendering in Desktop handheld camera, 3: Rendering for a screenshot</description>
        ///   </item>
        ///   <item>
        ///     <term><c>_VRChatCameraMask</c></term>
        ///     <description>The cullingMask property of the active camera, available if _VRChatCameraMode != 0.</description>
        ///   </item>
        ///   <item>
        ///     <term><c>_VRChatMirrorMode</c></term>
        ///     <description>0: Rendering normally, not in a mirror, 1: Rendering in a mirror viewed in VR, 2: Rendering in a mirror viewed in desktop mode</description>
        ///   </item>
        ///   <item>
        ///     <term><c>_VRChatFaceMirrorMode</c></term>
        ///     <description>1 when rendering the face mirror (VR and Desktop use different camera types!), 0 otherwise.</description>
        ///   </item>
        ///   <item>
        ///     <term><c>_VRChatMirrorCameraPos</c></term>
        ///     <description>World space position of mirror camera (eye independent, "centered" in VR). (0,0,0) when not rendering in a mirror.</description>
        ///   </item>
        ///   <item>
        ///     <term><c>_VRChatScreenCameraPos</c></term>
        ///     <description>World space position of main screen camera.</description>
        ///   </item>
        ///   <item>
        ///     <term><c>_VRChatScreenCameraRot</c></term>
        ///     <description>World space rotation (quaternion) of main screen camera.</description>
        ///   </item>
        ///   <item>
        ///     <term><c>_VRChatPhotoCameraPos</c></term>
        ///     <description>World space position of handheld photo camera (first instance when using Dolly Multicam), (0,0,0) when camera is not active.</description>
        ///   </item>
        ///   <item>
        ///     <term><c>_VRChatPhotoCameraRot</c></term>
        ///     <description>World space rotation (quaternion) of photo camera.</description>
        ///   </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <remarks>
        /// <seealso href="https://creators.vrchat.com/worlds/udon/vrc-graphics/vrchat-shader-globals/"/>
        /// </remarks>
        private bool _shouldDeclareVRChatVariables = false;
        /// <summary>
        /// <para>True to declare following two variables in <c>Shaders/custom_insert.hlsl</c>.</para>
        /// <para>
        /// <list>
        ///   <item><c>_AudioTexture</c></item>
        ///   <item><c>_AudioTexture_TexelSize</c></item>
        /// </list>
        /// </para>
        /// </summary>
        private bool _shouldDeclareAudioLinkVariables = false;
        /// <summary>
        /// <para>True to declare following three variables in <c>Shaders/custom_insert.hlsl</c>.</para>
        /// <para>
        /// <list>
        ///   <item><c>_Udon_VideoTex</c></item>
        ///   <item><c>_Udon_VideoTex_TexelSize</c></item>
        ///   <item><c>_Udon_VideoTex_ST</c></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <remarks>
        /// <seealso href="https://protv.dev/avatars"/>
        /// </remarks>
        private bool _shouldDeclareProTVVariables = false;
        /// <summary>
        /// Text for <see cref="System.Reflection.AssemblyTitleAttribute"/>.
        /// </summary>
        private string _assemblyTitle;
        /// <summary>
        /// Text for <see cref="System.Reflection.AssemblyDescriptionAttribute"/>.
        /// </summary>
        private string _assemblyDescription;
        /// <summary>
        /// Text for <see cref="System.Reflection.AssemblyCompanyAttribute"/>.
        /// </summary>
        private string _assemblyCompany;
        /// <summary>
        /// Text for <see cref="System.Reflection.AssemblyProductAttribute"/>.
        /// </summary>
        private string _assemblyProduct;
        /// <summary>
        /// Text for <see cref="System.Reflection.AssemblyCopyrightAttribute"/>.
        /// </summary>
        private string _assemblyCopyright;
        /// <summary>
        /// Text for <see cref="System.Reflection.AssemblyTrademarkAttribute"/>.
        /// </summary>
        private string _assemblyTrademark;
        /// <summary>
        /// Text for <see cref="System.Reflection.AssemblyCultureAttribute"/>.
        /// </summary>
        private string _assemblyCulture;
        /// <summary>
        /// Text for <see cref="System.Reflection.AssemblyVersionAttribute"/>.
        /// </summary>
        private string _assemblyVersion;


        /// <summary>
        /// Last export directory.
        /// </summary>
        private string _lastExportDirectoryPath;


        /// <summary>
        /// <para>Called when this window is created.</para>
        /// <para>Initialize <see cref="_serializedObject"/> and <see cref="_propertyReorderableList"/>.</para>
        /// </summary>
        private void OnEnable()
        {
            _jsonRoot = DeserializeJson(AssetDatabase.GUIDToAssetPath("407d2dc27f05f774d9ca8d53fdef2047"));
            _templateNames = _jsonRoot.configList.Select(config => config.name).ToArray();

            _serializedObject = new SerializedObject(this);
            _v2fMemberReorderableList = new V2FMemberReorderbleList(_serializedObject, _serializedObject.FindProperty(nameof(_v2fMemberList)), _v2fMemberList);
            _propertyReorderableList = new PropertyReorderableList(_serializedObject, _serializedObject.FindProperty(nameof(_shaderPropDefList)), _shaderPropDefList);

            var userName = Environment.UserName;
            var m = RegexProvider.IdentifierRegex.Match(userName);
            if (m.Success)
            {
                var g = m.Groups;
                _namespace = g[1].Value.ToUpperInvariant() + g[2].Value + ".LilToonCustom.Editor";
                _shaderName = g[0].Value + "/MyCustomShader";
            }

            _assemblyTitle = _namespace;
            _assemblyDescription = string.Format("Material inspector of {0}.", _shaderName);
            _assemblyCompany = userName;
            _assemblyProduct = _assemblyTitle;
            _assemblyCopyright = string.Format("Copyright (C) {0} {1} All Rights Reserverd.", DateTime.Now.Year, userName);
            _assemblyTrademark = "";
            _assemblyCulture = "";
            _assemblyVersion = "1.0.0.0";
        }


        /// <summary>
        /// Draw GUI components.
        /// </summary>
        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Basic configuration", EditorStyles.boldLabel);
                _templateIndex = EditorGUILayout.Popup("Template", _templateIndex, _templateNames);
                _shaderName = EditorGUILayout.TextField("Shader name", _shaderName);
                _shaderTitle = EditorGUILayout.TextField("Shader title", _shaderTitle);
                _namespace = EditorGUILayout.TextField("Inspector Namespace", _namespace);
                _inspectorName = EditorGUILayout.TextField("Inspector class name", _inspectorName);
                _newLineType = (NewLineType)EditorGUILayout.Popup("New Line Code", (int)_newLineType, _newLineSelections);
            }

            using (var svScope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = svScope.scrollPosition;

                _serializedObject.Update();
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    _propertyReorderableList.DoLayoutList();
                }
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    _v2fMemberReorderableList.DoLayoutList();
                }
                _serializedObject.ApplyModifiedProperties();

                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Shader options", EditorStyles.boldLabel);
                    _shouldEmitGeometryShader = EditorGUILayout.ToggleLeft("Edit geometry shader", _shouldEmitGeometryShader);
                    if (_shouldEmitGeometryShader)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                        {
                            _shouldOverrideFurGeometry = EditorGUILayout.ToggleLeft("Override fur geometry shaders", _shouldOverrideFurGeometry);
                            _shouldOverrideOnePassOutlineGeometry = EditorGUILayout.ToggleLeft("Override one pass outline geometry shaders (HDRP only)", _shouldOverrideOnePassOutlineGeometry);
                        }
                        using (new EditorGUI.DisabledScope(_shouldEmitGeometryShader))
                        {
                            EditorGUILayout.ToggleLeft("Emit lilSubShaderInsertPost and generate lilCustomShaderInsertPost.lilblock and custom_insert_post.hlsl", _shouldEmitGeometryShader);
                        }
                    }
                    else
                    {
                        _shouldGenerateInsertPost = EditorGUILayout.ToggleLeft("Emit lilSubShaderInsertPost and generate lilCustomShaderInsertPost.lilblock and custom_insert_post.hlsl", _shouldGenerateInsertPost);
                    }
                    _shouldDeclareVRChatVariables = EditorGUILayout.ToggleLeft("Use VRChat variables", _shouldDeclareVRChatVariables);
                    _shouldDeclareAudioLinkVariables = EditorGUILayout.ToggleLeft("Use AudioLink variables", _shouldDeclareAudioLinkVariables);
                    _shouldDeclareProTVVariables = EditorGUILayout.ToggleLeft("Use ProTV variables", _shouldDeclareProTVVariables);
                }

                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Inspector options", EditorStyles.boldLabel);
                    _shouldGenerateVersionDetectionHeader = EditorGUILayout.ToggleLeft("Generate Version Detection Header", _shouldGenerateVersionDetectionHeader);
                    _shouldGenerateLangTsv = EditorGUILayout.ToggleLeft("Generate Language File", _shouldGenerateLangTsv);
                    _shouldGenerateConvertMenu = EditorGUILayout.ToggleLeft("Generate Convert Menu", _shouldGenerateConvertMenu);
                    _shouldGenerateCacheClearMenu = EditorGUILayout.ToggleLeft("Generate Cache Clear Menu", _shouldGenerateCacheClearMenu);
                    _shouldGenerateAssemblyInfo = EditorGUILayout.ToggleLeft("Generate AssemblyInfo.cs", _shouldGenerateAssemblyInfo);
                    if (_shouldGenerateAssemblyInfo)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                        {
                            _assemblyTitle = EditorGUILayout.TextField("AssemblyTitle", _assemblyTitle);
                            _assemblyDescription = EditorGUILayout.TextField("AssemblyDescription", _assemblyDescription);
                            _assemblyCompany = EditorGUILayout.TextField("AssemblyCompany", _assemblyCompany);
                            _assemblyProduct = EditorGUILayout.TextField("AssemblyProduct", _assemblyProduct);
                            _assemblyCopyright = EditorGUILayout.TextField("AssemblyCopyright", _assemblyCopyright);
                            _assemblyTrademark = EditorGUILayout.TextField("AssemblyTrademark", _assemblyTrademark);
                            _assemblyCulture = EditorGUILayout.TextField("AssemblyCulture", _assemblyCulture);
                            _assemblyVersion = EditorGUILayout.TextField("AssemblyVersion", _assemblyVersion);
                        }
                    }
                }
            }

            if (GUILayout.Button("Generate Custom Shader"))
            {
                var exportDirPath = EditorUtility.SaveFolderPanel(
                    "Select export directory",
                    Directory.Exists(_lastExportDirectoryPath) ? _lastExportDirectoryPath : Application.dataPath,
                    string.Empty);
                if (string.IsNullOrEmpty(exportDirPath))
                {
                    return;
                }

                _lastExportDirectoryPath = exportDirPath;

                Debug.LogFormat("Export dir: {0}", exportDirPath);

                var assetPath = AssetPathHelper.AbsPathToAssetPath(exportDirPath);
                Generate(assetPath == null ? exportDirPath : assetPath);
            }
        }

        private void Generate(string dstDirAssetPath)
        {
            var templateEngine = CreateTemplateEngine();

            var isInProject = dstDirAssetPath.StartsWith("Assets");
            if (isInProject)
            {
                templateEngine.AddTag("BASE_DIRECTORY", dstDirAssetPath + "/");
            }

            // Generate `Shaders` directory and obtain its GUID.
            var shaderDirAssetPath = dstDirAssetPath + "/" + "Shaders";
            Directory.CreateDirectory(shaderDirAssetPath);

            string guidShaderDir;
            if (isInProject)
            {
                AssetDatabase.ImportAsset(shaderDirAssetPath);
                guidShaderDir = AssetDatabase.AssetPathToGUID(shaderDirAssetPath);
            }
            else
            {
                guidShaderDir = CreateMetaFile(shaderDirAssetPath + ".meta").ToString("N");
            }
            if (guidShaderDir.Length != 0)
            {
                templateEngine.AddTag("GUID_SHADER_DIR", guidShaderDir);
            }

            var config = _jsonRoot.configList[_templateIndex];
            Debug.LogFormat("Generate files from {0}", config.name);

            // Try to find `Editor/lang_custom.tsv`.
            var langCustomIndex = IndexOfDestination(config.templates, "Editor/lang_custom.tsv");

            // Generate `Editor/lang_custom.tsv` and obtain its GUID.
            if (langCustomIndex != -1)
            {
                if (_shouldGenerateLangTsv)
                {
                    var tfcLangCustom = config.templates[langCustomIndex];
                    config.templates.RemoveAt(langCustomIndex);

                    var dstFilePath = dstDirAssetPath + "/" + templateEngine.Replace(tfcLangCustom.destination);
                    var path = AssetDatabase.GUIDToAssetPath(tfcLangCustom.guid);

                    Directory.CreateDirectory(Path.GetDirectoryName(dstFilePath));

                    Debug.LogFormat("  {0} -> {1}", path, dstFilePath);

                    templateEngine.ExpandTemplate(path, dstFilePath);

                    string guidlangCustom;
                    if (isInProject)
                    {
                        AssetDatabase.ImportAsset(dstFilePath);
                        guidlangCustom = AssetDatabase.AssetPathToGUID(dstFilePath);
                    }
                    else
                    {
                        guidlangCustom = CreateMetaFile(dstFilePath + ".meta").ToString("N");
                    }
                    Debug.LogFormat("{0}: {1}", path, dstFilePath, guidlangCustom);
                    if (guidlangCustom.Length != 0)
                    {
                        templateEngine.AddTag("GUID_LANG_CUSTOM", guidlangCustom);
                    }
                }
                else
                {
                    config.templates.RemoveAt(langCustomIndex);
                }
            }

            if (!_shouldGenerateInsertPost)
            {
                var index = IndexOfDestination(config.templates, "Shaders/custom_insert_port.hlsl");
                if (index != -1)
                {
                    config.templates.RemoveAt(index);
                }

                index = IndexOfDestination(config.templates, "Shaders/lilCustomShaderInsertPort.lilblock");
                if (index != -1)
                {
                    config.templates.RemoveAt(index);
                }
            }

            if (!_shouldGenerateVersionDetectionHeader)
            {
                var index = IndexOfDestination(config.templates, "Editor/Startup.cs");
                if (index != -1)
                {
                    config.templates.RemoveAt(index);
                }
            }

            if (!_shouldGenerateAssemblyInfo)
            {
                var index = IndexOfDestination(config.templates, "Editor/AssemblyInfo.cs");
                if (index != -1)
                {
                    config.templates.RemoveAt(index);
                }
            }

            foreach (var tfc in config.templates)
            {
                var dstFilePath = dstDirAssetPath + "/" + templateEngine.Replace(tfc.destination);
                var path = AssetDatabase.GUIDToAssetPath(tfc.guid);

                Debug.LogFormat("  {0} -> {1}", path, dstFilePath);

                Directory.CreateDirectory(Path.GetDirectoryName(dstFilePath));

                templateEngine.ExpandTemplate(path, dstFilePath);
            }

            if (isInProject)
            {
                // Import created files.
                AssetDatabase.Refresh();
            }
        }

        private static int IndexOfDestination(List<TemplateFileConfig> tfcList, string destination)
        {
            int index = 0;
            foreach (var tfc in tfcList)
            {
                if (tfc.destination == destination)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        private static Guid CreateMetaFile(string metaFilePath)
        {
            var guid = Guid.NewGuid();
            CreateMetaFile(metaFilePath, guid);
            return guid;
        }

        private static void CreateMetaFile(string metaFilePath, Guid guid)
        {
            using (var targetStream = new FileStream(metaFilePath, FileMode.Create, FileAccess.Write, FileShare.Read, 256, FileOptions.SequentialScan))
            using (var writer = new StreamWriter(targetStream, Encoding.ASCII, 256)
            {
                NewLine = "\n"
            })
            {
                writer.WriteLine("fileFormatVersion: 2");
                writer.WriteLine("guid: {0:N}", guid);
                if (Directory.Exists(Path.Combine(Path.GetDirectoryName(metaFilePath), Path.GetFileNameWithoutExtension(metaFilePath))))
                {
                    writer.WriteLine("folderAsset: yes");
                }
                writer.WriteLine("DefaultImporter:");
                writer.WriteLine("  externalObjects: {}");
                writer.WriteLine("  userData: ");
                writer.WriteLine("  assetBundleName: ");
                writer.WriteLine("  assetBundleVariant: ");
            }
        }

        private static JsonRoot DeserializeJson(string filePath)
        {
            var jsonRoot = JsonRoot.LoadFromJsonFile(filePath);
            var nameConfigDict = new Dictionary<string, TemplateConfig>();

            // Create list to resolve their parents.
            var inheritList = new List<TemplateConfig>();
            foreach (var config in jsonRoot.configList)
            {
                nameConfigDict.Add(config.name, config);
                if (config.basedOn != null)
                {
                    inheritList.Add(config);
                }
            }

            // Resolve "basedOn".
            var visitSet = new HashSet<string>();
            var dstSet = new HashSet<string>();
            foreach (var config in inheritList)
            {
                dstSet.Clear();
                foreach (var tfc in config.templates)
                {
                    dstSet.Add(tfc.destination);
                }

                visitSet.Clear();
                visitSet.Add(config.name);

                var parentConfig = config;
                while (parentConfig.basedOn != null)
                {
                    if (visitSet.Contains(parentConfig.basedOn))
                    {
                        throw new InvalidOperationException("Circular definition detected: " + config.name);
                    }

                    parentConfig = nameConfigDict[parentConfig.basedOn];
                    foreach (var tfc in parentConfig.templates)
                    {
                        if (!dstSet.Contains(tfc.destination))
                        {
                            Console.WriteLine("Add {0}", tfc.destination);
                            config.templates.Add(tfc);
                            dstSet.Add(tfc.destination);
                        }
                    }
                }
            }

            return jsonRoot;
        }

        private TemplateEngine CreateTemplateEngine()
        {
            var shaderPropDefList = _shaderPropDefList;
            var materialPropNames = new string[shaderPropDefList.Count];
            var langTags = new string[shaderPropDefList.Count];

            var index = 0;
            foreach (var shaderProp in _shaderPropDefList)
            {
                materialPropNames[index] = RegexProvider.PropertyNameRegex.Replace(shaderProp.name, m => "_" + m.Groups[1].Value.ToLower() + m.Groups[2].Value);
                langTags[index] = RegexProvider.PropertyNameRegex.Replace(shaderProp.name, m => "s" + m.Groups[1].Value + m.Groups[2].Value);
                index++;
            }

            var templateEngine = new TemplateEngine()
            {
                NewLine = _newLineType == NewLineType.CrLf ? "\n\r" : _newLineType == NewLineType.Cr ? "\r" : "\n"
            };
            templateEngine.AddTag("SHADER_NAME", _shaderName);
            templateEngine.AddTag("NAMESPACE", _namespace);
            templateEngine.AddTag("SHADER_TITLE", _shaderTitle);
            templateEngine.AddTag("INSPECTOR_NAME", _inspectorName);
            // templateEngine.AddTag("AUTHOR_NAME", Environment.UserName);
            // templateEngine.AddTag("YEAR", DateTime.Now.Year.ToString());

            templateEngine.AddTag("ASSEMBLY_TITLE", _assemblyTitle);
            templateEngine.AddTag("ASSEMBLY_DESCRIPTION", _assemblyDescription);
            templateEngine.AddTag("ASSEMBLY_COMPANY", _assemblyCompany);
            templateEngine.AddTag("ASSEMBLY_PRODUCT", _assemblyProduct);
            templateEngine.AddTag("ASSEMBLY_COPYRIGHT", _assemblyCopyright);
            templateEngine.AddTag("ASSEMBLY_TRADEMARK", _assemblyTrademark);
            templateEngine.AddTag("ASSEMBLY_CULTURE", _assemblyCulture);
            templateEngine.AddTag("ASSEMBLY_VERSION", _assemblyVersion);

            var sb = new StringBuilder();

            index = 0;
            foreach (var shaderProp in _shaderPropDefList)
            {
                sb.AppendLine("/// <summary>")
                    .AppendFormat("/// <see cref=\"MaterialProperty\" of \"{0}\".", shaderProp.name).AppendLine()
                    .AppendLine("/// </summary>")
                    .AppendFormat("private MaterialProperty {0};", materialPropNames[index])
                    .AppendLine();
                index++;
            }
            templateEngine.AddTag("DECLARE_MATERIAL_PROPERTIES", sb.ToString());

            sb.Clear();
            index = 0;
            foreach (var shaderProp in _shaderPropDefList)
            {
                sb.AppendFormat("{0} = FindProperty(\"{1}\", props);", materialPropNames[index], shaderProp.name)
                    .AppendLine();
                index++;
            }
            templateEngine.AddTag("INITIALIZE_MATERIAL_PROPERTIES", sb.ToString());

            sb.Clear();
            index = 0;
            foreach (var shaderProp in _shaderPropDefList)
            {
                sb.AppendFormat("propertyList.Add({0});", materialPropNames[index])
                    .AppendLine();
                index++;
            }
            templateEngine.AddTag("INITIALIZE_MATERIAL_PROPERTY_LIST", sb.ToString());

            if (_shouldGenerateLangTsv)
            {
                sb.Clear();
                index = 0;
                foreach (var shaderProp in _shaderPropDefList)
                {
                    sb.AppendFormat(
                        "m_MaterialEditor.ShaderProperty({0}, GetLoc(\"{1}\"));",
                        materialPropNames[index],
                        langTags[index]).AppendLine();
                    index++;
                }
                templateEngine.AddTag("DRAW_MATERIAL_PROPERTIES", sb.ToString());

                sb.Clear();
                index = 0;
                foreach (var shaderProp in _shaderPropDefList)
                {
                    sb.AppendFormat("{0}\t{1}\t{1}\t{1}\t{1}\t{1}", langTags[index], shaderProp.description.Length == 0 ? langTags[index] : shaderProp.description)
                        .AppendLine();
                    index++;
                }
                templateEngine.AddTag("LANGUAGE_FILE_CONTENT", sb.ToString());
            }
            else
            {
                sb.Clear();
                index = 0;
                foreach (var shaderProp in _shaderPropDefList)
                {
                    sb.AppendFormat(
                        "m_MaterialEditor.ShaderProperty({0}, {0}.displayName);",
                        materialPropNames[index]).AppendLine();
                    index++;
                }
                templateEngine.AddTag("DRAW_MATERIAL_PROPERTIES", sb.ToString());
            }

            sb.Clear();
            index = 0;
            foreach (var shaderProp in _shaderPropDefList)
            {
                if (shaderProp.IsTexture)
                {
                    sb.AppendFormat(
                        "lilEditorGUI.LocalizedPropertyTexture(m_MaterialEditor, new GUIContent(GetLoc({0}.displayName), GetLoc(\"sTextureRGBA\")), {0});",
                        materialPropNames[index]);
                }
                else
                {
                    sb.AppendFormat(
                        "lilEditorGUI.LocalizedProperty(m_MaterialEditor, {0});",
                        materialPropNames[index]);
                }
                sb.AppendLine();
                index++;
            }
            templateEngine.AddTag("DRAW_LOCALIZED_MATERIAL_PROPERTIES", sb.ToString());

            sb.Clear();
            if (_shouldGenerateLangTsv)
            {
                index = 0;
                foreach (var shaderProp in _shaderPropDefList)
                {
                    sb.AppendFormat(
                        "{0} (\"{1}\", {2}) = {3}",
                        shaderProp.name,
                        langTags[index],
                        shaderProp.PropertyTypeText,
                        shaderProp.DefaultValueString).AppendLine();
                    index++;
                }
            }
            else
            {
                foreach (var shaderProp in _shaderPropDefList)
                {
                    sb.AppendFormat(
                        "{0} (\"{1}\", {2}) = {3}",
                        shaderProp.name,
                        shaderProp.description,
                        shaderProp.PropertyTypeText,
                        shaderProp.DefaultValueString).AppendLine();
                }
            }
            templateEngine.AddTag("DECLARE_CUSTOM_PROPERTIES", sb.ToString());

            sb.Clear();
            foreach (var shaderProp in _shaderPropDefList)
            {
                if (shaderProp.propertyType != ShaderPropertyType.Float
                    && shaderProp.propertyType != ShaderPropertyType.Range
                    && shaderProp.propertyType != ShaderPropertyType.Int
                    && shaderProp.propertyType != ShaderPropertyType.Vector
                    && shaderProp.propertyType != ShaderPropertyType.Color)
                {
                    continue;
                }
                if (sb.Length > 0)
                {
                    sb.Append(@" \").AppendLine();
                }
                sb.AppendFormat("{0} {1};", ShaderPropertyDefinition.VariableTypeSelections[(int)shaderProp.uniformType], shaderProp.name);
            }
            if (sb.Length > 0)
            {
                sb.AppendLine();
                templateEngine.AddTag("DECLARE_UNIFORM_VARIABLES", sb.ToString());
            }

            sb.Clear();
            foreach (var shaderProp in _shaderPropDefList)
            {
                var textureDeclarationMacro = shaderProp.TextureDeclarationMacro;
                if (textureDeclarationMacro == null)
                {
                    continue;
                }
                if (sb.Length > 0)
                {
                    sb.Append(@" \").AppendLine();
                }
                sb.AppendFormat("{0}({1});", textureDeclarationMacro, shaderProp.name);
            }
            if (sb.Length > 0)
            {
                sb.AppendLine();
                templateEngine.AddTag("DECLARE_TEXTURE_VARIABLES", sb.ToString());
            }

            sb.Clear();
            index = 0;
            foreach (var v2fMember in _v2fMemberList)
            {
                if (sb.Length > 0)
                {
                    sb.Append(@" \").AppendLine();
                }
                sb.AppendFormat("{0} {1} : TEXCOORD ## id{2};", v2fMember.VariableTypeText, v2fMember.name, index);
                index++;
            }
            if (sb.Length > 0)
            {
                sb.AppendLine();
                templateEngine.AddTag("V2F_MEMBERS", sb.ToString());
            }

            sb.Clear();
            index = 1;
            foreach (var v2fMember in _v2fMemberList)
            {
                if (sb.Length > 0)
                {
                    sb.Append(@" \").AppendLine();
                }
                sb.AppendFormat("{0} {1} : TEXCOORD ## id{2};", v2fMember.variableType, v2fMember.name, index);
                index++;
            }
            if (sb.Length > 0)
            {
                sb.AppendLine();
                templateEngine.AddTag("V2F_MEMBERS_VER140_SHADOWCASTER", sb.ToString());
            }

            if (!_namespace.StartsWith("lilToon."))
            {
                templateEngine.AddTag("SHOULD_EMIT_USING_LILTOON", "true");
            }
            if (_shouldEmitGeometryShader)
            {
                templateEngine.AddTag("SHOULD_EMIT_GEOMETRY_SHADER", "true");
            }
            if (_shouldOverrideFurGeometry)
            {
                templateEngine.AddTag("OVERRIDE_FUR_GEOMETRY", "true");
            }
            if (_shouldOverrideOnePassOutlineGeometry)
            {
                templateEngine.AddTag("OVERRIDE_ONEPASS_GEOMETRY", "true");
            }
            if (_shouldEmitGeometryShader || _shouldGenerateInsertPost)
            {
                templateEngine.AddTag("SHOULD_GENERATE_INSERT_POST", "true");
            }
            if (_shouldGenerateConvertMenu)
            {
                templateEngine.AddTag("SHOULD_GENERATE_CONVERT_MENU", "true");
            }
            if (_shouldGenerateCacheClearMenu)
            {
                templateEngine.AddTag("SHOULD_GENERATE_REFRESH_MENU", "true");
            }
            if (_shouldGenerateVersionDetectionHeader)
            {
                templateEngine.AddTag("SHOULD_GENERATE_VERSION_DEF_FILE", "true");
            }
            if (_shouldDeclareVRChatVariables)
            {
                templateEngine.AddTag("SHOULD_DECLARE_VRCHAT_VARIABLES", "true");
            }
            if (_shouldDeclareAudioLinkVariables)
            {
                templateEngine.AddTag("SHOULD_DECLARE_AUDIOLINK_VARIABLES", "true");
            }
            if (_shouldDeclareProTVVariables)
            {
                templateEngine.AddTag("SHOULD_DECLARE_PROTV_VARIABLES", "true");
            }

            return templateEngine;
        }

        [MenuItem("Tools/lilToon Custom Generator")]
        private static void OpenWindow()
        {
            GetWindow<LilToonCustomGeneratorWindow>("lilToon Custom Generator");
        }
    }
}
