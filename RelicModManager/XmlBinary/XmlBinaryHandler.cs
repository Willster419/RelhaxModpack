using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
// using Packed_Section_Reader;
// using Primitive_File_Reader;

namespace RelhaxModpack.XmlBinary
{
    public partial class XmlBinary
    {
        // public string PackedFileName = "";
        // public static readonly string sver = "0.5";
        // public static readonly string stitle = "WoT Mod Tools ";
        public Packed_Section_Reader PS = new Packed_Section_Reader();
        public Primitive_File_Reader PF = new Primitive_File_Reader();
        private StringBuilder DecodedXML = new StringBuilder();

        public static readonly Int32 Binary_Header = 0x42a14e65;

        public XmlDocument xDoc;

        public void Handler(string FileNameLoad, string FileNameSave = "")
        {
            // DecodedXML.Clear();
            // InitializeComponent();
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
                Utils.ExceptionLog("DecodePackedFile", ex);
                return false;
            }
        }

        private bool ReadPrimitiveFile(string file)
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

        private bool openFile(string filename)
        {
            bool result = false;
            // saveAsToolStripMenuItem.Enabled = false;
            // btnSave.Enabled = false;
            xDoc = new XmlDocument();
            DecodedXML.Clear();
            string PackedFileName = Path.GetFileName(filename).ToLower();
            // PackedFileName = PackedFileName.ToLower();
            // Text = stitle + sver + " - " + PackedFileName;
            FileStream F = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(F);
            Int32 head = reader.ReadInt32();
            try
            {
                if (head == Packed_Section_Reader.Packed_Header)
                {
                    result = DecodePackedFile(reader, Path.GetFileName(filename));
                    // saveAsToolStripMenuItem.Enabled = true;
                    // btnSave.Enabled = true;
                }
                else if (head == Binary_Header)
                {
                    result = ReadPrimitiveFile(filename);
                    //saveAsToolStripMenuItem.Enabled = true;
                    //btnSave.Enabled = true;
                }
                else
                {
                    if (PackedFileName.Contains(".xml") || PackedFileName.Contains(".def") || PackedFileName.Contains(".visual") || PackedFileName.Contains(".chunk") || PackedFileName.Contains(".settings") || PackedFileName.Contains(".model"))
                    {
                        // saveAsToolStripMenuItem.Enabled = true;
                        // btnSave.Enabled = true;
                        // DecodedXML.LoadFile(file, RichTextBoxStreamType.PlainText);
                        Utils.AppendToLog(string.Format("Warning: File {0} seams to be a plain text file (no binary), so no processing!", filename));
                    }
                    else
                    {
                        // saveAsToolStripMenuItem.Enabled = false;
                        // btnSave.Enabled = false;
                        // throw new IOException("Invalid header");
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

        // private void openToolStripMenuItem_Click(object sender, EventArgs e)
        // {
        //     using (var ofd = new OpenFileDialog { Filter = "WoT Packed XML|*.xml;*.def;*.visual;*.chunk;*.settings;*.primitives;*.model;*.animation;*.anca|All files|*.*" })
        //         if (DialogResult.OK == ofd.ShowDialog())
        //         {
        //             openFile(ofd.FileName);
        //         }
        // }

        private void saveFile(string FileName)
        {
            // using (var sfd = new SaveFileDialog { })
            // {
                // sfd.Filter = "Unpacked XML|*.xml";
                // string SaveFileName = PackedFileName;
                // if (!PackedFileName.Contains(".xml"))
                //     SaveFileName = SaveFileName + ".xml";

                // sfd.FileName = SaveFileName;
                // if (DialogResult.OK == sfd.ShowDialog())
                // {
                    xDoc.Save(FileName);
                    //txtOut.SaveFile(sfd.FileName, RichTextBoxStreamType.UnicodePlainText);
                // }
            // }
        }

        // private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        // {
        //     using (var sfd = new SaveFileDialog { })
        //     {
        //         sfd.Filter = "Unpacked XML|*.xml";
        //         string SaveFileName = PackedFileName;
        //         if (!PackedFileName.Contains(".xml"))
        //             SaveFileName = SaveFileName + ".xml";

        //         sfd.FileName = SaveFileName;
        //         if (DialogResult.OK == sfd.ShowDialog())
        //         {
        //             xDoc.Save(sfd.FileName);
        //txtOut.SaveFile(sfd.FileName, RichTextBoxStreamType.UnicodePlainText);
        //         }
        //     }
        // }

        // private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        // {
        //     Close();
        // }

        // private void MainForm_Load(object sender, EventArgs e)
        // {
        //     this.Text = stitle + sver;
        //     this.DragEnter += new DragEventHandler(MainForm_DragEnter);
        //     this.DragDrop += new DragEventHandler(MainForm_DragDrop);
        // }

        // void MainForm_DragEnter(object sender, DragEventArgs e)
        // {
        //     if (e.Data.GetDataPresent(DataFormats.FileDrop))
        //         e.Effect = DragDropEffects.Copy;
        //     else
        //         e.Effect = DragDropEffects.None;

        // }

        // void MainForm_DragDrop(object sender, DragEventArgs e)
        // {
        //     string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
        // 
        //     foreach (string File in FileList)
        //         openFile(File);
        // }

        // private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        // {
        //     using (var abd = new AboutBox { })
        //     {
        //         if (DialogResult.Cancel == abd.ShowDialog())
        //         {
        //         }
        //     }       
        // }

        // private void newToolStripMenuItem_Click(object sender, EventArgs e)
        // {
        // saveAsToolStripMenuItem.Enabled = false;
        // btnSave.Enabled = false;
        // txtOut.Clear();
        // }

        // private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        // {
        //
        // }
    }
}
