using Microsoft.VisualStudio.TestTools.UnitTesting;
using EzAdo;
using EzAdo.Models;
using System.Collections.Generic;

namespace Test
{
    /// <summary>Examples and tests illustrating the execution of procedures *Note there will always be a lag time associated with the first call while the factory builds out the procedures.  Make sure to clear old data prior to running the tests. </summary>
    [TestClass]
    public class SampleTests
    {
        /// <summary>Clears the current data</summary>
        [TestMethod]
        public void ClearData()
        {
            Procedure procedure = ProcedureFactory.GetProcedure("open", "CLEAR_DATA");
            procedure.ExecuteNonQuery();
            Assert.AreEqual(procedure.ReturnValue<int>(), 200);
        }

        private class Person
        {
            public int PersonId { set; get; }
            public string LastName { set; get; }
            public string FirstName { set; get; }
            public string Email { set; get; }
        }
        private class Order
        {
            public int OrderId { set; get; }
            public string OrderName { set; get; }
            List<OrderItem> OrderItems { set; get; }
        }
        private class OrderItem
        {
            public int OrderItemId { set; get; }
            public string Name { set; get; }
            public string Comments { set; get; }
        }
        

        /// <summary>Load from object populates the parameters using the properties of the object passed.  ExecuteJson&ltT&gt; executes a stored procedure annotated with *Returns Json* and deserializes the result into T </summary>
        [TestMethod]
        public void LoadFromObject1()
        {
            //Create the person
            Person person = new Person() { FirstName = "John", LastName = "Doe", Email = "JohnDoe@gmail.com" };

            //Get the procedure
            Procedure proc = ProcedureFactory.GetProcedure("open", "POST_PERSON");

            //Load the parameters from the object
            proc.LoadFromObject<Person>(person);

            //Execute the result - procedures returns the newly created object
            Person result = proc.ExecuteJson<Person>();

            Assert.AreNotEqual(result.PersonId, person.PersonId);
            Assert.AreEqual(result.FirstName, person.FirstName);
            Assert.AreEqual(result.LastName, person.LastName);
            Assert.AreEqual(result.Email, person.Email);
            Assert.AreEqual(proc.ReturnValue<int>(), 200);
            Assert.AreEqual(proc.GetValue<long>("sqlErrorId"), 0);
            Assert.AreEqual(proc.GetValue<string>("messageResult"), null);

        }

        /// <summary>Same as LoadFromObject1 however the mapping strategy for person is now cached so the execution time will see a slight improvement</summary>
        [TestMethod]
        public void LoadFromObject2()
        {
            Person person = new Person() { FirstName = "Jack", LastName = "Be", Email = "JackBeNimble@gmail.com" };
            Procedure proc = ProcedureFactory.GetProcedure("open", "POST_PERSON");
            proc.LoadFromObject<Person>(person);
            Person result = proc.ExecuteJson<Person>();
            Assert.AreNotEqual(result.PersonId, person.PersonId);
            Assert.AreEqual(result.FirstName, person.FirstName);
            Assert.AreEqual(result.LastName, person.LastName);
            Assert.AreEqual(result.Email, person.Email);
            Assert.AreEqual(proc.ReturnValue<int>(), 200);
            Assert.AreEqual(proc.GetValue<long>("sqlErrorId"), 0);
            Assert.AreEqual(proc.GetValue<string>("messageResult"), null);

        }

        /// <summary>Load from parameters sets the parameters directly using the procedure indexer - the conversion from object to the mapped C# type defined by SqlDbType is tested.</summary>
        [TestMethod]
        public void LoadFromParameters1()
        {
            Person person = new Person() { FirstName = "Bill", LastName = "Buzz", Email = "BillBuzz@gmail.com" };
            Procedure proc = ProcedureFactory.GetProcedure("open", "POST_PERSON");
            proc["firstName"] = person.FirstName;
            proc["lastName"] = person.LastName;
            proc["email"] = person.Email;

            //returning person
            Person result = proc.ExecuteJson<Person>();

            Assert.AreNotEqual(result.PersonId, person.PersonId);
            Assert.AreEqual(result.FirstName, person.FirstName);
            Assert.AreEqual(result.LastName, person.LastName);
            Assert.AreEqual(result.Email, person.Email);
            Assert.AreEqual(proc.ReturnValue<int>(), 200);
            Assert.AreEqual(proc.GetValue<long>("sqlErrorId"), 0);
            Assert.AreEqual(proc.GetValue<string>("messageResult"), null);

        }

