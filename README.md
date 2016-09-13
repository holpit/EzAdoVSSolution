# EzAdo

## What is it?
EzAdo Objects + Naming Convention + Procedure \*annotations\* + Validators = a very simplified but robust and secure way of interfacing with Sql Server. It is an attempt to remove as much of the mundane code of setting parameters, executing procedures, processing results, and ulimately returning results to client applications.

##Who is it for?
If your organization relies heavily on stored procedures this is for you.  If your developing a lot of rest services that primarly interface with Sql Server this is for you. If you are completely satisfied with a tool like EntityFramework you may want to bail, as this is favors developers who lean towards SQL.  In fact if you are a DBA type person, you are gonna love this.

###Part 1 The Procedure.Factory
The procedure factory initializes by executing the stored procedures restsql.PROCEDURES and restsql.USER_DEFINED_TABLES.  The results of those procedures are utilized to pre-build the procedures and user defined table types that are exposed through the factory. When a call is made to retrieve a procedure, the factory locates the procedure form its internal cache, clones it and initializes it by preparing the Sqlcommand, its parameters, and wiring up the connection.  Calling the Factory Get Procedure returns a procedure in a ready to execute state.
In addtion to the procedures and types that are cached, when objects are populate from result sets, or procedure parameters are populated from objects, those mappings are cached in the factory and utlized in subsequent calls, thereby minimimizing the impact of reflection based setters and getters.  The first call to map an object pays a small penalty.

###Part 2 Set the values for the Procedure
Once the procedure is returned from the factory, it is ready to have the parameter values set.  There are several methods available including directly setting parameters, loading the parameters from a query string, loading the parameters from json, or loading parameters from an object.

###Part 3 Execute the Procedure
Afer the values of the procedure are set the procedure is ready to execute.  Prior to execution the procedure  will iterate the parameters, validating against nulls, character length, numeric minimum and maximum values, and regular expressions. There are several methods including execute json, execute object, execute non query, execute response, where the execution type must align with the annotations of the stored procedure.

###Procedure annotations
* \*Returns Json\* - procedure returns json (SQL 2016) - forces ExecuteJson()
* \*Single Result\* - procedure returns a single row forces Execete\<T\> where T is not enumerable
* \*Always Encrypted\* will append the connction string and enable always on encrypeted columns  
* \*Non Query\* - procedure returns no results - (it can however have ouput parameters)
* Non-Nullable - created when a procedure inspects an input parameter for null via IF @PARAMETER IS NULL THROW
* Regular expressions - Add to the REST_SQL_VALIDATORS table
* Numeric Minimum - Add to the REST_SQL_VALIDATORS table
* Numeric Maximum - Add to the REST_SQL_VALIDATORS table

##Getting Started
Recommend SQL 2016 Devleloper edition - although the local db will suffice
If using developer edition create the target db SampleDB

* Set the properties for the SampleDB project
* Run the DB Project
* Run the tests
* View the controllers

To be continued...
