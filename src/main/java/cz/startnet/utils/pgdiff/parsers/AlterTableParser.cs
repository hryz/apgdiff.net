using System;
using System.Collections.Generic;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers
{
    public class AlterTableParser
    {
        private AlterTableParser()
        {
        }

        public static void Parse(PgDatabase database, string statement, bool outputIgnoredStatements)
        {
            var parser = new Parser(statement);
            parser.Expect("ALTER", "TABLE");
            parser.ExpectOptional("ONLY");

            var tableName = parser.ParseIdentifier();
            var schemaName = ParserUtils.GetSchemaName(tableName, database);
            var schema = database.GetSchema(schemaName);

            if (schema == null)
                throw new Exception(string.Format(Resources.CannotFindSchema, schemaName, statement));

            var objectName = ParserUtils.GetObjectName(tableName);
            var table = schema.GetTable(objectName);

            if (table == null)
            {
                var view = schema.GetView(objectName);

                if (view != null)
                {
                    ParseView(parser, view, outputIgnoredStatements, tableName, database);
                    return;
                }

                var sequence = schema.GetSequence(objectName);

                if (sequence != null)
                {
                    ParseSequence(parser, sequence, outputIgnoredStatements, tableName, database);
                    return;
                }

                throw new Exception(string.Format(Resources.CannotFindObject, tableName, statement));
            }

            while (!parser.ExpectOptional(";"))
            {
                if (parser.ExpectOptional("ALTER"))
                    ParseAlterColumn(parser, table);
                else if (parser.ExpectOptional("CLUSTER", "ON"))
                    table.ClusterIndexName = ParserUtils.GetObjectName(parser.ParseIdentifier());
                else if (parser.ExpectOptional("OWNER", "TO"))
                    if (outputIgnoredStatements)
                        database.IgnoredStatements.Add($"ALTER TABLE {tableName} OWNER TO {parser.ParseIdentifier()};");
                    else
                        parser.ParseIdentifier();
                else if (parser.ExpectOptional("ADD"))
                    if (parser.ExpectOptional("FOREIGN", "KEY"))
                        ParseAddForeignKey(parser, table);
                    else if (parser.ExpectOptional("CONSTRAINT"))
                        ParseAddConstraint(parser, table, schema);
                    else
                        parser.ThrowUnsupportedCommand();
                else if (parser.ExpectOptional("ENABLE"))
                    ParseEnable(parser, outputIgnoredStatements, tableName, database);
                else if (parser.ExpectOptional("DISABLE"))
                    ParseDisable(parser, outputIgnoredStatements, tableName, database);
                else
                    parser.ThrowUnsupportedCommand();

                if (parser.ExpectOptional(";"))
                    break;
                parser.Expect(",");
            }
        }


        private static void ParseEnable(Parser parser, bool outputIgnoredStatements, string tableName, PgDatabase database)
        {
            if (parser.ExpectOptional("REPLICA"))
                if (parser.ExpectOptional("TRIGGER"))
                    if (outputIgnoredStatements)
                        database.IgnoredStatements.Add($"ALTER TABLE {tableName} ENABLE REPLICA TRIGGER {parser.ParseIdentifier()};");
                    else
                        parser.ParseIdentifier();
                else if (parser.ExpectOptional("RULE"))
                    if (outputIgnoredStatements)
                        database.IgnoredStatements.Add($"ALTER TABLE {tableName} ENABLE REPLICA RULE {parser.ParseIdentifier()};");
                    else
                        parser.ParseIdentifier();
                else
                    parser.ThrowUnsupportedCommand();
            else if (parser.ExpectOptional("ALWAYS"))
                if (parser.ExpectOptional("TRIGGER"))
                    if (outputIgnoredStatements)
                        database.IgnoredStatements.Add($"ALTER TABLE {tableName} ENABLE ALWAYS TRIGGER {parser.ParseIdentifier()};");
                    else
                        parser.ParseIdentifier();
                else if (parser.ExpectOptional("RULE"))
                    if (outputIgnoredStatements)
                        database.IgnoredStatements.Add($"ALTER TABLE {tableName} ENABLE RULE {parser.ParseIdentifier()};");
                    else
                        parser.ParseIdentifier();
                else
                    parser.ThrowUnsupportedCommand();
        }


        private static void ParseDisable(Parser parser, bool outputIgnoredStatements, string tableName, PgDatabase database)
        {
            if (parser.ExpectOptional("TRIGGER"))
                if (outputIgnoredStatements)
                    database.IgnoredStatements.Add($"ALTER TABLE {tableName} DISABLE TRIGGER {parser.ParseIdentifier()};");
                else
                    parser.ParseIdentifier();
            else if (parser.ExpectOptional("RULE"))
                if (outputIgnoredStatements)
                    database.IgnoredStatements.Add($"ALTER TABLE {tableName} DISABLE RULE {parser.ParseIdentifier()};");
                else
                    parser.ParseIdentifier();
            else
                parser.ThrowUnsupportedCommand();
        }


        private static void ParseAddConstraint(Parser parser,
            PgTable table, PgSchema schema)
        {
            var constraintName = ParserUtils.GetObjectName(parser.ParseIdentifier());
            var constraint = new PgConstraint(constraintName);
            constraint.TableName = table.Name;
            table.AddConstraint(constraint);

            if (parser.ExpectOptional("PRIMARY", "KEY"))
            {
                schema.AddPrimaryKey(constraint);
                constraint.Definition ="PRIMARY KEY " + parser.GetExpression();
            }
            else
            {
                constraint.Definition = parser.GetExpression();
            }
        }


        private static void ParseAlterColumn(Parser parser,
            PgTable table)
        {
            parser.ExpectOptional("COLUMN");

            var columnName =
                ParserUtils.GetObjectName(parser.ParseIdentifier());

            if (parser.ExpectOptional("SET"))
                if (parser.ExpectOptional("STATISTICS"))
                {
                    var column = table.GetColumn(columnName);
                    if (column == null)
                        throw new Exception(string.Format(Resources.CannotFindTableColumn, columnName, table.Name, parser.GetString()));

                    column.Statistics = parser.ParseInteger();
                }
                else if (parser.ExpectOptional("DEFAULT"))
                {
                    var defaultValue = parser.GetExpression();
                    if (table.ContainsColumn(columnName))
                    {
                        var column = table.GetColumn(columnName);
                        if (column == null)
                            throw new Exception(string.Format(Resources.CannotFindTableColumn, columnName, table.Name, parser.GetString()));

                        column.DefaultValue =defaultValue;
                    }
                    else
                    {
                        throw new ParserException(string.Format(Resources.CannotFindColumnInTable, columnName, table.Name));
                    }
                }
                else if (parser.ExpectOptional("STORAGE"))
                {
                    var column = table.GetColumn(columnName);

                    if (column == null)
                        throw new Exception(string.Format(Resources.CannotFindTableColumn, columnName, table.Name, parser.GetString()));

                    if (parser.ExpectOptional("PLAIN"))
                        column.Storage = "PLAIN";
                    else if (parser.ExpectOptional("EXTERNAL"))
                        column.Storage = "EXTERNAL";
                    else if (parser.ExpectOptional("EXTENDED"))
                        column.Storage = "EXTENDED";
                    else if (parser.ExpectOptional("MAIN"))
                        column.Storage = "MAIN";
                    else
                        parser.ThrowUnsupportedCommand();
                }
                else
                {
                    parser.ThrowUnsupportedCommand();
                }
            else
                parser.ThrowUnsupportedCommand();
        }


        private static void ParseAddForeignKey(Parser parser, PgTable table)
        {
            var columnNames = new List<string>();
            parser.Expect("(");

            while (!parser.ExpectOptional(")"))
            {
                columnNames.Add(ParserUtils.GetObjectName(parser.ParseIdentifier()));
                if (parser.ExpectOptional(")"))
                    break;

                parser.Expect(",");
            }

            var constraintName = ParserUtils.GenerateName(table.Name + "_", columnNames, "_fkey");
            var constraint = new PgConstraint(constraintName);
            table.AddConstraint(constraint);
            constraint.Definition = parser.GetExpression();
            constraint.TableName =table.Name;
        }


        private static void ParseView(Parser parser, PgView view, bool outputIgnoredStatements, string viewName, PgDatabase database)
        {
            while (!parser.ExpectOptional(";"))
                if (parser.ExpectOptional("ALTER"))
                {
                    parser.ExpectOptional("COLUMN");
                    var columnName = ParserUtils.GetObjectName(parser.ParseIdentifier());
                    if (parser.ExpectOptional("SET", "DEFAULT"))
                    {
                        var expression = parser.GetExpression();
                        view.AddColumnDefaultValue(columnName, expression);
                    }
                    else if (parser.ExpectOptional("DROP", "DEFAULT"))
                    {
                        view.RemoveColumnDefaultValue(columnName);
                    }
                    else
                    {
                        parser.ThrowUnsupportedCommand();
                    }
                }
                else if (parser.ExpectOptional("OWNER", "TO"))
                {
                    // we do not parse this one so we just consume the identifier
                    if (outputIgnoredStatements)
                        database.IgnoredStatements.Add($"ALTER TABLE {viewName} OWNER TO {parser.ParseIdentifier()};");
                    else
                        parser.ParseIdentifier();
                }
                else
                {
                    parser.ThrowUnsupportedCommand();
                }
        }


        private static void ParseSequence(Parser parser, PgSequence sequence, bool outputIgnoredStatements, string sequenceName, PgDatabase database)
        {
            while (!parser.ExpectOptional(";"))
                if (parser.ExpectOptional("OWNER", "TO"))
                    if (outputIgnoredStatements)
                        database.IgnoredStatements.Add($"ALTER TABLE {sequenceName} OWNER TO {parser.ParseIdentifier()};");
                    else
                        parser.ParseIdentifier();
                else
                    parser.ThrowUnsupportedCommand();
        }
    }
}