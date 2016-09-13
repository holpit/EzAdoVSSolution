using EzAdo;
using EzAdo.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Threading;
using System.Web.Http.Controllers;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace SampleREST.Controllers
{
    public class OpenController : ApiController
    {

        private string _specificSchema = "open";

        [HttpGet]
        [Route("api/open/Admin/Rebuild")]
        public HttpResponseMessage Rebuild()
        {
            var request = this;

            ProcedureFactory.Rebuild();
            Procedure proc = ProcedureFactory.GetRestProcedure("PUT", _specificSchema, "PERSON");
            string json = JsonConvert.SerializeObject(proc);

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

            result.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return result;
        }


        [HttpGet]
        [Route("api/open/{specificName}")]
        public HttpResponseMessage Get(string specificName)
        {
            Procedure proc = ProcedureFactory.GetRestProcedure("GET", _specificSchema, specificName);
            proc.LoadFromQuery(Request.GetQueryNameValuePairs());
            string json = proc.ExecuteJson();
            return ProcessProcedureResult(json, proc);
        }

        [HttpPost]
        [Route("api/open/{specificName}")]
        public async Task<HttpResponseMessage> Post(string specificName)
        {
            string requestJson = await Request.Content.ReadAsStringAsync();
            Procedure proc = ProcedureFactory.GetRestProcedure("POST", _specificSchema, specificName);
            proc.LoadFromJson(requestJson);
            string json = proc.ExecuteJson();
            return ProcessProcedureResult(json, proc);
        }

        [HttpPut]
        [Route("api/open/{specificName}")]
        public async Task<HttpResponseMessage> Put(string specificName)
        {
            string requestJson = await Request.Content.ReadAsStringAsync();
            Procedure proc = ProcedureFactory.GetRestProcedure("PUT", _specificSchema, specificName);
            proc.LoadFromJson(requestJson);
            string json = proc.ExecuteJson();
            return ProcessProcedureResult(json, proc);
        }

        [HttpDelete]
        [Route("api/open/{specificName}")]
        public async Task<HttpResponseMessage> Delete(string specificName)
        {
            string requestJson = await Request.Content.ReadAsStringAsync();
            Procedure proc = ProcedureFactory.GetRestProcedure("DELETE", _specificSchema, specificName);
            proc.LoadFromJson(requestJson);
            string json = proc.ExecuteJson();
            return ProcessProcedureResult(json, proc);

        }

        private HttpResponseMessage ProcessProcedureResult(string json, Procedure proc)
        {
            int returnValue = proc.ReturnValue<int>();
            HttpResponseMessage result = new HttpResponseMessage((HttpStatusCode)returnValue);
            if (returnValue == 200)
            {
                if (json != null)
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

    }
}
