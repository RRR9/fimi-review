using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Fimi
{
    public static class SoapTest
    {
        private static string _pathToRequestFile;

        static SoapTest()
        {
            _pathToRequestFile = "";
        }

        private static void SetPathToRequestFile()
        {
            _pathToRequestFile = UtilsTest.PathToRequestFile();
            if (_pathToRequestFile is null)
            {
                throw new ShukrMoliyaException("Path to request file is empty");
            }
        }

        public static string InitSessionRq()
        {
            SetPathToRequestFile();

            string s = "";
            using (StreamReader sr = new StreamReader(Path.Combine(_pathToRequestFile, "InitSessionRq.xml")))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sr.ReadToEnd());

                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["RetAddress"]!.Value = UtilsTest.RetAddress();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["Clerk"]!.Value = UtilsTest.Clerk();

                s = PrettyXml(xmlDoc.ToString()!);
            }

            if(s.Length == 0)
            {
                throw new ShukrMoliyaException("InitSessionRq failed");
            }

            return s;
        }

        public static string LogonRq()
        {
            SetPathToRequestFile();

            string s = "";
            using (StreamReader sr = new StreamReader(Path.Combine(_pathToRequestFile, "LogonRq.xml")))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sr.ReadToEnd());

                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["Session"]!.Value = UtilsTest.SessionId();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["RetAddress"]!.Value = UtilsTest.RetAddress();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["Clerk"]!.Value = UtilsTest.Clerk();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["Password"]!.Value = UtilsTest.Password();

                s = PrettyXml(xmlDoc.ToString()!);
            }

            if (s.Length == 0)
            {
                throw new ShukrMoliyaException("LogonRq failed");
            }

            return s;
        }

        public static string GetCardInfoRq()
        {
            SetPathToRequestFile();

            string s = "";
            using (StreamReader sr = new StreamReader(Path.Combine(_pathToRequestFile, "GetCardInfoRq.xml")))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sr.ReadToEnd());

                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["Session"]!.Value = UtilsTest.SessionId();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["RetAddress"]!.Value = UtilsTest.RetAddress();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["Clerk"]!.Value = UtilsTest.Clerk();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["Password"]!.Value = UtilsTest.Password();

                string PANTest = UtilsTest.PANTest();
                if(PANTest.Length != 0)
                {
                    xmlDoc.SelectSingleNode("//fimi1:PAN")!.InnerText = UtilsTest.PANTest();
                }

                s = PrettyXml(xmlDoc.ToString()!);
            }

            if (s.Length == 0)
            {
                throw new ShukrMoliyaException("GetCardInfoRq failed");
            }

            return s;
        }

        public static string POSRequestRq(string transactionNumber)
        {
            SetPathToRequestFile();

            string s = "";
            using (StreamReader sr = new StreamReader(Path.Combine(_pathToRequestFile, "POSRequestRq.xml")))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sr.ReadToEnd());

                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["Clerk"]!.Value = UtilsTest.Clerk();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["Password"]!.Value = UtilsTest.Password();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["RetAddress"]!.Value = UtilsTest.RetAddress();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["TransactionNumber"]!.Value = transactionNumber;

                s = PrettyXml(xmlDoc.ToString()!);
            }

            if (s.Length == 0)
            {
                throw new ShukrMoliyaException("POSRequestRq failed");
            }

            return s;
        }

        public static string ReverseTransactionRq(string thisTranId)
        {
            SetPathToRequestFile();

            string s = "";
            using (StreamReader sr = new StreamReader(Path.Combine(_pathToRequestFile, "ReverseTransactionRq.xml")))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sr.ReadToEnd());

                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["Session"]!.Value = UtilsTest.SessionId();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["RetAddress"]!.Value = UtilsTest.RetAddress();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["Clerk"]!.Value = UtilsTest.Clerk();
                xmlDoc.SelectSingleNode("//fimi:Request")!.Attributes!["Password"]!.Value = UtilsTest.Password();

                xmlDoc.SelectSingleNode("//fimi1:Id")!.InnerText = thisTranId;

                s = PrettyXml(xmlDoc.ToString()!);
            }

            if (s.Length == 0)
            {
                throw new ShukrMoliyaException("ReverseTransactionRq failed");
            }

            return s;
        }

        private static string PrettyXml(string xml)
        {
            var stringBuilder = new StringBuilder();

            var element = XElement.Parse(xml);

            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            settings.NewLineOnAttributes = false;

            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                element.Save(xmlWriter);
            }

            return stringBuilder.ToString();
        }
    }
}
