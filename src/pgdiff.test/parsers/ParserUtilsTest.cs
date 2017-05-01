
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pgdiff.parsers;
using pgdiff.schema;

namespace pgdiff.test
{
    [TestClass]
    public class ParserUtilsTest
    {

        [TestMethod]
        public void TestParseSchemaBothQuoted()
        {
            var database = new PgDatabase();
            var schema = new PgSchema("juzz_system");
            database.Schemas.Add(schema);

            Assert.AreEqual(ParserUtils.GetSchemaName("\"juzz_system\".\"f_obj_execute_node_select\"", database),"juzz_system");
        }

        [TestMethod]
        public void TestParseSchemaFirstQuoted()
        {
            var database = new PgDatabase();
            var schema = new PgSchema("juzz_system");
            database.Schemas.Add(schema);

            Assert.AreEqual(ParserUtils.GetSchemaName("\"juzz_system\".f_obj_execute_node_select", database),"juzz_system");
        }

        [TestMethod]
        public void TestParseSchemaSecondQuoted()
        {
            var database = new PgDatabase();
            var schema = new PgSchema("juzz_system");
            database.Schemas.Add(schema);

            Assert.AreEqual(ParserUtils.GetSchemaName("juzz_system.\"f_obj_execute_node_select\"", database),"juzz_system");
        }

        [TestMethod]
        public void TestParseSchemaNoneQuoted()
        {
            var database = new PgDatabase();
            var schema = new PgSchema("juzz_system");
            database.Schemas.Add(schema);

            Assert.AreEqual(ParserUtils.GetSchemaName("juzz_system.f_obj_execute_node_select", database),"juzz_system");
        }

        [TestMethod]
        public void TestParseSchemaThreeQuoted()
        {
            var database = new PgDatabase();
            var schema = new PgSchema("juzz_system");
            database.Schemas.Add(schema);

            Assert.AreEqual(ParserUtils.GetSchemaName("\"juzz_system\".\"f_obj_execute_node_select\".\"test\"",database),"juzz_system");
        }

        [TestMethod]
        public void TestParseObjectBothQuoted()
        {
            Assert.AreEqual(ParserUtils.GetObjectName("\"juzz_system\".\"f_obj_execute_node_select\""),"f_obj_execute_node_select");
        }

        [TestMethod]
        public void TestParseObjectFirstQuoted()
        {
            Assert.AreEqual(ParserUtils.GetObjectName("\"juzz_system\".f_obj_execute_node_select"),"f_obj_execute_node_select");
        }

        [TestMethod]
        public void TestParseObjectSecondQuoted()
        {
            Assert.AreEqual(ParserUtils.GetObjectName("juzz_system.\"f_obj_execute_node_select\""),"f_obj_execute_node_select");
        }

        [TestMethod]
        public void TestParseObjectNoneQuoted()
        {
            Assert.AreEqual(ParserUtils.GetObjectName("juzz_system.f_obj_execute_node_select"),"f_obj_execute_node_select");
        }

        [TestMethod]
        public void TestParseObjectThreeQuoted()
        {
            Assert.AreEqual(ParserUtils.GetObjectName("\"juzz_system\".\"f_obj_execute_node_select\".\"test\""),"test");
        }
    }
}

