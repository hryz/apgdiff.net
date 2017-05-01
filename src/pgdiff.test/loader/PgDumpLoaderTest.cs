using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pgdiff.loader;

namespace pgdiff.test
{
    [TestClass]
    public class PgDumpLoaderTest
    {
        [TestMethod] public void LoadSchema1() => LoadSchema(1);
        [TestMethod] public void LoadSchema2() => LoadSchema(2);
        [TestMethod] public void LoadSchema3() => LoadSchema(3);
        [TestMethod] public void LoadSchema4() => LoadSchema(4);
        [TestMethod] public void LoadSchema5() => LoadSchema(5);
        [TestMethod] public void LoadSchema6() => LoadSchema(6);
        [TestMethod] public void LoadSchema7() => LoadSchema(7);
        [TestMethod] public void LoadSchema8() => LoadSchema(8);
        [TestMethod] public void LoadSchema9() => LoadSchema(9);
        [TestMethod] public void LoadSchema10() => LoadSchema(10);
        [TestMethod] public void LoadSchema11() => LoadSchema(11);
        [TestMethod] public void LoadSchema12() => LoadSchema(12);
        [TestMethod] public void LoadSchema13() => LoadSchema(13);
        [TestMethod] public void LoadSchema14() => LoadSchema(14);
        [TestMethod] public void LoadSchema15() => LoadSchema(15);

        private static void LoadSchema(int i)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream($"pgdiff.test.scripts.loader.schema_{i}.sql"))
            using (var reader = new StreamReader(stream))
            {
                PgDumpLoader.LoadDatabaseSchema(reader, "UTF-8", false, false);
            }
        }
    }
}