        /// <summary>Same as Load from parameters 1 only using the generic setters, this provides a slightly more robust method of setting the parameters as the parameter expects the type to be the mapped c# type defined by the SqlDbType</summary>
        [TestMethod]
        public void LoadFromParameters2()
        {
            //Generic setters provide additional validation
            
            Procedure proc = ProcedureFactory.GetProcedure("open", "POST_PERSON");
            proc.SetValue<string>("firstName", "Mike");
            proc.SetValue<string>("lastName", "Quick");
            proc.SetValue<string>("email", "MikeQuick@me.com");

            //returning json
            string result = proc.ExecuteJson();
            Assert.AreNotEqual(result, null);
            Assert.AreEqual(proc.ReturnValue<int>(), 200);
            Assert.AreEqual(proc.GetValue<long>("sqlErrorId"), 0);
            Assert.AreEqual(proc.GetValue<string>("messageResult"), null);

        }

        /// <summary>Post order populates the parameter collection from the json string provided - not this procedure uses a user defined table type of the order item collection.</summary>
        [TestMethod]
        public void PostOrder1()
        {
            string Json = "{'personId': 1, 'orderName': 'my first order', 'orderItems': [{'name': 'pizza', 'comments': 'Extra Cheese Please'},{'name': 'italian sandwich', 'comments': 'No peppers'}]}";
            Procedure procedure = ProcedureFactory.GetRestProcedure("POST", "trusted", "ORDER");
            procedure.LoadFromJson(Json);
            string result = procedure.ExecuteJson();
        }
        

        /// <summary>Just illustrating return values</summary>
        [TestMethod]
        public void PostPersonDuplicates()
        {
            //Generic setters provide additional validation

            Procedure procGood = ProcedureFactory.GetProcedure("open", "POST_PERSON");
            procGood.SetValue<string>("firstName", "a");
            procGood.SetValue<string>("lastName", "a");
            procGood.SetValue<string>("email", "a@a.com");
            procGood.ExecuteJson<Person>();

            Procedure procBad = ProcedureFactory.GetProcedure("open", "POST_PERSON");
            procBad.SetValue<string>("firstName", "a");
            procBad.SetValue<string>("lastName", "a");
            procBad.SetValue<string>("email", "a@a.com");
            procBad.ExecuteJson<Person>();

            Assert.AreEqual(procGood.GetValue<int>("RETURN_VALUE"), 200);
            Assert.AreEqual(procGood.GetValue<long>("SQL_ERROR_ID"), 0);
            Assert.AreEqual(procGood.GetValue<string>("MESSAGE_RESULT"), null);

            Assert.AreEqual(procBad.GetValue<int>("RETURN_VALUE"), 409);
            Assert.AreEqual(procBad.GetValue<long>("SQL_ERROR_ID"), 0);
            Assert.AreNotEqual(procBad.GetValue<string>("MESSAGE_RESULT"), null);

        }

        /// <summary>Illustrates a procedure that executes a result set and returns an object</summary>
        [TestMethod]
        public void GetPersonObject()
        {
            Procedure procedure = ProcedureFactory.GetProcedure("open", "PERSON_BY_ID");
            procedure.SetValue<int>("PERSON_ID", 1);
            Person person = procedure.ExecuteReader<Person>();

            Assert.AreEqual(person.PersonId, 1);
            Assert.AreEqual(procedure.GetValue<long>("SQL_ERROR_ID"), 0);
            Assert.AreEqual(procedure.GetValue<string>("MESSAGE_RESULT"), null);

        }

        /// <summary>Illustrates a procedure that executes a result set and returns an object serialized to json</summary>
        [TestMethod]
        public void GetPersonObjectJson1()
        {
            Procedure procedure = ProcedureFactory.GetProcedure("open", "PERSON_BY_ID");
            procedure.SetValue<int>("PERSON_ID", 1);
            string result = procedure.ExecuteReaderAsJson<Person>();
            Assert.AreNotEqual(result, null);
            Assert.AreEqual(procedure.GetValue<long>("SQL_ERROR_ID"), 0);
            Assert.AreEqual(procedure.GetValue<string>("MESSAGE_RESULT"), null);
        }

        /// <summary>Same as 1 but should show a slight bump in performance</summary>
        [TestMethod]
        public void GetPersonObjectJson2()
        {
            Procedure procedure = ProcedureFactory.GetProcedure("open", "PERSON_BY_ID");
            procedure.SetValue<int>("PERSON_ID", 1);
            string result = procedure.ExecuteReaderAsJson<Person>();
            Assert.AreNotEqual(result, null);
            Assert.AreEqual(procedure.GetValue<long>("SQL_ERROR_ID"), 0);
            Assert.AreEqual(procedure.GetValue<string>("MESSAGE_RESULT"), null);
        }


    }
}