using EzAdo;
using EzAdo.Models;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SampleREST.Controllers
{
    /*
    POST
    {'personId': 1, 'orderName': 'my first order', 'orderItems': [{'name': 'pizza', 'comments': 'Extra Cheese Please'},{'name': 'Italian sandwich', 'comments': 'No peppers'}]}
    
    GET
    http://localhost:58594/api/trusted/ordersByPerson?personId=1
    */

    /// <summary>Executes procedures in the trusted schema</summary>
    public class TrustedController : ApiController
    {
        //Note this controller is locked down to the trusted schema
        private string _specificSchema = "trusted";

        private HttpResponseMessage ProcessProcedureResult(string Json, Procedure proc)
        {
            int returnValue = proc.ReturnValue<int>();
            HttpResponseMessage result = new HttpResponseMessage((HttpStatusCode)returnValue);
            if(returnValue == 200)
            {
                if(Json != null)
                {
                    result.Content = new StringContent(Json, Encoding.UTF8, "application/Json");
                }
            }
            else
            {
                result.Content = new StringContent(proc.GetValue<string>("@MESSAGE_RESULT"), Encoding.UTF8, "application/Json");
            }

            return result;
        }

        [HttpPost]
        [Route("api/trusted/{specificName}")]
        public async Task<HttpResponseMessage> Post(string specificName)
        {
            string requestJson = await Request.Content.ReadAsStringAsync();
            Procedure proc = ProcedureFactory.GetRestProcedure("POST", _specificSchema, specificName);
            proc.LoadFromJson(requestJson);
            string Json = proc.ExecuteJson();
            return ProcessProcedureResult(Json, proc);
        }

        /// <summary>
        /// Executes the open.GET_{procedure_name} specified in the request parameter with any addition parameters supplied in the query string.</summary>
        /// <param name="specificName">The stored procedure name in camelCase format</param>
        [HttpGet]
        [Route("api/trusted/{specificName}")]
        public HttpResponseMessage Get(string specificName)
        {
            Procedure proc = ProcedureFactory.GetRestProcedure("GET", _specificSchema, specificName);
            proc.LoadFromQuery(Request.GetQueryNameValuePairs());
            string Json = proc.ExecuteJson();
            return ProcessProcedureResult(Json, proc);
        }
    }
}
