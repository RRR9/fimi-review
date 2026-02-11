using System.Xml;

namespace Fimi
{
    public class XmlConfigurationFimi
    {
        private readonly XmlDocument _xmlDoc;
        private readonly Dictionary<string, string> _savedParams;
        private SessionType _sessionType;
        private readonly string _operationId;

        private bool _requestPassedFimi;
        public bool RequestPassedFimi { get { return _requestPassedFimi; } set { _requestPassedFimi = value; } }

        public static readonly string NotFound;

        static XmlConfigurationFimi()
        {
            NotFound = "#Not found#";
        }

        public XmlConfigurationFimi(string xml, Dictionary<string, string> requestParams, string operationId)
        {
            _xmlDoc = new XmlDocument();
            _xmlDoc.LoadXml(xml);

            _operationId = operationId;
            _savedParams = new Dictionary<string, string>();

            SetRequestParams(requestParams);
        }

        private void SetRequestParams(Dictionary<string, string> requestParams)
        {
            foreach(var item in requestParams)
            {
                _savedParams[item.Key] = item.Value;
            }
        }

        private void SetSessionType(string commandName)
        {
            if (commandName == "POSRequestRq" || commandName == "GetCardInfoRq")
            {
                _sessionType = SessionType.POSRequestRq;
            }
            else if (commandName == "ReverseTransactionRq")
            {
                _sessionType = SessionType.ReverseTransactionRq;
            }
            else if(commandName == "P2P" || commandName == "CardVerification")
            {
                _sessionType = SessionType.Unnecessary;
            }
        }

        private void AddParamTrack2()
        {
            if (_savedParams.ContainsKey("PAN") == false || _savedParams.ContainsKey("ExpDate") == false)
            {
                return;
            }

            _savedParams["Track2"] = _savedParams["PAN"] + "=" + _savedParams["ExpDate"];
        }

        public Dictionary<string, string> ExecCommand(string commandName)
        {
            var xmlCommand = "//" + commandName;

            SetSessionType(commandName);

            //if(_sessionType == SessionType.POSRequestRq)
            //{
            //    AddParamTrack2();
            //}

            AddParamTrack2();

            if (_xmlDoc.SelectSingleNode(xmlCommand) is null)
            {
                throw new ShukrMoliyaException($"{xmlCommand} in xml config not found");
            }

            ExecRequiredCommand(xmlCommand);
            ExecMainCommand(xmlCommand);

            return _savedParams;
        }

        private void ExecMainCommand(string xmlCommand)
        {
            if (_xmlDoc.SelectSingleNode(xmlCommand) is null)
            {
                throw new ShukrMoliyaException($"'{xmlCommand}' command not found");
            }

            var xmlNode = _xmlDoc.SelectSingleNode(xmlCommand);

            NodeHandler(xmlNode!);
        }

        private void ExecRequiredCommand(string xmlCommand)
        {
            var xmlRequiredCommand = xmlCommand + "/required";

            if(_xmlDoc.SelectSingleNode(xmlRequiredCommand) is null)
            {
                throw new ShukrMoliyaException("'required' command not found");
            }

            var xmlRequiredNode = _xmlDoc.SelectSingleNode(xmlRequiredCommand);

            if(xmlRequiredNode!.HasChildNodes == false)
            {
                return;
            }

            var xmlNodeList = xmlRequiredNode.ChildNodes;

            for(int i = 0; i < xmlNodeList.Count; i++)
            {
                if (xmlNodeList[i] is null)
                {
                    throw new ShukrMoliyaException("'required' section is incorrect");
                }

                NodeHandler(xmlNodeList[i]!); // xmlNodeList[i]!.Name => "InitSessionRq", "LogonRq"
            }
        }

