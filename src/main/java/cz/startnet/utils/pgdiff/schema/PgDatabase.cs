using System.Collections.Generic;

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
            if (name == null)
                return DefaultSchema;

            foreach (var schema in Schemas)
                if (schema.Name.Equals(name))
                    return schema;

            return null;
        }
    }
}