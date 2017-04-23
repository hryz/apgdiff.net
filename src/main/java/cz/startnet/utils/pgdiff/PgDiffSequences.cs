
using System;
using System.Text;
using cz.startnet.utils.pgdiff.schema;

namespace cz.startnet.utils.pgdiff {




public class PgDiffSequences {

    
    public static void createSequences(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        // Add new sequences
        for (PgSequence sequence : newSchema.getSequences()) {
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
        for (PgSequence sequence : newSchema.getSequences()) {
            if ((oldSchema == null
                    || !oldSchema.containsSequence(sequence.getName()))
                    && sequence.getOwnedBy() != null
                    && !sequence.getOwnedBy().isEmpty()) {
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
        for (PgSequence sequence : oldSchema.getSequences()) {
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

        for (PgSequence newSequence : newSchema.getSequences()) {
            PgSequence oldSequence =
                    oldSchema.getSequence(newSequence.getName());

            if (oldSequence == null) {
                continue;
            }

            sbSQL.setLength(0);

            String oldIncrement = oldSequence.getIncrement();
            String newIncrement = newSequence.getIncrement();

            if (newIncrement != null
                    && !newIncrement.Equals(oldIncrement)) {
                sbSQL.append("\n\tINCREMENT BY ");
                sbSQL.append(newIncrement);
            }

            String oldMinValue = oldSequence.getMinValue();
            String newMinValue = newSequence.getMinValue();

            if (newMinValue == null && oldMinValue != null) {
                sbSQL.append("\n\tNO MINVALUE");
            } else if (newMinValue != null
                    && !newMinValue.Equals(oldMinValue)) {
                sbSQL.append("\n\tMINVALUE ");
                sbSQL.append(newMinValue);
            }

            String oldMaxValue = oldSequence.getMaxValue();
            String newMaxValue = newSequence.getMaxValue();

            if (newMaxValue == null && oldMaxValue != null) {
                sbSQL.append("\n\tNO MAXVALUE");
            } else if (newMaxValue != null
                    && !newMaxValue.Equals(oldMaxValue)) {
                sbSQL.append("\n\tMAXVALUE ");
                sbSQL.append(newMaxValue);
            }

            if (!arguments.isIgnoreStartWith()) {
                String oldStart = oldSequence.getStartWith();
                String newStart = newSequence.getStartWith();

                if (newStart != null && !newStart.Equals(oldStart)) {
                    sbSQL.append("\n\tRESTART WITH ");
                    sbSQL.append(newStart);
                }
            }

            String oldCache = oldSequence.getCache();
            String newCache = newSequence.getCache();

            if (newCache != null && !newCache.Equals(oldCache)) {
                sbSQL.append("\n\tCACHE ");
                sbSQL.append(newCache);
            }

            bool oldCycle = oldSequence.isCycle();
            bool newCycle = newSequence.isCycle();

            if (oldCycle && !newCycle) {
                sbSQL.append("\n\tNO CYCLE");
            } else if (!oldCycle && newCycle) {
                sbSQL.append("\n\tCYCLE");
            }

            String oldOwnedBy = oldSequence.getOwnedBy();
            String newOwnedBy = newSequence.getOwnedBy();

            if (newOwnedBy != null && !newOwnedBy.Equals(oldOwnedBy)) {
                sbSQL.append("\n\tOWNED BY ");
                sbSQL.append(newOwnedBy);
            }

            if (sbSQL.length() > 0) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("ALTER SEQUENCE "
                        + PgDiffUtils.getQuotedName(newSequence.getName()));
                writer.Write(sbSQL.toString());
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