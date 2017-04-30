using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace pgdiff.schema
{
    public class PgFunction
    {
        private readonly List<Argument> _arguments = new List<Argument>();

        public string Body { get; set; }

        public string Comment { get; set; }

        public string Name { get; set; }


        public string GetCreationSql()
        {
            var sbSql = new StringBuilder(500);
            sbSql.Append("CREATE OR REPLACE FUNCTION ");
            sbSql.Append(PgDiffUtils.GetQuotedName(Name));
            sbSql.Append('(');

            var addComma = false;

            foreach (var argument in _arguments)
            {
                if (addComma) sbSql.Append(", ");

                sbSql.Append(argument.GetDeclaration(true));

                addComma = true;
            }

            sbSql.Append(") ");
            sbSql.Append(Body);
            sbSql.Append(';');

            if (!string.IsNullOrEmpty(Comment))
            {
                sbSql.Append("\n\nCOMMENT ON FUNCTION ");
                sbSql.Append(PgDiffUtils.GetQuotedName(Name));
                sbSql.Append('(');

                addComma = false;

                foreach (var argument in _arguments)
                {
                    if (addComma) sbSql.Append(", ");

                    sbSql.Append(argument.GetDeclaration(false));

                    addComma = true;
                }

                sbSql.Append(") IS ");
                sbSql.Append(Comment);
                sbSql.Append(';');
            }

            return sbSql.ToString();
        }

        public string GetDropSql()
        {
            var sbString = new StringBuilder(100);
            sbString.Append("DROP FUNCTION ");
            sbString.Append(Name);
            sbString.Append('(');

            var addComma = false;

            foreach (var argument in _arguments)
            {
                if ("OUT".Equals(argument.Mode, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (addComma) sbString.Append(", ");

                sbString.Append(argument.GetDeclaration(false));

                addComma = true;
            }

            sbString.Append(");");

            return sbString.ToString();
        }

        public List<Argument> GetArguments()
        {
            return new List<Argument>(_arguments);
        }


        public void AddArgument(Argument argument)
        {
            _arguments.Add(argument);
        }


        public string GetSignature()
        {
            var sbString = new StringBuilder(100);
            sbString.Append(Name);
            sbString.Append('(');

            var addComma = false;

            foreach (var argument in _arguments)
            {
                if ("OUT".Equals(argument.Mode, StringComparison.InvariantCultureIgnoreCase)) continue;

                if (addComma) sbString.Append(',');

                sbString.Append(argument.DataType.ToLower());

                addComma = true;
            }

            sbString.Append(')');

            return sbString.ToString();
        }


        public override bool Equals(object @object)
        {
            if (!(@object is PgFunction)) return false;
            if (@object == this) return true;

            return Equals(@object, false);
        }


        public bool Equals(object @object, bool ignoreFunctionWhitespace)
        {
            var equals = false;

            if (this == @object)
            {
                equals = true;
            }
            else if (@object is PgFunction)
            {
                var function = (PgFunction) @object;

                if (Name == null && function.Name != null
                    || Name != null && !Name.Equals(function.Name)) return false;

                string thisBody;
                string thatBody;

                if (ignoreFunctionWhitespace)
                {
                    thisBody = Regex.Replace(Body, "\\s+", " ");
                    thatBody = Regex.Replace(function.Body, "\\s+", " ");
                }
                else
                {
                    thisBody = Body;
                    thatBody = function.Body;
                }

                if (thisBody == null && thatBody != null
                    || thisBody != null && !thisBody.Equals(thatBody)) return false;

                if (_arguments.Count != function.GetArguments().Count) return false;
                for (var i = 0; i < _arguments.Count; i++)
                    if (!_arguments[i].Equals(function.GetArguments()[i])) return false;

                return true;
            }

            return equals;
        }


        public override int GetHashCode()
        {
            var sbString = new StringBuilder(500);
            sbString.Append(Body);
            sbString.Append('|');
            sbString.Append(Name);

            foreach (var argument in _arguments)
            {
                sbString.Append('|');
                sbString.Append(argument.GetDeclaration(true));
            }

            return sbString.ToString().GetHashCode();
        }


        public class Argument
        {
            public string DataType { get; set; }

            public string DefaultExpression { get; set; }

            private string _mode = "IN";
            public string Mode
            {
                get => _mode;
                set => _mode = string.IsNullOrEmpty(value) ? "IN" : value;
            }

            public string Name { get; set; }

            public string GetDeclaration(bool includeDefaultValue)
            {
                var sbString = new StringBuilder(50);

                if (Mode != null && !"IN".Equals(Mode, StringComparison.InvariantCultureIgnoreCase))
                {
                    sbString.Append(Mode);
                    sbString.Append(' ');
                }

                if (!string.IsNullOrEmpty(Name))
                {
                    sbString.Append(PgDiffUtils.GetQuotedName(Name));
                    sbString.Append(' ');
                }

                sbString.Append(DataType);

                if (includeDefaultValue && !string.IsNullOrEmpty(DefaultExpression))
                {
                    sbString.Append(" = ");
                    sbString.Append(DefaultExpression);
                }

                return sbString.ToString();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Argument)) return false;
                if (this == obj) return true;

                var argument = (Argument) obj;

                return (DataType?.Equals(argument.DataType, StringComparison.InvariantCultureIgnoreCase) ?? argument.DataType == null)
                       && (DefaultExpression?.Equals(DefaultExpression) ?? argument.DefaultExpression == null)
                       && (Mode?.Equals(argument.Mode, StringComparison.InvariantCultureIgnoreCase) ?? argument.Mode == null)
                       && (Name?.Equals(argument.Name) ?? argument.Name == null);
            }


            public override int GetHashCode()
            {
                var sbString = new StringBuilder(50);
                sbString.Append(Mode == null ? null : Mode.ToUpper());
                sbString.Append('|');
                sbString.Append(Name);
                sbString.Append('|');
                sbString.Append(DataType == null ? null : DataType.ToUpper());
                sbString.Append('|');
                sbString.Append(DefaultExpression);

                return sbString.ToString().GetHashCode();
            }
        }
    }
}