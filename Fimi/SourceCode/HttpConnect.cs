using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace Fimi
{
    public class HttpConnect
    {
        private readonly LogManager _log;
        private readonly string _connection;

        public string? Response { get; set; }
        public bool RequestPassed { get; set; }
        
        public HttpConnect(string url)
        {
            _connection = url;
            Response = null;
            RequestPassed = false;
            _log = new LogManager(Utils.LogPath);
        }

        public void SendRequest(string body, string operationId)
        {
            byte[] data = Encoding.UTF8.GetBytes(body);
            body = PrettyXml(body);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_connection);
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.ContentLength = data.Length;

            _log.Info($"Request {operationId}: \n\n{body}\n");

            using Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);

            try
            {
                using HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if(response.StatusCode != HttpStatusCode.OK)
                {
                    throw CreateWebException(response);
                }
                
                using StreamReader streamReader = new StreamReader(response.GetResponseStream());
                Response = PrettyXml(streamReader.ReadToEnd());
                _log.Info($"Response {operationId}: \n\nStatuscode={Convert.ToInt32(response?.StatusCode)} \n{Response}");
                RequestPassed = true;
            }
            catch(WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                if (ex.Response != null)
                {
                    using StreamReader streamReader = new StreamReader(ex.Response!.GetResponseStream());
                    var b = streamReader.ReadToEnd();
                    try
                    {
                        Response = PrettyXml(b);
                    }
                    catch
                    {
                        Response = b;
                    }
                }
                else
                {
                    Response = "Body empty";
                }
                _log.Error($"Response {operationId}: \n\nStatuscode={Convert.ToInt32(response?.StatusCode)}\n{ex.Message}\n{Response}\n");
            }
        }

        private string PrettyXml(string xml)
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

        private WebException CreateWebException(HttpWebResponse response)
        {
            return new WebException(
                response.StatusDescription,
                null,
                WebExceptionStatus.UnknownError,
                response);
        }
    }
}