using Fimi.Db;
using Fimi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fimi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FimiController : ControllerBase
    {
        private readonly DbContext _db;
        private readonly LogManager _log;
        private readonly int _operationTypeId;
        private readonly string _routeName;
        private readonly HashSet<string> _routeNames;

        public FimiController(IHttpContextAccessor httpContextAccessor)
        {
            _db = new DbContext();
            _log = new LogManager(Utils.LogPath);

            _routeNames = _db.GetRouteNames();

            var path = httpContextAccessor.HttpContext!.Request.Path!.Value!;
            _routeName = path.Substring(path.LastIndexOf('/') + 1, path.Length - path.LastIndexOf('/') - 1);

            if(!_routeNames.Contains(_routeName))
            {
                throw new ShukrMoliyaException($"Database does not contain `{_routeName}`. Check your controller's action for correctness");
            }

            _operationTypeId = _db.GetOperationTypeId(_routeName);
        }

        [HttpPost]
        public ActionResult OperationCreate([FromBody] OperationCreateRequest request)
        {
            int operationId = _db.OperationCreate(request.PlatformId, request.Amount, request.OperationTypeId, request.ExternalId);
            
            return Ok(new OperationCreateResponse()
            {
                OperationId = operationId,
                ResultCode = 0,
                ResultDesc = "Success"
            });
        }

        [HttpPost]
        public ActionResult P2P([FromBody] P2PRequest request)
        {
            var r = new Dictionary<string, string>()
            {
                { "PAN", request.PAN },
                { "PAN2", request.PAN2 },
                { "Amount", request.Amount.ToString().Replace(",", ".") },
                { "ExternalId", request.ExternalId },
                { "OperationId", request.OperationId.ToString() }
            };

            var d = FimiHandler.Exec(
                request.PlatformId,
                request.OperationId,
                request.ExternalId,
                request.Amount,
                _operationTypeId,
                _routeName,
                _db,
                r,
                out ErrorResponse? errorResponse,
                _log);

            if (d is null && errorResponse is null)
            {
                throw new ShukrMoliyaException("Unknown error in 'P2P' action");
            }

            if(d is not null)
            {
                var response = new P2PResponse()
                {
                    Response = d["Response"],
                    AvailBalance = Convert.ToDouble(d["AvailBalance"].Replace(".", ",")),
                    AuthRespCode = d["AuthRespCode"],
                    ResultCode = 0,
                    ResultDesc = "Fimi request passed success"
                };

                return Ok(response);
            }

            return Ok(errorResponse);
        }

        [HttpPost]
        public ActionResult CardVerification([FromBody] CardVerificationRequest request)
        {
            var r = new Dictionary<string, string>()
            {
                { "PAN", request.PAN},
                { "ExpDate", request.ExpDate },
                { "CVV2", request.CVV2 },
            };

            var d = FimiHandler.Exec(
                request.PlatformId,
                request.OperationId,
                request.ExternalId,
                null,
                _operationTypeId,
                _routeName,
                _db,
                r,
                out ErrorResponse? errorResponse,
                _log);

            if(d is null && errorResponse is null)
            {
                throw new ShukrMoliyaException("Unknown error in 'CardVerification' action");
            }

            if(d is not null)
            {
                var response = new CardVerificationResponse()
                {
                    Response = d["Response"],
                    ApprovalCode = d["ApprovalCode"],
                    AvailBalance = d["AvailBalance"],
                    AuthRespCode = d["AuthRespCode"],
                    ResultCode = 0,
                    ResultDesc = "Fimi request passed success"
                };

                return Ok(response);
            }

            return Ok(errorResponse);
        }

        [HttpPost]
        public ActionResult GetCardInfoRq([FromBody] GetCardInfoRequest request)
        {
            var r = new Dictionary<string, string>()
            {
                { "PAN", request.PAN }
            };

            var d = FimiHandler.Exec(
                request.PlatformId,
                request.OperationId,
                request.ExternalId,
                null,
                _operationTypeId,
                _routeName,
                _db,
                r,
                out ErrorResponse? errorResponse,
                _log);

            if (d is null && errorResponse is null)
            {
                throw new ShukrMoliyaException("Unknown error in 'GetCardInfoRq' action");
            }

            if(d is not null)
            {
                var response = new CardInfoResponse()
                {
                    //MBR = Convert.ToInt32(d["MBR"]),
                    //NameOnCard = d["NameOnCard"],
                    //InstName = d["InstName"],
                    //Address = d["Address"],
                    //LedgerBalance = Convert.ToDouble(d["LedgerBalance"].Replace(".", ",")),
                    ResultCode = 0,
                    ResultDesc = "Fimi request passed success"
                };

                return Ok(response);
            }

            return Ok(errorResponse);
        }

        [HttpPost]
        public ActionResult ReverseTransactionRq([FromBody] ReverseTransactionRequest request)
        {
            var r = new Dictionary<string, string>()
            {
                { "TranId", request.TranId }
            };

            var d = FimiHandler.Exec(
                request.PlatformId,
                request.OperationId,
                request.ExternalId,
                null,
                _operationTypeId,
                _routeName,
                _db,
                r,
                out ErrorResponse? errorResponse,
                _log);

            if (d is null && errorResponse is null)
            {
                throw new ShukrMoliyaException("Unknown error in 'ReverseTransactionRq' action");
            }

            if(d is not null)
            {
                var response = new ReverseTransactionResponse()
                {
                    Response = d["Response"],
                    ThisTranId = d["ThisTranId"],
                    ResultCode = 0,
                    ResultDesc = "Fimi request passed success"
                };

                return Ok(response);
            }

            return Ok(errorResponse);
        }

        [HttpPost]
        public ActionResult POSRequestRq([FromBody] POSRequestRqRequest request)
        {
            var transactionNumber = _db.GetTransactionNumber(request.OperationId);

            var r = new Dictionary<string, string>
            {
                { "PAN", request.PAN },
                { "Amount", request.Amount.ToString().Replace(",", ".") },
                { "CVV2", request.CVV2 },
                { "TransactionNumber", transactionNumber.ToString() },
                { "ExpDate", request.ExpDate }
            };

            var d = FimiHandler.Exec(
                request.PlatformId,
                request.OperationId,
                request.ExternalId,
                request.Amount,
                _operationTypeId,
                _routeName,
                _db,
                r,
                out ErrorResponse? errorResponse,
                _log);

            if (d is null && errorResponse is null)
            {
                throw new ShukrMoliyaException("Unknown error in 'POSRequestRq' action");
            }

            if(d is not null)
            {
                var response = new POSRequestRqResponse()
                {
                    ApprovalCode = d["ApprovalCode"],
                    AuthRespCode = Convert.ToInt32(d["AuthRespCode"]),
                    Response = Convert.ToInt32(d["Response"]),
                    ThisTranId = Convert.ToInt32(d["ThisTranId"]),
                    ResultCode = 0,
                    ResultDesc = "Fimi request passed success"
                };

                return Ok(response);
            }

            return Ok(errorResponse);
        }
    }
}
