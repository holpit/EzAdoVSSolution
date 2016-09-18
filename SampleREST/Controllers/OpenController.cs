using EzAdo;
using EzAdo.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;
using System.Text;

namespace SampleREST.Controllers
{
 /*
    substitute the {personId} with a valid value
    substitute the port 
    //Insert a person
    POST
    http://localhost:58594/api/open/person
    {"firstName": "alan", "lastName": "hyneman", "email": “alan.h.hyneman@gmail.com“}

    //Get a person
    GET
    http://localhost:58594/api/open/person?personId={personId}

    //update a person *note the invalid email* will throw an exception because of the entry in EZ_ADO_VALIDATORS
    PUT
    http://localhost:58594/api/open/person
    {"personId": 7, "firstName": "Alan", "lastName": "Hyneman", "email": “alan.h.s@df.com"}


    //Delete a person
    DELETE
    http://localhost:58594/api/open/person
    {"personId": 7}
*/


    /// <summary>Sample for the open schema</summary>
    public class OpenController : ApiController
    {

        private string _specificSchema = "open";

        /// <summary>
        /// Executes the open.GET_{procedure_name} specified in the request parameter with any addition parameters supplied in the query string.</summary>
        /// <param name="specificName">The stored procedure name in camelCase format</param>
        [HttpGet]
        [Route("api/open/{specificName}")]
        public HttpResponseMessage Get(string specificName)
        {
            Procedure proc = ProcedureFactory.GetRestProcedure("GET", _specificSchema, specificName);
            proc.LoadFromQuery(Request.GetQueryNameValuePairs());
            string Json = proc.ExecuteJson();
            return ProcessProcedureResult(Json, proc);
        }


        /// <summary>Executes the open.POST_{procedure_name} specified in the request parameter with any addition parameters supplied in the request content.</summary>
        /// <param name="specificName">The stored procedure name in camelCase format</param>
        [HttpPost]
        [Route("api/open/{specificName}")]
        public async Task<HttpResponseMessage> Post(string specificName)
        {
            string requestJson = await Request.Content.ReadAsStringAsync();
            Procedure proc = ProcedureFactory.GetRestProcedure("POST", _specificSchema, specificName);
            proc.LoadFromJson(requestJson);
            string Json = proc.ExecuteJson();
            return ProcessProcedureResult(Json, proc);
        }

        /// <summary>Executes the open.PUT_{procedure_name} specified in the request parameter with any addition parameters supplied in the request content.</summary>
        /// <param name="specificName">The stored procedure name in camelCase format</param>
        [HttpPut]
        [Route("api/open/{specificName}")]
        public async Task<HttpResponseMessage> Put(string specificName)
        {
            string requestJson = await Request.Content.ReadAsStringAsync();
            Procedure proc = ProcedureFactory.GetRestProcedure("PUT", _specificSchema, specificName);
            proc.LoadFromJson(requestJson);
            string Json = proc.ExecuteJson();
            return ProcessProcedureResult(Json, proc);
        }

        
        [HttpDelete]
        [Route("api/open/{specificName}")]
        public async Task<HttpResponseMessage> Delete(string specificName)
        {
            string requestJson = await Request.Content.ReadAsStringAsync();
            Procedure proc = ProcedureFactory.GetRestProcedure("DELETE", _specificSchema, specificName);
            proc.LoadFromJson(requestJson);
            proc.ExecuteNonQuery();
            return ProcessProcedureResult(null, proc);
        }

        /// <summary>
        /// This is where any additional processing would occur for procedures executing in the open schema.  Optional an object could be created that derives from procedure that would handle the processing.</summary>
        /// <param name="Json"></param>
        /// <param name="proc"></param>
        /// <returns></returns>
        private HttpResponseMessage ProcessProcedureResult(string Json, Procedure proc)
        {
            int returnValue = proc.ReturnValue<int>();
            HttpResponseMessage result = new HttpResponseMessage((HttpStatusCode)returnValue);
            if (returnValue == 200)
            {
                if (Json != null)
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
    }
}
