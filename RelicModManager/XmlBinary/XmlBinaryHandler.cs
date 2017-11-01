using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace RelhaxModpack.XmlBinary
{
    public partial class XmlBinaryHandler
    {
        private Packed_Section_Reader PS = new Packed_Section_Reader();
        private Primitive_File_Reader PF = new Primitive_File_Reader();
        private StringBuilder DecodedXML = new StringBuilder();

        private static readonly Int32 Binary_Header = 0x42a14e65;

        private XmlDocument xDoc;

        public void unPack(string FileNameLoad, string FileNameSave = "")
        {
            if (FileNameLoad.Length != 0 && File.Exists(FileNameLoad))
            {
                if (openFile(FileNameLoad))
                {
                    if (FileNameSave.Length == 0)
                        FileNameSave = FileNameLoad;
                    saveFile(FileNameSave);
                    Utils.AppendToLog(string.Format("Saved processed file: {0} ({1})", FileNameSave, Path.GetFileName(FileNameLoad)));
                }
            }
            else
            {
                Utils.AppendToLog(string.Format("Failed to process file: {0} (not existing)", FileNameLoad));
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
                xtw = new XmlTextWriter(sw);

                //we want the output formatted
                xtw.Formatting = Formatting.Indented;

                //get the dom to dump its contents into the xtw 
                xd.WriteTo(xtw);
            }
            finally
            {
                //clean up even if error
                if (xtw != null)
                    xtw.Close();
            }

            //return the formatted xml
            return sb.ToString();
        }

        private bool DecodePackedFile(BinaryReader reader, string PackedFileName)
        {
            try
            {
                reader.ReadSByte();
                List<string> dictionary = PS.readDictionary(reader);
                XmlNode xmlroot = xDoc.CreateNode(XmlNodeType.Element, PackedFileName, "");
                PS.readElement(reader, xmlroot, xDoc, dictionary);
                xDoc.AppendChild(xmlroot);
                DecodedXML.Append(FormatXml(xDoc.OuterXml));
                return true;
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("DecodePackedFile", "File: " + PackedFileName, ex);
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
                return false;       // send false in any case, so the content will not be saved !
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("ReadPrimitiveFile", "File: " + file, ex);
                return false;
            }
        }

        private bool openFile(string filename)
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
                if (head == Packed_Section_Reader.Packed_Header)
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
                        Utils.AppendToLog(string.Format("Warning: File {0} seams to be a plain text file (no binary), so no processing!", filename));
                    }
                    else
                    {
                        Utils.AppendToLog(string.Format("ERROR: File {0} with Invalid header. No processing!", filename));
                    }
                }
            }
            finally
            {
                reader.Close();
                F.Close();
            }
            return result;
        }

        private void saveFile(string FileName)
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
}
