﻿using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;

namespace RelhaxModpack
{
    /// <summary>
    /// A patch is an instruction object of how to modify a text file. Can be a json, xml, or plain text file
    /// </summary>
    public class Patch
    {
        /// <summary>
        /// A single string with the filename of the processingNativeFile (needed for tracing work instructions after installation)
        /// </summary>
        public string NativeProcessingFile = string.Empty;

        /// <summary>
        /// the actual name of the original patch before processed
        /// </summary>
        public string ActualPatchName = string.Empty;

        /// <summary>
        /// The type of patch, xml or regex (direct text replacement)
        /// </summary>
        public string Type = string.Empty;

        /// <summary>
        /// If not regex, the mode that the xml patcher should use.<para/>Examples: add xml node, remove xml node, edit xml node
        /// </summary>
        public string Mode = string.Empty;

        /// <summary>
        /// The starting path to the file
        /// </summary>
        public string PatchPath = string.Empty;

        /// <summary>
        /// The path to the file, relative to patchPath
        /// </summary>
        public string File = string.Empty;

        /// <summary>
        /// The complete path to the file, saved at parse time
        /// </summary>
        public string CompletePath = string.Empty;

        /// <summary>
        /// Saves the complete path for if in editor mode, otherwise not used
        /// </summary>
        public string FollowPathEditorCompletePath = string.Empty;

        /// <summary>
        /// The version of the patch for parsing. Allows for multiple variations. Default to 1
        /// </summary>
        public int Version = 1;

        /// <summary>
        /// If xml or json, the xml xpath or json jsonpath to the node
        /// </summary>
        public string Path = string.Empty;

        /// <summary>
        /// If regex, the optional specific lines in the text file
        /// </summary>
        public string[] Lines;

        /// <summary>
        /// The node inner text (xml) or regex criteria to search for
        /// </summary>
        public string Search = string.Empty;

        /// <summary>
        /// The text to replace the found search text with
        /// </summary>
        public string Replace = string.Empty;

        /// <summary>
        /// For json patches, if it should use the new method of separating the path for getting the xvm references
        /// </summary>
        public bool FollowPath = false;

        /// <summary>
        /// If from editor/patch designer, enable verbose logging for the duration of that patch
        /// </summary>
        public bool FromEditor = false;

        /// <summary>
        /// Collects all patch information for logging
        /// </summary>
        public string DumpPatchInfoForLog
        {
            get
            {
                return string.Format("{0} patch, Version={1}, NativeProcessingFile={2}, ActualFile={3}," +
                    "{4}{5}PatchPath={6}, FileToPatch={7}," +
                    "{8}{9}Lines={10}, Path={11}, Search={12}, Replace={13}",
                    Type.ToLower(),
                    Version,
                    NativeProcessingFile,
                    ActualPatchName,
                    Environment.NewLine,
                    Settings.LogSpacingLineup,
                    PatchPath,
                    File,
                    Environment.NewLine,
                    Settings.LogSpacingLineup,
                    Lines == null ? "null" : string.Join(",", Lines),
                    string.IsNullOrEmpty(Path) ? "null" :Path,
                    Search,
                    Replace);
            }
        }

        /// <summary>
        /// The string representation of the object
        /// </summary>
        /// <returns>The type, mode and lines/path if in editor mode. Else, the base ToString()</returns>
        public override string ToString()
        {
            return FromEditor? string.Format("type={0} ,mode={1}, lines/path={2}", Type, Mode, Lines == null ? Path : string.Join(",", Lines)) : base.ToString();
        }

        public bool IsValidForSave
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Type))
                    return false;

                if (string.IsNullOrWhiteSpace(Mode))
                    return false;

                if (string.IsNullOrWhiteSpace(PatchPath))
                    return false;

                if (string.IsNullOrWhiteSpace(File))
                    return false;

                if (string.IsNullOrWhiteSpace(Search) && string.IsNullOrWhiteSpace(Replace))
                    return false;

                return true;
            }
        }

        public static List<Patch> GetInvalidPatchesForSave(Patch[] patchList)
        {
            return GetInvalidPatchesForSave(patchList.ToList());
        }

        public static List<Patch> GetInvalidPatchesForSave(List<Patch> patchList)
        {
            //https://stackoverflow.com/questions/1938204/linq-where-vs-findall
            return patchList.FindAll(patch => !patch.IsValidForSave);
        }

        public static List<Patch> GetInvalidPatchesForSave(ItemCollection patchList)
        {
            //https://stackoverflow.com/questions/471595/casting-an-item-collection-from-a-listbox-to-a-generic-list
            return GetInvalidPatchesForSave(patchList.Cast<Patch>().ToList());
        }

    }
}
