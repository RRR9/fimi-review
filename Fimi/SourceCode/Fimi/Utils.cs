using System.Xml;

namespace Fimi
{
    public static class Utils
    {
        private static readonly string _pathToConfig;

        public static readonly string PathToRSAPublicPrivateKey;
        public static readonly string LogPath;
        public static readonly string PathToRequestFile;

        static Utils()
        {
            XmlDocument xmlDocument = new XmlDocument();
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            string s = "";

            using (StreamReader sr = new StreamReader(Path.Combine(currentPath, "PathToSettings.xml")))
            {
                s = sr.ReadToEnd();
            }

            if (s.Length == 0)
            {
                throw new ShukrMoliyaException("Empty settings");
            }

            xmlDocument.LoadXml(s);
            if (xmlDocument.SelectSingleNode("/Config/PathToConfigFimi") is null)
            {
                throw new ShukrMoliyaException("Incorrect path to config file in xml file");
            }

            PathToRequestFile = xmlDocument.SelectSingleNode("/Config/pathToRequestFile")!.InnerText;
            LogPath = xmlDocument.SelectSingleNode("/Config/LogPath")!.InnerText;
            PathToRSAPublicPrivateKey = xmlDocument.SelectSingleNode("/Config/PathToRSAPublicPrivateKey")!.InnerText;
            _pathToConfig = xmlDocument.SelectSingleNode("/Config/PathToConfigFimi")!.InnerText;

            PathToRequestFile = Path.GetFullPath(Path.Combine("", PathToRequestFile));
            LogPath = Path.GetFullPath(Path.Combine("", LogPath));
            PathToRSAPublicPrivateKey = Path.GetFullPath(Path.Combine("", PathToRSAPublicPrivateKey));
            _pathToConfig = Path.GetFullPath(Path.Combine("", _pathToConfig));

            if (_pathToConfig is null || _pathToConfig.Length == 0)
            {
                throw new ShukrMoliyaException("Incorrect path to config file");
            }

            if(PathToRSAPublicPrivateKey is null || PathToRSAPublicPrivateKey.Length == 0)
            {
                throw new ShukrMoliyaException("Incorrect path to RSA file");
            }

            if(LogPath is null || LogPath.Length == 0)
            {
                throw new ShukrMoliyaException("Incorrect path to RSA file");
            }

            if(PathToRequestFile is null ||  PathToRequestFile.Length == 0)
            {
                throw new ShukrMoliyaException("Incorrect path to SOAP reuqest directory");
            }
        }

        private static XmlDocument GetXmlDocument()
        {
            XmlDocument config;
            if (_pathToConfig is not null)
            {
                config = new XmlDocument();
                config.Load(_pathToConfig);
                return config;
            }
            throw new ShukrMoliyaException("Incorrect path to config file");
        }

        public static string DbConnection()
        {
            var config = GetXmlDocument();
            string p;
            p = config.SelectSingleNode("/Config/fimi/DBConnection")!.InnerText;
            
            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("Database connection string is empty");
            }

            return p;
        }

        public static string PasswordMD5()
        {
            var config = GetXmlDocument();
            string p;
            p = config.SelectSingleNode("/Config/fimi/passwordMD5")!.InnerText;

            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("PasswordMD5 is empty");
            }

            return p;
        }

        public static string SessionId(SessionType sessionType) // => !
        {
            var config = GetXmlDocument();
            string p = "";

            if(sessionType == SessionType.POSRequestRq)
            {
                p = config.SelectSingleNode("/Config/fimi/sessionPOSRequestRq/id")!.InnerText;
            }
            else if(sessionType == SessionType.ReverseTransactionRq)
            {
                p = config.SelectSingleNode("/Config/fimi/sessionReverseTransactionRq/id")!.InnerText;
            }

            if (p.Length == 0)
            {
                throw new ShukrMoliyaException("SessionId is empty");
            }

            return p;
        }

        public static string SessionDate(SessionType sessionType) // => !
        {
            var config = GetXmlDocument();
            string p = "";

            if (sessionType == SessionType.POSRequestRq)
            {
                p = config.SelectSingleNode("/Config/fimi/sessionPOSRequestRq/date")!.InnerText;

                if (p.Length == 0)
                {
                    throw new ShukrMoliyaException("SessionDate is empty");
                }
            }
            else if(sessionType == SessionType.ReverseTransactionRq)
            {
                p = config.SelectSingleNode("/Config/fimi/sessionReverseTransactionRq/date")!.InnerText;

                if (p.Length == 0)
                {
                    throw new ShukrMoliyaException("SessionDate is empty");
                }
            }
            
            return p;
        }

        public static void SetSessionId(string value, SessionType sessionType) // => !
        {
            var config = GetXmlDocument();

            if(sessionType == SessionType.POSRequestRq)
            {
                config.SelectSingleNode("/Config/fimi/sessionPOSRequestRq/id")!.InnerText = value;
                config.SelectSingleNode("/Config/fimi/sessionPOSRequestRq/date")!.InnerText = DateTime.Now.ToString("yyyyMMddHHmmss");
            }
            else if(sessionType == SessionType.ReverseTransactionRq)
            {
                config.SelectSingleNode("/Config/fimi/sessionReverseTransactionRq/id")!.InnerText = value;
                config.SelectSingleNode("/Config/fimi/sessionReverseTransactionRq/date")!.InnerText = DateTime.Now.ToString("yyyyMMddHHmmss");
            }
            
            config.Save(_pathToConfig);
        }

        public static string GetConfig()
        {
            string config = "";

            using(StreamReader sr = new StreamReader(_pathToConfig))
            {
                config = sr.ReadToEnd();
            }
            return config;
        }
    }
}
