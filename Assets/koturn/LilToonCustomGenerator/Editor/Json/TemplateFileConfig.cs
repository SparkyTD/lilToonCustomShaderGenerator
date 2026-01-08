using System;


namespace Koturn.LilToonCustomGenerator.Editor.Json
{
    [Serializable]
    internal class TemplateFileConfig
    {
        /// <summary>
        /// Template file GUID.
        /// </summary>
        public string guid;
        /// <summary>
        /// Destination file path.
        /// </summary>
        public string destination;
    }
}
