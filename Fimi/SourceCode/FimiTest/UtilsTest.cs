using System.Xml;

namespace Fimi
{
    public static class UtilsTest
    {
        private static readonly string _pathToConfig;

        static UtilsTest()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PathToSettings.xml"));
            if(xmlDocument.SelectSingleNode("/Config/PathToConfig") is null)
            {
                throw new ShukrMoliyaException("Incorrect path to config file in xml file");
            }

            _pathToConfig = xmlDocument.SelectSingleNode("/Config/PathToConfig")!.InnerText;

            if (_pathToConfig is null || _pathToConfig.Length == 0)
            {
                throw new ShukrMoliyaException("Incorrect path to config file");
            }
        }

        private static XmlDocument GetXmlDocument()
        {
            XmlDocument config;
            if(_pathToConfig is not null)
            {
                config = new XmlDocument();
                config.Load(_pathToConfig);
                return config;
            }
            throw new ShukrMoliyaException("Incorrect path to config file");
        }

        public static string Url()
        {
            var config = GetXmlDocument();
            string p;
            p = config.SelectSingleNode("/Config/fimi/url")!.InnerText;

            if(p.Length == 0)
            {
                throw new ShukrMoliyaException("Url is empty");
            }

            return p;
        }

        public static string RetAddress()
        {
            var config = GetXmlDocument();
            string p;
            p = config.SelectSingleNode("/Config/fimi/retaddress")!.InnerText;

            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("RetAddress is empty");
            }

            return p;
        }

        public static string Term()
        {
            var config = GetXmlDocument();
            string p;
            p = config.SelectSingleNode("/Config/fimi/term")!.InnerText;

            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("Term is empty");
            }

            return p;
        }

        public static string Vendor()
        {
            var config = GetXmlDocument();
            string p;
            p = config.SelectSingleNode("/Config/fimi/vendor")!.InnerText;

            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("Vendor is empty");
            }

            return p;
        }

        public static string Clerk()
        {
            var config = GetXmlDocument();
            string p;
            p = config.SelectSingleNode("/Config/fimi/clerk")!.InnerText;

            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("Clerk is empty");
            }

            return p;
        }

        public static string Password()
        {
            var config = GetXmlDocument();
            string p;
            p = config.SelectSingleNode("/Config/fimi/password")!.InnerText;

            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("Password is empty");
            }

            return p;
        }

        public static string SessionId()
        {
            var config = GetXmlDocument();
            string p;
            p = config.SelectSingleNode("/Config/fimi/session/id")!.InnerText;

            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("SessionId is empty");
            }

            return p;
        }

        public static string SessionDate()
        {
            var config = GetXmlDocument();
            string p;
            p = config.SelectSingleNode("/Config/fimi/session/date")!.InnerText;

            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("SessionDate is empty");
            }

            return p;
        }

        public static void SetSessionId(string Value)
        {
            var config = GetXmlDocument();

            config.SelectSingleNode("/Config/fimi/session/id")!.InnerText = Value;
            config.SelectSingleNode("/Config/fimi/session/date")!.InnerText = DateTime.Now.ToString("yyyyMMddHHmmss");
            config.Save(_pathToConfig);
        }

        public static string LogPath()
        {
            var config = GetXmlDocument();
            string p = config.SelectSingleNode("/Config/fimi/logPath")!.InnerText;

            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("LogPath is empty");
            }

            return p;
        }

        public static string PathToRequestFile()
        {
            var config = GetXmlDocument();
            string p = config.SelectSingleNode("/Config/fimi/pathToRequestFile")!.InnerText;

            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("PathToRequestFile is empty");
            }

            return p;
        }

        public static string PANTest()
        {
            var config = GetXmlDocument();
            string p = config.SelectSingleNode("/Config/fimi/PANTest")!.InnerText;

            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("PANTest is empty");
            }

            return p;
        }
    }
}
