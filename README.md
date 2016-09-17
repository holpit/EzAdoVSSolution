#ezAdo Overview

###By subscribing to a convention and utilizing the tools provided by the api, ezAdo allows a skilled team of developers to work in a way that plays to each members strengths.  Business objectives can be met faster, and without sacrificing performance, scalability, maintainability, or extensibility.

###The best way to illustrate is by example.  The following method calls a stored procedure named open.GET_ORDER.  The procedure returns JSON and has two out parameters.  For simplicity we are not processing the output parameters, just know they are there.

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
####For additional examples see the test project and SampleRest projects

#Ground rules

###There are several points that you need to concede up front. If the following is contrary to the way you work, the promise is that if subscribe to the the following, you get rewarded by eliminating alot of redundant code that is typically error prone and a pain in the ass to maintain.
1. Data access will now be done through the use of stored procedures.
2. Store procedures should be created in schemas that delineate functionality or security.
3. Each schema is mapped to a particular login, and subsequently a connections string identified by the schema.
4. Naming conventions must be followed.


##Things to know to shorten the learning curve.
The SampleDB database contains three schema's in addition to the defaults.  Each of these schema's map to a login, and ultimately a connection string in app.config or web.config.
####ezAdo
contains the procedure that queries system objects to return parameter definitions
contains the procedure that queries the user defined tables
contains the user defined type for schema names
####open
contains stored procedures and types that execute via the open login
####trusted
contains stored procedures and types that execute via the trusted login









##Prerequisits
Recommend SQL 2016 Devleloper edition - although the local db will suffice
If using developer edition create the target db SampleDB

* Set the properties for the SampleDB project
* Run the DB Project
* Run the tests
* View the controllers

