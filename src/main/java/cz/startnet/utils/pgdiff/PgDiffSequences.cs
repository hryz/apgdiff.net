using System;
using System.IO;
using System.Text;
using pgdiff.schema;

namespace pgdiff {




public class PgDiffSequences {

    
    public static void createSequences(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        // Add new sequences
        foreach (PgSequence sequence in newSchema.getSequences()) {
            if (oldSchema == null
                    || !oldSchema.containsSequence(sequence.getName())) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(sequence.getCreationSQL());
            }
        }
    }

    
    public static void alterCreatedSequences(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        // Alter created sequences
        foreach (PgSequence sequence in newSchema.getSequences()) {
            if ((oldSchema == null
                    || !oldSchema.containsSequence(sequence.getName()))
                    && !String.IsNullOrEmpty(sequence.getOwnedBy())) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(sequence.getOwnedBySQL());
            }
        }
    }

    
    public static void dropSequences(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        // Drop sequences that do not exist in new schema
        foreach (PgSequence sequence in oldSchema.getSequences()) {
            if (!newSchema.containsSequence(sequence.getName())) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(sequence.getDropSQL());
            }
        }
    }

    
    public static void alterSequences(TextWriter writer,
            PgDiffArguments arguments, PgSchema oldSchema,
            PgSchema newSchema, SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        StringBuilder sbSQL = new StringBuilder(100);

        foreach (PgSequence newSequence in newSchema.getSequences()) {
            PgSequence oldSequence =
                    oldSchema.getSequence(newSequence.getName());

            if (oldSequence == null) {
                continue;
            }

            sbSQL.Length = 0;

            String oldIncrement = oldSequence.getIncrement();
            String newIncrement = newSequence.getIncrement();

            if (newIncrement != null
                    && !newIncrement.Equals(oldIncrement)) {
                sbSQL.Append("\n\tINCREMENT BY ");
                sbSQL.Append(newIncrement);
            }

            String oldMinValue = oldSequence.getMinValue();
            String newMinValue = newSequence.getMinValue();

            if (newMinValue == null && oldMinValue != null) {
                sbSQL.Append("\n\tNO MINVALUE");
            } else if (newMinValue != null
                    && !newMinValue.Equals(oldMinValue)) {
                sbSQL.Append("\n\tMINVALUE ");
                sbSQL.Append(newMinValue);
            }

            String oldMaxValue = oldSequence.getMaxValue();
            String newMaxValue = newSequence.getMaxValue();

            if (newMaxValue == null && oldMaxValue != null) {
                sbSQL.Append("\n\tNO MAXVALUE");
            } else if (newMaxValue != null
                    && !newMaxValue.Equals(oldMaxValue)) {
                sbSQL.Append("\n\tMAXVALUE ");
                sbSQL.Append(newMaxValue);
            }

            if (!arguments.isIgnoreStartWith()) {
                String oldStart = oldSequence.getStartWith();
                String newStart = newSequence.getStartWith();

                if (newStart != null && !newStart.Equals(oldStart)) {
                    sbSQL.Append("\n\tRESTART WITH ");
                    sbSQL.Append(newStart);
                }
            }

            String oldCache = oldSequence.getCache();
            String newCache = newSequence.getCache();

            if (newCache != null && !newCache.Equals(oldCache)) {
                sbSQL.Append("\n\tCACHE ");
                sbSQL.Append(newCache);
            }

            bool oldCycle = oldSequence.isCycle();
            bool newCycle = newSequence.isCycle();

            if (oldCycle && !newCycle) {
                sbSQL.Append("\n\tNO CYCLE");
            } else if (!oldCycle && newCycle) {
                sbSQL.Append("\n\tCYCLE");
            }

            String oldOwnedBy = oldSequence.getOwnedBy();
            String newOwnedBy = newSequence.getOwnedBy();

            if (newOwnedBy != null && !newOwnedBy.Equals(oldOwnedBy)) {
                sbSQL.Append("\n\tOWNED BY ");
                sbSQL.Append(newOwnedBy);
            }

            if (sbSQL.Length > 0) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("ALTER SEQUENCE "
                        + PgDiffUtils.getQuotedName(newSequence.getName()));
                writer.Write(sbSQL.ToString());
                writer.WriteLine(';');
            }

            if (oldSequence.getComment() == null
                    && newSequence.getComment() != null
                    || oldSequence.getComment() != null
                    && newSequence.getComment() != null
                    && !oldSequence.getComment().Equals(
                    newSequence.getComment())) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON SEQUENCE ");
                writer.Write(PgDiffUtils.getQuotedName(newSequence.getName()));
                writer.Write(" IS ");
                writer.Write(newSequence.getComment());
                writer.WriteLine(';');
            } else if (oldSequence.getComment() != null
                    && newSequence.getComment() == null) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON SEQUENCE ");
                writer.Write(newSequence.getName());
                writer.WriteLine(" IS NULL;");
            }
        }
    }

    
    private PgDiffSequences() {
    }
}
}