using System.IO;
using System.Text;
using pgdiff.schema;

namespace pgdiff
{
    public class PgDiffSequences
    {
        private PgDiffSequences()
        {
        }


        public static void CreateSequences(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            // Add new sequences
            foreach (var sequence in newSchema.GetSequences())
                if (oldSchema == null
                    || !oldSchema.ContainsSequence(sequence.Name))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(sequence.GetCreationSql());
                }
        }


        public static void AlterCreatedSequences(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            // Alter created sequences
            foreach (var sequence in newSchema.GetSequences())
                if ((oldSchema == null
                     || !oldSchema.ContainsSequence(sequence.Name))
                    && !string.IsNullOrEmpty(sequence.OwnedBy))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(sequence.GetOwnedBySql());
                }
        }


        public static void DropSequences(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            if (oldSchema == null)
                return;

            // Drop sequences that do not exist in new schema
            foreach (var sequence in oldSchema.GetSequences())
                if (!newSchema.ContainsSequence(sequence.Name))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(sequence.GetDropSql());
                }
        }


        public static void AlterSequences(TextWriter writer, PgDiffArguments arguments, 
            PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            if (oldSchema == null)
                return;

            var sbSql = new StringBuilder(100);

            foreach (var newSequence in newSchema.GetSequences())
            {
                var oldSequence =
                    oldSchema.GetSequence(newSequence.Name);

                if (oldSequence == null)
                    continue;

                sbSql.Length = 0;

                var oldIncrement = oldSequence.Increment;
                var newIncrement = newSequence.Increment;

                if (newIncrement != null
                    && !newIncrement.Equals(oldIncrement))
                {
                    sbSql.Append("\n\tINCREMENT BY ");
                    sbSql.Append(newIncrement);
                }

                var oldMinValue = oldSequence.MinValue;
                var newMinValue = newSequence.MinValue;

                if (newMinValue == null && oldMinValue != null)
                {
                    sbSql.Append("\n\tNO MINVALUE");
                }
                else if (newMinValue != null
                         && !newMinValue.Equals(oldMinValue))
                {
                    sbSql.Append("\n\tMINVALUE ");
                    sbSql.Append(newMinValue);
                }

                var oldMaxValue = oldSequence.MaxValue;
                var newMaxValue = newSequence.MaxValue;

                if (newMaxValue == null && oldMaxValue != null)
                {
                    sbSql.Append("\n\tNO MAXVALUE");
                }
                else if (newMaxValue != null
                         && !newMaxValue.Equals(oldMaxValue))
                {
                    sbSql.Append("\n\tMAXVALUE ");
                    sbSql.Append(newMaxValue);
                }

                if (!arguments.IgnoreStartWith)
                {
                    var oldStart = oldSequence.StartWith;
                    var newStart = newSequence.StartWith;

                    if (newStart != null && !newStart.Equals(oldStart))
                    {
                        sbSql.Append("\n\tRESTART WITH ");
                        sbSql.Append(newStart);
                    }
                }

                var oldCache = oldSequence.Cache;
                var newCache = newSequence.Cache;

                if (newCache != null && !newCache.Equals(oldCache))
                {
                    sbSql.Append("\n\tCACHE ");
                    sbSql.Append(newCache);
                }

                var oldCycle = oldSequence.Cycle;
                var newCycle = newSequence.Cycle;

                if (oldCycle && !newCycle)
                    sbSql.Append("\n\tNO CYCLE");
                else if (!oldCycle && newCycle)
                    sbSql.Append("\n\tCYCLE");

                var oldOwnedBy = oldSequence.OwnedBy;
                var newOwnedBy = newSequence.OwnedBy;

                if (newOwnedBy != null && !newOwnedBy.Equals(oldOwnedBy))
                {
                    sbSql.Append("\n\tOWNED BY ");
                    sbSql.Append(newOwnedBy);
                }

                if (sbSql.Length > 0)
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("ALTER SEQUENCE "
                                 + PgDiffUtils.GetQuotedName(newSequence.Name));
                    writer.Write(sbSql.ToString());
                    writer.WriteLine(';');
                }

                if (oldSequence.Comment == null
                    && newSequence.Comment != null
                    || oldSequence.Comment != null
                    && newSequence.Comment != null
                    && !oldSequence.Comment.Equals(
                        newSequence.Comment))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON SEQUENCE ");
                    writer.Write(PgDiffUtils.GetQuotedName(newSequence.Name));
                    writer.Write(" IS ");
                    writer.Write(newSequence.Comment);
                    writer.WriteLine(';');
                }
                else if (oldSequence.Comment != null
                         && newSequence.Comment == null)
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON SEQUENCE ");
                    writer.Write(newSequence.Name);
                    writer.WriteLine(" IS NULL;");
                }
            }
        }
    }
}