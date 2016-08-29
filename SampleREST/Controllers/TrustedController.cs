using EzAdo;
using EzAdo.Models;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SampleREST.Controllers
{
    public class TrustedController : ApiController
    {
        private HttpResponseMessage ProcessProcedureResult(string json, Procedure proc)
        {
            int returnValue = proc.ReturnValue<int>();
            HttpResponseMessage result = new HttpResponseMessage((HttpStatusCode)returnValue);
            if(returnValue == 200)
            {
                if(json != null)
                {
                    result.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
            }
            else
            {
                result.Content = new StringContent(proc.GetValue<string>("@MESSAGE_RESULT"), Encoding.UTF8, "application/json");
            }

            return result;
        }

        [HttpPost]
        [Route("api/trusted/{specificName}")]
        public async Task<HttpResponseMessage> Post(string specificName)
        {
            string requestJson = await Request.Content.ReadAsStringAsync();
            Procedure proc = ProcedureFactory.GetRestProcedure("POST", "trusted", specificName);
            proc.LoadFromJson(requestJson);
            string json = proc.ExecuteJson();
            return ProcessProcedureResult(json, proc);
        }
    }
}
