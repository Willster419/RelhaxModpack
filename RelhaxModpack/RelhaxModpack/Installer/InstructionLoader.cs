using RelhaxModpack.Atlases;
using RelhaxModpack.Patching;
using RelhaxModpack.Shortcuts;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RelhaxModpack.Installer
{
    /// <summary>
    /// The instructionLoader class is for loading instructions from legacy style individual xml documents into a list of Instruction objects.
    /// </summary>
    public class InstructionLoader
    {
        /// <summary>
        /// Create a list of instructions from xml files in a directory.
        /// </summary>
        /// <param name="folderPath">The directory path to the location of xml files.</param>
        /// <param name="instructionsType">The type of instructions to load.</param>
        /// <param name="xmlSearchpath">The xml xpath to use to get the list of xml objects of each instruction xml.</param>
        /// <param name="originalPatchNames">A dictionary to serve as a map for each original filename of each xml document.</param>
        /// <returns>The loaded list of instructions, or an empty list if no instructions loaded.</returns>
        public List<Instruction> CreateInstructionsList(string folderPath, InstructionsType instructionsType, string xmlSearchpath, Dictionary<string, string> originalPatchNames = null)
        {
            if (string.IsNullOrEmpty(folderPath))
                throw new BadMemeException("FolderPath is null or empty");
            if (string.IsNullOrEmpty(xmlSearchpath))
                throw new BadMemeException("XmlSearchPath is null or empty");

            if (!Directory.Exists(folderPath))
            {
                Logging.Info(LogOptions.ClassName, "Directory {0} does not exist, skip parsing");
                return null;
            }

            //search for all files that we want to use to add to
            string[] filesList = FileUtils.FileSearch(folderPath, SearchOption.TopDirectoryOnly, false, false, @"*.xml", 50, 3, true);
            if (filesList == null)
            {
                Logging.Error(LogOptions.ClassName, "Failed to search for xml instruction files in directory {0}", folderPath);
                return null;
            }

            List<Instruction> instructions = new List<Instruction>();

            //for each instruction xml file, add them
            Logging.Debug(LogOptions.ClassName, "Parsed {0} xml files from path {1}", filesList.Count(), folderPath);
            foreach (string file in filesList)
            {
                AddInstructionObjectsToList(file, instructions, instructionsType, xmlSearchpath, originalPatchNames);
            }

            return instructions;
        }

        /// <summary>
        /// Adds instructions to a given Instruction list.
        /// </summary>
        /// <param name="file">The file that contains instructions to load.</param>
        /// <param name="instructions">The instructions list to add loaded instructions to.</param>
        /// <param name="instructionsType">The type of instructions to load.</param>
        /// <param name="xmlSearchpath">The xml xpath to use to get the list of xml objects of each instruction xml.</param>
        /// <seealso cref="InstructionsType"/>
        /// <seealso cref="Instruction"/>
        public void AddInstructionObjectsToList(string file, List<Instruction> instructions, InstructionsType instructionsType, string xmlSearchpath)
        {
            if (!File.Exists(file))
            {
                throw new BadMemeException("file does not exist");
            }

            AddInstructionObjectsToList(file, instructions, instructionsType, xmlSearchpath, null);
        }

        private void AddInstructionObjectsToList(string file, List<Instruction> instructions, InstructionsType instructionsType, string xmlSearchpath, Dictionary<string, string> originalPatchNames = null)
        {
            XDocument doc = XmlUtils.LoadXDocument(file, XmlLoadType.FromFile);
            if (doc == null)
            {
                Logging.Warning(LogOptions.ClassName, "Failed to parse xml file {0}", file);
                return;
            }

            //get all element objects like Patchs
            List<XPathNavigator> resultsList = XmlUtils.GetXNodesFromXpath(doc, xmlSearchpath);
            resultsList = resultsList.FindAll(result => result.NodeType == XPathNodeType.Element);

            foreach (XPathNavigator xPathNavigator in resultsList)
            {
                //now we have the main element like Patch. currently, each instruction part is an element
                Instruction instruction = CreateNewInstructionObject(instructionsType);
                XElement element = XElement.Parse(xPathNavigator.OuterXml);
                instruction.NativeProcessingFile = Path.GetFileName(file);
                if (originalPatchNames != null)
                    instruction.ActualPatchName = originalPatchNames[Path.GetFileName(file)];

                foreach (XElement instructionProperty in element.Elements())
                {
                    string instructionPropertyName = instructionProperty.Name.LocalName;
                    Type instructionType = instruction.GetType();
                    //we need to use a double for loop instead of contains because we need to compare by lower case (the legacy xml and code structure varies)
                    foreach (string property in instruction.PropertiesToSerialize())
                    {
                        string propertyLower = property.ToLower();
                        if (propertyLower.Equals(instructionPropertyName.ToLower()))
                        {
                            PropertyInfo propertyInfo = instructionType.GetProperty(property);
                            if (instructionProperty.HasElements && typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType) && !propertyInfo.PropertyType.Equals(typeof(string)))
                            {
                                //get the list object
                                IList listProperty = propertyInfo.GetValue(instruction) as IList;
                                //get the type of object inside that list
                                Type listObjectType = listProperty.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GenericTypeArguments.Length == 1)
                                    .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GenericTypeArguments[0];
                                foreach (XElement subElement in instructionProperty.Elements())
                                {
                                    if (listObjectType.IsValueType)
                                    {
                                        CommonUtils.SetObjectValue(listObjectType, subElement.Value, out object newObject);
                                        listProperty.Add(newObject);
                                    }
                                    //https://stackoverflow.com/a/2092912/3128017
                                    else if (listObjectType.Equals(typeof(string)))
                                    {
                                        listProperty.Add(subElement.Value);
                                    }
                                    else
                                    {
                                        Logging.Error(LogOptions.ClassName, "Unknown type of parse for instruction: {0}", listObjectType.ToString());
                                    }
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(instructionProperty.Value))
                                CommonUtils.SetObjectProperty(instruction, propertyInfo, instructionProperty.Value.Trim());
                            break;
                        }
                    }
                }

                //add to instructions list and create new object
                instructions.Add(instruction);
            }
        }

        private Instruction CreateNewInstructionObject(InstructionsType instructionsType)
        {
            switch (instructionsType)
            {
                case InstructionsType.Atlas:
                    return new Atlas();
                case InstructionsType.Patch:
                    return new Patch();
                case InstructionsType.Shortcut:
                    return new Shortcut();
                case InstructionsType.UnpackCopy:
                    return new XmlUnpack();
                default:
                    return null;
            }
        }
    }
}
