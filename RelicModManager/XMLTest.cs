using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RelicModManager
{
    public partial class XMLTest : Form
    {
        public XMLTest()
        {
            InitializeComponent();
        }

        private void XMLTest_Load(object sender, EventArgs e)
        {
            if (File.Exists("engine_config_test.xml")) File.Delete("engine_config_test.xml");
            File.Copy("engine_config.xml", "engine_config_test.xml");
            this.addBank("RelHaxGui.bnk");
            this.addBank("RelHaxGui2.bnk");

            this.removeBank("RelHaxGui.bnk");
            this.removeBank("RelHaxGui2.bnk");

            this.addBank("voiceover.bnk");
        }

        private void increaseSoundMemory()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("engine_config_test.xml");
            //patch defaultPool
            XmlNode defaultPool = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/defaultPool");
            defaultPool.InnerText = "32";
            //patch defaultPool
            XmlNode lowEnginePool = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/lowEnginePool");
            lowEnginePool.InnerText = "24";
            //patch defaultPool
            XmlNode preparedPool = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/preparedPool");
            preparedPool.InnerText = "256";
            //patch defaultPool
            XmlNode streamingPool = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/streamingPool");
            streamingPool.InnerText = "8";
            //patch defaultPool
            XmlNode IOPoolSize = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/IOPoolSize");
            IOPoolSize.InnerText = "12";
            if (File.Exists("engine_config_test.xml")) File.Delete("engine_config_test.xml");
            doc.Save("engine_config_test.xml");
        }

        private void addBank(string bankName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("engine_config_test.xml");
            //XmlElement layerOne = doc.CreateElement("engine_config.xml");
            //add check for voiceover.bnk
            XmlNode rel1 = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/voice_soundbanks/project/name");
            if (rel1 == null)
            //no soundbanks
            {
                XmlNode reff = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/voice_soundbanks");
                //create project node
                XmlElement project = doc.CreateElement("project");

                //create new soundbank node
                XmlElement rel2 = doc.CreateElement("name");
                rel2.InnerText = "voiceover.bnk";

                //insert soundbank into project
                project.InsertAfter(rel2, project.FirstChild);

                //insert project into voice_soundbanks
                reff.InsertAfter(project, reff.FirstChild);

                if (File.Exists("engine_config_test.xml")) File.Delete("engine_config_test.xml");
                doc.Save("engine_config_test.xml");
                return;
            }
            else if (rel1.InnerText == "voiceover.bnk")
            {
                rel1.InnerText = bankName;
            }
            else
            {
                //create refrence node
                XmlNode reff = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/voice_soundbanks");

                //create project node
                XmlElement project = doc.CreateElement("project");

                //create new soundbank node
                XmlElement rel2 = doc.CreateElement("name");
                rel2.InnerText = bankName;

                //insert soundbank into project
                project.InsertAfter(rel2, project.FirstChild);

                //insert project into voice_soundbanks
                reff.InsertAfter(project, reff.FirstChild);
            }
            if (File.Exists("engine_config_test.xml")) File.Delete("engine_config_test.xml");
            doc.Save("engine_config_test.xml");
        }

        //removes a sound bank to the engine config xml file
        private void removeBank(string bankName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("engine_config_test.xml");
            //check to see if it's already there
            XmlNode node = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/voice_soundbanks/project/name");
            node.ParentNode.RemoveAll();
            //save
            if (File.Exists("engine_config_test.xml")) File.Delete("engine_config_test.xml");
            doc.Save("engine_config_test.xml");

            XDocument doc2 = XDocument.Load("engine_config_test.xml");
            doc2.Descendants().Elements("project").Where(e => string.IsNullOrEmpty(e.Value)).Remove();
            doc2.Save("engine_config_test.xml");
        }
    }
}
