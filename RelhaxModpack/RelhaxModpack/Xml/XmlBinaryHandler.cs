using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Xml
{
#pragma warning disable CS1591
    public partial class XmlBinaryHandler
    {
        private PackedSectionReader PS = new PackedSectionReader();
        private PrimitiveFileReader PF = new PrimitiveFileReader();
        private StringBuilder DecodedXML = new StringBuilder();

        private static readonly Int32 Binary_Header = 0x42a14e65;

        private XmlDocument xDoc;

        public void UnpackXmlFile(string FileNameLoad, string FileNameSave = "")
        {
            if (FileNameLoad.Length != 0 && File.Exists(FileNameLoad))
            {
                if (OpenFile(FileNameLoad))
                {
                    if (FileNameSave.Length == 0)
                        FileNameSave = FileNameLoad;
                    SaveFile(FileNameSave);
                    Logging.Info(string.Format("Saved processed file: {0} ({1})", FileNameSave, Path.GetFileName(FileNameLoad)),Logfiles.Application);
                }
            }
            else
            {
                Logging.Warning(string.Format("Failed to process file: {0} (not existing)", FileNameLoad));
            }
            Dispose();
        }

        private string FormatXml(string sUnformattedXml)
        {
            //load unformatted xml into a dom
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(sUnformattedXml);

            //will hold formatted xml
            StringBuilder sb = new StringBuilder();

            //pumps the formatted xml into the StringBuilder above
            StringWriter sw = new StringWriter(sb);

            //does the formatting
            XmlTextWriter xtw = null;

            try
            {
                //point the xtw at the StringWriter
                //slap it into a using block because it's disposable
                using (xtw = new XmlTextWriter(sw) { Formatting = Formatting.Indented })
                {
                    //get the dom to dump its contents into the xtw 
                    xd.WriteTo(xtw);
                }
            }
            finally
            {
                //clean up even if error
                if (xtw != null)
                {
                    xtw.Close();
                    xtw.Dispose();
                }
            }

            //return the formatted xml
            return sb.ToString();
        }

        private bool DecodePackedFile(BinaryReader reader, string PackedFileName)
        {
            try
            {
                reader.ReadSByte();
                List<string> dictionary = PS.ReadDictionary(reader);
                XmlNode xmlroot = xDoc.CreateNode(XmlNodeType.Element, PackedFileName, "");
                PS.ReadElement(reader, xmlroot, xDoc, dictionary);
                xDoc.AppendChild(xmlroot);
                DecodedXML.Append(FormatXml(xDoc.OuterXml));
                return true;
            }
            catch (Exception ex)
            {
                Logging.Exception("DecodePackedFile", "File: " + PackedFileName, ex);
                return false;
            }
        }

        private bool ReadPrimitiveFile(string file)
        {
            try
            {
                FileStream F = new FileStream(file, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(F);

                XmlComment ptiComment = xDoc.CreateComment("DO NOT SAVE THIS FILE! THIS CODE IS JUST FOR INFORMATION PUPORSES!");

                XmlNode xmlprimitives = xDoc.CreateNode(XmlNodeType.Element, "primitives", "");

                PF.ReadPrimitives(reader, xmlprimitives, xDoc);

                xDoc.AppendChild(ptiComment);
                xDoc.AppendChild(xmlprimitives);

                DecodedXML.Append(FormatXml(xDoc.OuterXml));
                reader.Dispose();
                return false;       // send false in any case, so the content will not be saved !
            }
            catch (Exception ex)
            {
                Logging.Exception("ReadPrimitiveFile", "File: " + file, ex);
                return false;
            }
        }

        private bool OpenFile(string filename)
        {
            bool result = false;
            xDoc = new XmlDocument();
            DecodedXML.Clear();
            string PackedFileName = Path.GetFileName(filename).ToLower();
            FileStream F = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(F);
            Int32 head = reader.ReadInt32();
            try
            {
                if (head == PackedSectionReader.Packed_Header)
                {
                    result = DecodePackedFile(reader, Path.GetFileName(filename));
                }
                else if (head == Binary_Header)
                {
                    result = ReadPrimitiveFile(filename);
                }
                else
                {
                    if (PackedFileName.Contains(".xml") || PackedFileName.Contains(".def") || PackedFileName.Contains(".visual") || PackedFileName.Contains(".chunk") || PackedFileName.Contains(".settings") || PackedFileName.Contains(".model"))
                    {
                        Logging.Warning(string.Format("Warning: File {0} seams to be a plain text file (no binary), so no processing!", filename));
                    }
                    else
                    {
                        Logging.Error(string.Format("ERROR: File {0} with Invalid header. No processing!", filename));
                    }
                }
            }
            finally
            {
                reader.Close();
                F.Close();
                F.Dispose();
            }
            return result;
        }

        private void SaveFile(string FileName)
        {
            xDoc.Save(FileName);
        }

        private void Dispose()
        {
            PS = null;
            PF = null;
            DecodedXML = null;
            xDoc = null;
            GC.Collect();
        }
    }
#pragma warning restore CS1591
}
