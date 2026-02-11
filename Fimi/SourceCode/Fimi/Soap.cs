using System.Xml;

namespace Fimi
{
    public class Soap
    {
        private readonly string _pathToRequestFile;
        private readonly SessionType _sessionType;
        private readonly string _operationId;
        private readonly string _notFound;

        public Soap(SessionType sessionType, string operationId, string notFound)
        {
            _pathToRequestFile = Utils.PathToRequestFile;
            if (_pathToRequestFile is null)
            {
                throw new ShukrMoliyaException("Path to request file is empty");
            }

            _operationId = operationId;
            _sessionType = sessionType;
            _notFound = notFound;
        }

        private Dictionary<string, string>? CheckSessionTimeout(string soapRequestName)
        {
            DateTime sessionDate = DateTime.ParseExact(Utils.SessionDate(_sessionType), "yyyyMMddHHmmss", null);
            DateTime currentDate = DateTime.Now;
            var timeout = currentDate - sessionDate;

            if (soapRequestName == "InitSessionRq" && timeout.TotalMinutes < 20)
            {
                return new Dictionary<string, string>
                {
                    { "Session", Utils.SessionId(_sessionType) }
                };
            }

            return null;
        }

        public Dictionary<string, string> RequestBuilder(Dictionary<string, Dictionary<string, string>> nodes, string soapRequestName, ref bool requestPassedFimi)
        {
            if(_sessionType == SessionType.Unknown)
            {
                throw new ShukrMoliyaException("Unknown session type");
            }

            if (_sessionType != SessionType.Unnecessary)
            {
                var d = CheckSessionTimeout(soapRequestName);

                if (d != null)
                {
                    return d;
                }
            }

            using var sr = new StreamReader(Path.Combine(_pathToRequestFile, soapRequestName + ".xml"));
            string s = sr.ReadToEnd();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(s);

            if(nodes.ContainsKey("url") == false)
            {
                throw new ShukrMoliyaException("'url' not found");
            }

            string url = nodes["url"]["value"];
            var savedNodes = new Dictionary<string, string>();

            foreach(var item in nodes)
            {
                var attributes = item.Value;
                string nodeName = item.Key;

                if (attributes.ContainsKey("output"))
                {
                    savedNodes[nodeName] = attributes["from"];
                }
                else if(attributes.ContainsKey("attribute"))
                {
                    string path = attributes["place"];
                    string value = attributes["value"];

                    if (attributes["attribute"] == "true")
                    {
                        xmlDocument.GetElementsByTagName(path)[0]!.Attributes![nodeName]!.Value = value;
                    }
                    else
                    {
                        xmlDocument.GetElementsByTagName(path)[0]!.InnerText = value;
                    }
                }
            }

            var http = new HttpConnect(url);
            http.SendRequest(xmlDocument.InnerXml, _operationId);

            if(!http.RequestPassed)
            {
                throw new ShukrMoliyaException($"Http request failed {url}");
            }

            requestPassedFimi = true;

            var responseXmlDocument = new XmlDocument();
            responseXmlDocument.LoadXml(http.Response!);

            var result = new Dictionary<string, string>();

            foreach(var item in savedNodes)
            {
                string nodeName = item.Key;
                string path = item.Value;
                string attribute = nodes[nodeName]["attribute"];

                if(attribute == null)
                {
                    throw new ShukrMoliyaException($"Incorrect attribue value in {nodeName}");
                }

                if(attribute == "true")
                {
                    result[nodeName] = responseXmlDocument.GetElementsByTagName(path)[0]?.Attributes?[nodeName]?.Value ?? _notFound; //throw new ShukrMoliyaException($"{path} not found");
                }
                else if(attribute == "false")
                {
                    result[nodeName] = responseXmlDocument.GetElementsByTagName(path)[0]?.InnerText ?? _notFound; //throw new ShukrMoliyaException($"{path} not found");
                }
            }

            if(soapRequestName == "InitSessionRq")
            {
                Utils.SetSessionId(result["Session"], _sessionType);
            }

            return result;
        }
    }
}