        private void NodeHandler(XmlNode xmlNode)
        {
            var nodes = new Dictionary<string, Dictionary<string, string>>();

            foreach (XmlNode xmlChildNode in xmlNode.ChildNodes)
            {
                if(xmlChildNode.Name == "#comment")
                {
                    continue;
                }

                if(xmlChildNode.Name == "required")
                {
                    continue;
                }

                if (xmlChildNode.Name == "url")
                {
                    nodes[xmlChildNode.Name] = new Dictionary<string, string>
                    {
                        { "value", xmlChildNode.InnerText }
                    };
                    //nodes[xmlChildNode.Name]["value"] = xmlChildNode.InnerText;
                    continue;
                }

                var list = AttributesCheck(xmlChildNode);

                foreach (var attribute in list)
                {
                    if (nodes.ContainsKey(xmlChildNode.Name) == false)
                    {
                        nodes[xmlChildNode.Name] = new Dictionary<string, string>
                        {
                            { attribute.Name, attribute.Value }
                        };
                    }
                    else
                    {
                        nodes[xmlChildNode.Name][attribute.Name] = attribute.Value;
                    }
                }
            }

            Soap soap = new Soap(_sessionType, _operationId, NotFound);
            RequestPassedFimi = false;
            var savedParams = soap.RequestBuilder(nodes, xmlNode.Name, ref _requestPassedFimi);

            foreach(var item in savedParams)
            {
                _savedParams[item.Key] = item.Value;
            }
        }

        private List<Attribute> AttributesCheck(XmlNode xmlNode)
        {
            var attributes = xmlNode.Attributes;
            var attributeExist = 0;
            var placeExist = 0;
            var outputExist = 0;
            var fromExist = 0;
            var inputExist = 0;

            var r = new List<Attribute>();

            for (int i = 0; i < attributes?.Count; ++i)
            {
                string name = attributes[i].Name;
                string value = attributes[i].Value;

                if (name == "attribute")
                {
                    if (value != "true" && value != "false")
                    {
                        throw new ShukrMoliyaException("Incorrect attribute value 'attribute'");
                    }
                    r.Add(new Attribute(name, value));
                    attributeExist += 1;
                }
                else if (name == "place")
                {
                    if (value.Length == 0)
                    {
                        throw new ShukrMoliyaException("Incorrect attribute value 'place'");
                    }
                    r.Add(new Attribute(name, value));
                    placeExist += 1;
                }
                else if (name == "output")
                {
                    if (value.Length > 0)
                    {
                        throw new ShukrMoliyaException("Incorrect attribute value 'output'");
                    }
                    r.Add(new Attribute(name, value));
                    outputExist += 1;
                }
                else if (name == "from")
                {
                    if (value.Length == 0)
                    {
                        throw new ShukrMoliyaException("Incorrect attribute value 'from'");
                    }
                    r.Add(new Attribute(name, value));
                    fromExist += 1;
                }
                else if(name == "input")
                {
                    if(value.Length > 0)
                    {
                        throw new ShukrMoliyaException("Incorrect attribute value 'input'");
                    }
                    inputExist += 1;
                }
                else
                {
                    throw new ShukrMoliyaException("Incorrect attribute name or value");
                }
            }

            if (attributeExist > 1 || placeExist > 1 || outputExist > 1 || fromExist > 1 || inputExist > 1)
            {
                throw new ShukrMoliyaException("Incorrect count of attributes");
            }

            if (attributeExist == 1 && placeExist == 1 && outputExist == 0 && fromExist == 0)
            {
                if(inputExist == 1)
                {
                    r.Add(new Attribute("value", _savedParams[xmlNode.Name]));
                }
                else if (inputExist == 0)
                {
                    r.Add(new Attribute("value", xmlNode.InnerText));
                }
                return r;
            }

            if (outputExist == 1 && fromExist == 1 && attributeExist == 1 && placeExist == 0 && inputExist == 0)
            {
                return r;
            }

            throw new ShukrMoliyaException($"Incorrect attributes in '{xmlNode.Name}' node");
        }
    }

    struct Attribute
    {
        public Attribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }

    public enum SessionType : int
    {
        Unknown,
        POSRequestRq,
        ReverseTransactionRq,
        Unnecessary,
    }
}
