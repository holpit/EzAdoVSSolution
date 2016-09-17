# ezAdo Overview

##By subscribing to a convention and utilizing the tools provided by the api, ezAdo allows a skilled team of developers to work in a way that plays to each members strengths.  Business objectives can be met faster, and without sacrificing performance, scalability, maintainability, or extensibility.

##The best way to illustrate is by example.  The following method calls a stored procedure named open.GET_ORDER.  The procedure returns JSON and has two out parameters.  For simplicity we are not processing the output parameters, just know they are there.

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
            cmd.Parameters["@SQL_ERROR_ID"].Direction = ParameterDirection.Output;
            cmd.Parameters["@MESSAGE_RESULT"].Direction = ParameterDirection.Output;
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
            if(cmd.Parameters["@SQL_ERROR_ID"].Value != DBNull.Value)
            {
                sqlErrorId = (int)cmd.Parameters["@SQL_ERROR_ID"].Value;
            }
            if (cmd.Parameters["@MESSAGE_RESULT"].Value != DBNull.Value)
            {
                messageResult = (string)cmd.Parameters["@MESSAGE_RESULT"].Value;
            }
            cnn.Close();
        }
    }
    return bldr.ToString();
}
private string WithEz()
{
    Procedure proc = ProcedureFactory.GetProcedure("open", "GET_ORDER");
    proc["orderId"] = 1;
    string json = proc.ExecuteJson();
    var sqlErrorId = proc.GetValue<int>("sqlErrorId");
    var messageResult = proc.GetValue<string>("messageResult");
    return json;
}```

