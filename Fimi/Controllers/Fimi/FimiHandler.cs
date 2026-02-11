using Fimi.Models;
using Fimi.Db;

namespace Fimi.Controllers
{
    static class FimiHandler
    {
        public static Dictionary<string, string>? Exec(
            int platformId,
            int operationId,
            string? externalId,
            decimal? amount,
            int operationType,
            string soapRequestCommand,
            DbContext db,
            Dictionary<string, string> r,
            out ErrorResponse? errorResponse,
            LogManager log)
        {
            var xmlFimi = new XmlConfigurationFimi(Utils.GetConfig(), r, operationId.ToString());

            db.OperationModifyState(platformId, operationId, amount, externalId, operationType, DataBaseStates.Await);

            try
            {
                var d = xmlFimi.ExecCommand(soapRequestCommand);

                var dbState = CheckResponseFrom(soapRequestCommand, d, xmlFimi, operationId);

                db.OperationModifyState(platformId, operationId, amount, externalId, operationType, dbState);

                errorResponse = null;
                return d;
            }
            catch (Exception ex)
            {
                log.Error(ex);

                if (xmlFimi.RequestPassedFimi == false)
                {
                    db.OperationModifyState(platformId, operationId, amount, externalId, operationType, DataBaseStates.Cancel);
                    errorResponse = new ErrorResponse()
                    {
                        ResultCode = 2,
                        ResultDesc = "Fimi request failed"
                    };
                }
                else
                {
                    db.OperationModifyState(platformId, operationId, amount, externalId, operationType, DataBaseStates.Accept);
                    errorResponse = new ErrorResponse()
                    {
                        ResultCode = 1,
                        ResultDesc = "Fimi request passed successful but internal error in fimi module"
                    };
                }

                return null;
            }
        }

        private static DataBaseStates CheckResponseFrom(
            string soapRequestCommand,
            Dictionary<string, string> response,
            XmlConfigurationFimi xmlFimi,
            int operationId)
        {
            if (soapRequestCommand.ToLower() == "POSRequestRq".ToLower())
            {
                if(response.ContainsKey("AuthRespCode") == false)
                {
                    xmlFimi.RequestPassedFimi = false;
                    throw new ShukrMoliyaException("AuthRespCode is not saved. Check fimi config file for saving AuthRespCode from response");
                }

                if (response["AuthRespCode"] == XmlConfigurationFimi.NotFound)
                {
                    xmlFimi.RequestPassedFimi = false;
                    throw new ShukrMoliyaException("AuthRespCode does not exist from `POSRequestRq` response");
                }

                if (response["AuthRespCode"] != "1")
                {
                    xmlFimi.RequestPassedFimi = false;
                    throw new ShukrMoliyaException($"AuthRespCode from `POSRequestRq` response is not equal to 1. OperationId={operationId}");
                }
            }

            return DataBaseStates.Accept;
        }
    }
}
