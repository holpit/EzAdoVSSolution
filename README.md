#ezAdo Overview

 By subscribing to a convention and utilizing the tools provided by the api, ezAdo allows a skilled team of developers to work in a way that plays to each members strengths.  Business objectives can be met faster, and without sacrificing performance, scalability, maintainability, or extensibility.

 The best way to illustrate is by example.  The following method calls a stored procedure named open.GET_ORDER.  The procedure returns JSON and has two out parameters.  For simplicity we are not processing the output parameters, just know they are there.

```C#
private string WithoutEz()
{
    int sqlErrorId = 0;
    string messageResult = null;
    StringBuilder bldr = new StringBuilder();

    using (SqlConnection cnn = new SqlConnection("your connection string"))
    {
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = cnn;
            cmd.CommandText = "[open].[GET_ORDER]";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            //defining these parameters is alway prone to errors 
            cmd.Parameters.Add("@ORDER_ID", SqlDbType.Int, 8).Value = 1;
            cmd.Parameters.Add("@SQL_ERROR_ID", SqlDbType.Int);
            cmd.Parameters.Add("@MESSAGE_RESULT", SqlDbType.VarChar, 256);
            cnn.Open();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                while(rdr.Read())
                {
                    //larger json results will return as a single column reader
                    bldr.Append(rdr.GetString(0));
                }
                rdr.Close();
            }
            cnn.Close();
        }
    }
    return bldr.ToString();
}
private string WithEz()
{
    Procedure proc = ProcedureFactory.GetProcedure("open", "GET_ORDER");
    proc.SetValue<int>("orderId", 1);
    return proc.ExecuteJson();
}
```
######For additional examples see the test project and SampleRest projects


#Ground rules
There are several points you need to concede up front. If the following is contrary to the way you work, the promise is that if you subscribe to the the following, you are rewarded with the elimination of alot of redundant code, that is typically error prone and a pain in the ass to maintain.

1. Data access will now be done through the use of stored procedures.
2. Store procedures should be created in schemas that delineate functionality or security.
3. Each schema is mapped to a particular login, and subsequently a connections string identified by the schema.
4. Naming conventions must be followed.


#Things to know to shorten the learning curve.

###Schemas
 Schemas are a very important part of the design and the samples include the following:
 
1. ezAdo.
  * contains the procedure that queries system objects to return parameter definitions.
  * contains the procedure that queries the user defined tables. 
  * contains the user defined type for schema names. 
2. open.
  * contains stored procedures and types that execute via the open login. 
3. trusted.
  * contains stored procedures and types that execute via the trusted login. 

###Procedure annotations
####Procedure annotations provide a way for the creator of a procedure to give clues to the ado calling procedure.  This provides developers immediate feedback when a call is made in a way that doesn't match the intended use.
Annotations include:
  * \*Returns Json\* - the procedure return json via FOR JSON PATH
  * \*Single Result\* - the result of the call is a single entity - not enumerable
  * \*Always Encrypted\* the procedure contains always encrypted columns
  * \*Non Query\* - procedure returns no results

###Pocedure Naming Conventions
#####Procedures that are created for the purpose of providing data from web api should be prefixed with the Http Method Type.  In addition those procedures should return the Http Status and can optionally log errors and return custom messages.  The RestSample project illustrates some of the conepts.
  * POST - schema.POST_ENTITY - sample expects json post
  * PUT - schema.PUT_ENTITY - sample expects json post
  * DELETE - schema.DELETE_ENTITY - sample expects json post
  * GET - schema.GET_ENTITY - sample expects query string parameters

###Other procedure syntax
#####In addition to annotations procedures should adhere to the following convention:
  * IF @PARAMETER IS NULL THROW... Parameter is now non-nullable
  * IF NOT EXISTS (SELECT 1 FROM @PARAMETER_TABLE) THROW... Parameter is now non-nullable 

###EZ_ADO_VALIDATORS
##### The table EZ_ADO_VALIDATORS provides the opportunity to expand the validation in the api with numeric range, and regular expressions and applies to a single parameter identified by schema, procedure name, and paramter name.  The following additional validation is possible:
* Regular expressions - Add to the REST_SQL_VALIDATORS table
* Numeric Minimum - Add to the REST_SQL_VALIDATORS table
* Numeric Maximum - Add to the REST_SQL_VALIDATORS table

#Getting started
#####Prerequisites
 * Recommend SQL 2016 Devleloper edition
 * Visual Studio 2015 Community Edition or Better
 * SQL Server Data Tools for Visual Studio
 * About a half out of your time

##Steps
1. Download the zip
2. Extract and open the solution in Visual Studio
3. If using Sql Developer Create a Database called SampleDB.
4. In the properties of the SampleDb project set the Debug Target Connection String to your DB.
5. Set the SampleDB Project as startup and run the project.
6. Verify that the logins, schemas, tables, and procedures have been creted.
7. Set the connection strings in the app.config of the test project
8. Set the connections string in the web.config of SampleRest project.
9. Run the tests and explore the code
10. Fire up postman and hit the endpoints




