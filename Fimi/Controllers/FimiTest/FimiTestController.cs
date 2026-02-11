using Microsoft.AspNetCore.Mvc;

namespace Fimi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FimiTestController : ControllerBase
    {
        private readonly string _password;

        public FimiTestController()
        {
            _password = "Qx*Ts^Mitjt4JWFTfM4ABRRGwOgzeqQc*!9nuB088jb7N7wk*5y*jivQwW3NkyuP";
        }
    }
}
