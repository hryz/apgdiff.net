using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace pgdiff.test
{

    [TestClass]
    public class PgDiffTest
    {
        // Tests scenario where COLUMN type is modified. 
        [TestMethod] public void modify_column_type() => Test("modify_column_type", false, false, false, false);
        // Tests scenario where CLUSTER is added to TABLE. 
        [TestMethod] public void add_cluster() => Test("add_cluster", false, false, false, false);
        // Tests scenario where CLUSTER is dropped from TABLE. 
        [TestMethod] public void drop_cluster() => Test("drop_cluster", false, false, false, false);
        // Tests scenario where CLUSTER is changed on TABLE. 
        [TestMethod] public void modify_cluster() => Test("modify_cluster", false, false, false, false);
        // Tests scenario where WITH OIDS is dropped from TABLE. 
        [TestMethod] public void drop_with_oids() => Test("drop_with_oids", false, false, false, false);
        // Tests scenario where INDEX is added. 
        [TestMethod] public void add_index() => Test("add_index", false, false, false, false);
        // Tests scenario where INDEX is dropped. 
        [TestMethod] public void drop_index() => Test("drop_index", false, false, false, false);
        // Tests scenario where INDEX that TABLE CLUSTER is based  on is dropped. 
        [TestMethod] public void drop_index_with_cluster() => Test("drop_index_with_cluster", false, false, false, false);
        // Tests scenario where INDEX definition is modified. 
        [TestMethod] public void modify_index() => Test("modify_index", false, false, false, false);
        // Tests scenario where STATISTICS information is added  to COLUMN. 
        [TestMethod] public void add_statistics() => Test("add_statistics", false, false, false, false);
        // Tests scenario where STATISTICS information is modified. 
        [TestMethod] public void modify_statistics() => Test("modify_statistics", false, false, false, false);
        // Tests scenario where STATISTICS information is dropped. 
        [TestMethod] public void drop_statistics() => Test("drop_statistics", false, false, false, false);
        // Tests scenario where DEFAULT value is set on COLUMN. 
        [TestMethod] public void add_default_value() => Test("add_default_value", false, false, false, false);
        // Tests scenario where DEFAULT value is modified. 
        [TestMethod] public void modify_default_value() => Test("modify_default_value", false, false, false, false);
        // Tests scenario where DEFAULT value is dropped from COLUMN. 
        [TestMethod] public void drop_default_value() => Test("drop_default_value", false, false, false, false);
        // Tests scenario where NOT NULL constraint is set on COLUMN. 
        [TestMethod] public void add_not_null() => Test("add_not_null", false, false, false, false);
        // Tests scenario where NOT NULL constraint is dropped  from COLUMN. 
        [TestMethod] public void drop_not_null() => Test("drop_not_null", false, false, false, false);
        // Tests scenario where COLUMN is added to TABLE definition. 
        [TestMethod] public void add_column() => Test("add_column", false, false, false, false);
        // Tests scenario where COLUMN is dropped from TABLE. 
        [TestMethod] public void drop_column() => Test("drop_column", false, false, false, false);
        // Tests scenario where new TABLE is added. 
        [TestMethod] public void add_table() => Test("add_table", false, false, false, false);
        // Tests scenario where TABLE is dropped. 
        [TestMethod] public void drop_table() => Test("drop_table", false, false, false, false);
        // Tests scenario where TABLE CONSTRAINT is added. 
        [TestMethod] public void add_constraint() => Test("add_constraint", false, false, false, false);
        // Tests scenario where TABLE CONSTRAINT is modified. 
        [TestMethod] public void modify_constraint() => Test("modify_constraint", false, false, false, false);
        // Tests scenario where TABLE CONSTRAINT is dropped. 
        [TestMethod] public void drop_constraint() => Test("drop_constraint", false, false, false, false);
        // Tests scenario where UNIQUE TABLE CONSTRAINT is added. 
        [TestMethod] public void add_unique_constraint() => Test("add_unique_constraint", false, false, false, true);
        // Tests reading of TABLE with INHERITS. 
        [TestMethod] public void read_inherits() => Test("read_inherits", false, false, false, false);
        // Tests scenario where TABLE with INHERITS is added. 
        [TestMethod] public void add_inherits() => Test("add_inherits", false, false, false, false);
        // Tests scenario where original and new TABLE contain different INHERITS. 
        [TestMethod] public void modify_inherits() => Test("modify_inherits", false, false, false, false);
        // Tests scenario where SEQUENCE is added. 
        [TestMethod] public void add_sequence() => Test("add_sequence", false, false, false, false);
        // Tests scenario where SEQUENCE is dropped. 
        [TestMethod] public void drop_sequence() => Test("drop_sequence", false, false, false, false);
        // Tests scenario where INCREMENT BY is modified on SEQUENCE. 
        [TestMethod] public void modify_sequence_increment() => Test("modify_sequence_increment", false, false, false, false);
        // Tests scenario where START WITH is modified on SEQUENCE (both with --ignore-start-with turned off and on). 
        [TestMethod] public void modify_sequence_start_ignore_off() => Test("modify_sequence_start_ignore_off", false, false, false, false);
        [TestMethod] public void modify_sequence_start_ignore_on() => Test("modify_sequence_start_ignore_on", false, false, false, true);
        // Tests scenario where MINVALUE is modified on SEQUENCE (both setting and unsetting the value). 
        [TestMethod] public void modify_sequence_minvalue_set() => Test("modify_sequence_minvalue_set", false, false, false, false);
        [TestMethod] public void modify_sequence_minvalue_unset() => Test("modify_sequence_minvalue_unset", false, false, false, false);
        // Tests scenario where MAXVALUE is modified on SEQUENCE (both setting and unsetting the value). 
        [TestMethod] public void modify_sequence_maxvalue_set() => Test("modify_sequence_maxvalue_set", false, false, false, false);
        [TestMethod] public void modify_sequence_maxvalue_unset() => Test("modify_sequence_maxvalue_unset", false, false, false, false);
        // Tests scenario where CACHE is modified on SEQUENCE. 
        [TestMethod] public void modify_sequence_cache() => Test("modify_sequence_cache", false, false, false, false);
        // Tests scenario where CYCLE is modified on SEQUENCE. 
        [TestMethod] public void modify_sequence_cycle_on() => Test("modify_sequence_cycle_on", false, false, false, false);

        [TestMethod] public void modify_sequence_cycle_off() => Test("modify_sequence_cycle_off", false, false, false, false);
        // Tests correct finding of function end. 
        [TestMethod] public void modify_function_end_detection() => Test("modify_function_end_detection", false, false, false, false);
        // Tests scenario where new FUNCTION without args is added. 
        [TestMethod] public void add_function_noargs() => Test("add_function_noargs", false, false, false, false);
        // Tests scenario where FUNCTION without args is dropped. 
        [TestMethod] public void drop_function_noargs() => Test("drop_function_noargs", false, false, false, false);
        // Tests scenario where FUNCTION without args is modified. 
        [TestMethod] public void modify_function_noargs() => Test("modify_function_noargs", false, false, false, false);
        // Tests scenario where new FUNCTION with args is added. 
        [TestMethod] public void add_function_args() => Test("add_function_args", false, false, false, false);
        // Tests scenario where FUNCTION with args is dropped. 
        [TestMethod] public void drop_function_args() => Test("drop_function_args", false, false, false, false);
        // Tests scenario where FUNCTION with args is modified. 
        [TestMethod] public void modify_function_args() => Test("modify_function_args", false, false, false, false);
        // Tests scenario where new FUNCTION with args is added. 
        [TestMethod] public void add_function_args2() => Test("add_function_args2", false, false, false, false);
        // Tests scenario where FUNCTION with args is dropped. 
        [TestMethod] public void drop_function_args2() => Test("drop_function_args2", false, false, false, false);
        // Tests scenario where FUNCTION with args is modified. 
        [TestMethod] public void modify_function_args2() => Test("modify_function_args2", false, false, false, false);
        // Tests scenario where FUNCTION with same name but different args is added. 
        [TestMethod] public void add_function_similar() => Test("add_function_similar", false, false, false, false);
        // Tests scenario where FUNCTION with same name but different args is dropped. 
        [TestMethod] public void drop_function_similar() => Test("drop_function_similar", false, false, false, false);
        // Tests scenario where FUNCTION with same name but different args is modified. 
        [TestMethod] public void modify_function_similar() => Test("modify_function_similar", false, false, false, false);
        // Tests different whitespace formatting in functions 
        [TestMethod] public void function_equal_whitespace() => Test("function_equal_whitespace", false, false, true, false);
        // Tests scenario where TRIGGER is added. 
        [TestMethod] public void add_trigger() => Test("add_trigger", false, false, false, false);
        // Tests scenario where TRIGGER is dropped. 
        [TestMethod] public void drop_trigger() => Test("drop_trigger", false, false, false, false);
        // Tests scenario where TRIGGER is modified. 
        [TestMethod] public void modify_trigger() => Test("modify_trigger", false, false, false, false);
        // Tests scenario where VIEW is added. 
        [TestMethod] public void add_view() => Test("add_view", false, false, false, false);
        // Tests scenario where VIEW is dropped. 
        [TestMethod] public void drop_view() => Test("drop_view", false, false, false, false);
        // Tests scenario where VIEW is modified. 
        [TestMethod] public void modify_view() => Test("modify_view", false, false, false, false);
        // Tests scenario where --add-defaults is specified. 
        [TestMethod] public void add_defaults() => Test("add_defaults", true, false, false, false);
        // Tests scenario where multiple schemas are in the dumps. 
        [TestMethod] public void multiple_schemas() => Test("multiple_schemas", false, false, false, false);
        // Tests scenario where --add-transaction is specified. 
        [TestMethod] public void multiple_schemas_trans() => Test("multiple_schemas", false, true, false, false);
        // Tests dropping view default value 
        [TestMethod] public void alter_view_drop_default() => Test("alter_view_drop_default", false, true, false, false);
        // Tests adding view default value 
        [TestMethod] public void alter_view_add_default() => Test("alter_view_add_default", false, true, false, false);
        // Tests adding of comments 
        [TestMethod] public void add_comments() => Test("add_comments", false, true, false, false);
        // Tests dropping of comments 
        [TestMethod] public void drop_comments() => Test("drop_comments", false, true, false, false);
        // Tests altering of comments 
        [TestMethod] public void alter_comments() => Test("alter_comments", false, true, false, false);
        // Tests changing view default value 
        [TestMethod] public void alter_view_change_default() => Test("alter_view_change_default", false, true, false, false);
        // Tests creation of sequence with bug in MINVALUE value 
        [TestMethod] public void add_sequence_bug2100013() => Test("add_sequence_bug2100013", false, true, false, false);
        // Tests view with default value 
        [TestMethod] public void view_bug3080388() => Test("view_bug3080388", false, true, false, false);
        // Tests function arguments beginning with in_ 
        [TestMethod] public void function_bug3084274() => Test("function_bug3084274", false, true, false, false);
        // Tests addition of comment when new column has been added 
        [TestMethod] public void add_comment_new_column() => Test("add_comment_new_column", false, true, false, false);
        // Tests handling of quoted schemas in search_path 
        [TestMethod] public void quoted_schema() => Test("quoted_schema", false, true, false, false);
        // Tests adding new column with add defaults turned on 
        [TestMethod] public void add_column_add_defaults() => Test("add_column_add_defaults", true, true, false, false);
        // Tests adding new sequence that is owned by table 
        [TestMethod] public void add_owned_sequence() => Test("add_owned_sequence", false, true, false, false);
        // Tests adding empty table 
        [TestMethod] public void add_empty_table() => Test("add_empty_table", false, false, false, false);


        public void Test(string fileNameTemplate, bool addDefaults, bool addTransaction, bool ignoreFunctionWhitespace, bool ignoreStartWith)
        {
            RunDiffSameOriginal(fileNameTemplate, addDefaults, addTransaction, ignoreFunctionWhitespace, ignoreStartWith);
            RunDiffSameNew(fileNameTemplate, addDefaults, addTransaction, ignoreFunctionWhitespace, ignoreStartWith);
            RunDiff(fileNameTemplate, addDefaults, addTransaction, ignoreFunctionWhitespace, ignoreStartWith);
        }

        public void RunDiffSameOriginal(string fileNameTemplate, bool addDefaults, bool addTransaction, bool ignoreFunctionWhitespace, bool ignoreStartWith)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var origRdr = new StreamReader(assembly.GetManifestResourceStream("pgdiff.test.scripts." + fileNameTemplate + "_original.sql")))
            using (var modifRdr = new StreamReader(assembly.GetManifestResourceStream("pgdiff.test.scripts." + fileNameTemplate + "_original.sql")))
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            using (var diffReader = new StreamReader(ms))
            {
                var arguments = new PgDiffArguments();
                PgDiff.CreateDiff(writer, arguments, origRdr, modifRdr);
                writer.Flush();

                var diff = diffReader.ReadToEnd();
                Assert.AreEqual("", diff.Trim());
            }
        }


        public void RunDiffSameNew(string fileNameTemplate, bool addDefaults, bool addTransaction, bool ignoreFunctionWhitespace, bool ignoreStartWith)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var origRdr = new StreamReader(assembly.GetManifestResourceStream("pgdiff.test.scripts." + fileNameTemplate + "_new.sql")))
            using (var modifRdr = new StreamReader(assembly.GetManifestResourceStream("pgdiff.test.scripts." + fileNameTemplate + "_new.sql")))
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            using (var diffReader = new StreamReader(ms))
            {
                var arguments = new PgDiffArguments();
                PgDiff.CreateDiff(writer, arguments, origRdr, modifRdr);
                writer.Flush();

                var diff = diffReader.ReadToEnd();
                Assert.AreEqual("", diff.Trim());
            }
        }


        public void RunDiff(string fileNameTemplate, bool addDefaults, bool addTransaction, bool ignoreFunctionWhitespace, bool ignoreStartWith)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var origRdr = new StreamReader(assembly.GetManifestResourceStream("pgdiff.test.scripts." + fileNameTemplate + "_original.sql")))
            using (var modifRdr = new StreamReader(assembly.GetManifestResourceStream("pgdiff.test.scripts." + fileNameTemplate + "_new.sql")))
            using (var expectedDiffRdr = new StreamReader(assembly.GetManifestResourceStream("pgdiff.test.scripts." + fileNameTemplate + "_diff.sql")))
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            using (var diffReader = new StreamReader(ms))
            {
                var arguments = new PgDiffArguments
                {
                    AddDefaults = addDefaults,
                    //AddTransaction = addTransaction, //TODO: this is a bug of the Java version
                    IgnoreFunctionWhitespace = ignoreFunctionWhitespace,
                    IgnoreStartWith = ignoreStartWith
                };

                PgDiff.CreateDiff(writer, arguments, origRdr, modifRdr);
                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                
                Assert.AreEqual(expectedDiffRdr.ReadToEnd().Replace("\r\n","\n").Trim(), diffReader.ReadToEnd().Replace("\r\n", "\n").Trim());
            }
        }
    }
}
