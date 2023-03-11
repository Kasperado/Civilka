using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Civilka {
    class Config {
        static readonly string configPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\ClientConfig.xml";
        static XmlDocument clientConfig = new XmlDocument();
        public static void loadConfig() {
            clientConfig.Load(configPath);
            setGraphics(clientConfig["ClientConfig"]["Graphics"]);
            Console.WriteLine("Configuration loaded successfully.");
        }

        private static void setGraphics(XmlElement node) {
            Graphics.width = int.Parse(node.Attributes["width"].Value);
            Graphics.height = int.Parse(node.Attributes["height"].Value);
        }

        public static class Graphics {
            public static int width;
            public static int height;
        }
    }
}
