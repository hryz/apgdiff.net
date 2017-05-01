using System.Collections.Generic;
using System.Linq;

namespace pgdiff.schema
{
    public class PgDatabase
    {
        public PgDatabase()
        {
            Schemas.Add(new PgSchema("public"));
            DefaultSchema = Schemas[0];
        }


        public List<PgSchema> Schemas { get; set; } = new List<PgSchema>();

        public List<string> IgnoredStatements { get; set; } = new List<string>();

        public PgSchema DefaultSchema { get; set; }

        public string Comment { get; set; }

        public void SetDefaultSchema(string name)
        {
            DefaultSchema = GetSchema(name);
        }

        public PgSchema GetSchema(string name)
        {
            return name != null
                ? Schemas.FirstOrDefault(schema => schema.Name.Equals(name))
                : DefaultSchema;
        }
    }
}