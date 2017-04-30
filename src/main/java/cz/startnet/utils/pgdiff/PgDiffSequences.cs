using System;
using System.IO;
using System.Text;
using pgdiff.schema;

namespace pgdiff {




public class PgDiffSequences {

    
    public static void CreateSequences(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        // Add new sequences
        foreach (PgSequence sequence in newSchema.GetSequences()) {
            if (oldSchema == null
                    || !oldSchema.ContainsSequence(sequence.GetName())) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(sequence.GetCreationSql());
            }
        }
    }

    
    public static void AlterCreatedSequences(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        // Alter created sequences
        foreach (PgSequence sequence in newSchema.GetSequences()) {
            if ((oldSchema == null
                    || !oldSchema.ContainsSequence(sequence.GetName()))
                    && !String.IsNullOrEmpty(sequence.GetOwnedBy())) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(sequence.GetOwnedBySql());
            }
        }
    }

    
    public static void DropSequences(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        // Drop sequences that do not exist in new schema
        foreach (PgSequence sequence in oldSchema.GetSequences()) {
            if (!newSchema.ContainsSequence(sequence.GetName())) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(sequence.GetDropSql());
            }
        }
    }

    
    public static void AlterSequences(TextWriter writer,
            PgDiffArguments arguments, PgSchema oldSchema,
            PgSchema newSchema, SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        StringBuilder sbSql = new StringBuilder(100);

        foreach (PgSequence newSequence in newSchema.GetSequences()) {
            PgSequence oldSequence =
                    oldSchema.GetSequence(newSequence.GetName());

            if (oldSequence == null) {
                continue;
            }

            sbSql.Length = 0;

            String oldIncrement = oldSequence.GetIncrement();
            String newIncrement = newSequence.GetIncrement();

            if (newIncrement != null
                    && !newIncrement.Equals(oldIncrement)) {
                sbSql.Append("\n\tINCREMENT BY ");
                sbSql.Append(newIncrement);
            }

            String oldMinValue = oldSequence.GetMinValue();
            String newMinValue = newSequence.GetMinValue();

            if (newMinValue == null && oldMinValue != null) {
                sbSql.Append("\n\tNO MINVALUE");
            } else if (newMinValue != null
                    && !newMinValue.Equals(oldMinValue)) {
                sbSql.Append("\n\tMINVALUE ");
                sbSql.Append(newMinValue);
            }

            String oldMaxValue = oldSequence.GetMaxValue();
            String newMaxValue = newSequence.GetMaxValue();

            if (newMaxValue == null && oldMaxValue != null) {
                sbSql.Append("\n\tNO MAXVALUE");
            } else if (newMaxValue != null
                    && !newMaxValue.Equals(oldMaxValue)) {
                sbSql.Append("\n\tMAXVALUE ");
                sbSql.Append(newMaxValue);
            }

            if (!arguments.IsIgnoreStartWith()) {
                String oldStart = oldSequence.GetStartWith();
                String newStart = newSequence.GetStartWith();

                if (newStart != null && !newStart.Equals(oldStart)) {
                    sbSql.Append("\n\tRESTART WITH ");
                    sbSql.Append(newStart);
                }
            }

            String oldCache = oldSequence.GetCache();
            String newCache = newSequence.GetCache();

            if (newCache != null && !newCache.Equals(oldCache)) {
                sbSql.Append("\n\tCACHE ");
                sbSql.Append(newCache);
            }

            bool oldCycle = oldSequence.IsCycle();
            bool newCycle = newSequence.IsCycle();

            if (oldCycle && !newCycle) {
                sbSql.Append("\n\tNO CYCLE");
            } else if (!oldCycle && newCycle) {
                sbSql.Append("\n\tCYCLE");
            }

            String oldOwnedBy = oldSequence.GetOwnedBy();
            String newOwnedBy = newSequence.GetOwnedBy();

            if (newOwnedBy != null && !newOwnedBy.Equals(oldOwnedBy)) {
                sbSql.Append("\n\tOWNED BY ");
                sbSql.Append(newOwnedBy);
            }

            if (sbSql.Length > 0) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("ALTER SEQUENCE "
                        + PgDiffUtils.GetQuotedName(newSequence.GetName()));
                writer.Write(sbSql.ToString());
                writer.WriteLine(';');
            }

            if (oldSequence.GetComment() == null
                    && newSequence.GetComment() != null
                    || oldSequence.GetComment() != null
                    && newSequence.GetComment() != null
                    && !oldSequence.GetComment().Equals(
                    newSequence.GetComment())) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON SEQUENCE ");
                writer.Write(PgDiffUtils.GetQuotedName(newSequence.GetName()));
                writer.Write(" IS ");
                writer.Write(newSequence.GetComment());
                writer.WriteLine(';');
            } else if (oldSequence.GetComment() != null
                    && newSequence.GetComment() == null) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON SEQUENCE ");
                writer.Write(newSequence.GetName());
                writer.WriteLine(" IS NULL;");
            }
        }
    }

    
    private PgDiffSequences() {
    }
}
}