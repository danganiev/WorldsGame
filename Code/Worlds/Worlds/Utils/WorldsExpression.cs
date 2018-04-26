using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using NCalc;

namespace WorldsGame.Utils
{
    public class WorldsExpression
    {
        private readonly List<string> _parameters;
        private const string PATTERN = @"\[(.*?)\]";
        private const string NO_BRACKETS_PATTERN = @"\b[A-Za-z0-9_]+\b";

        public Expression Expression { get; private set; }

        public IList<string> Parameters { get { return _parameters.AsReadOnly(); } }

        public WorldsExpression(string expression)
        {
            _parameters = new List<string>();
            SetExpression(expression.ToLowerInvariant());
        }

        public void SetExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                Expression = new Expression("true");
                return;
            }

            MatchCollection matches = Regex.Matches(expression, PATTERN);

            foreach (Match match in matches)
            {
                string matchString = match.Groups[1].ToString();
                _parameters.Add(matchString);
            }
            string newExpression = expression;

            foreach (string parameter in from s in _parameters
                                         orderby s.Length descending
                                         select s)
            {
                newExpression = newExpression.Replace(parameter, "");
            }

            matches = Regex.Matches(newExpression, NO_BRACKETS_PATTERN);

            foreach (Match match in matches)
            {
                string matchString = match.Groups[0].ToString();
                if (matchString != "")
                {
                    int tempInt;
                    bool parseResult = int.TryParse(matchString, out tempInt);

                    if (!parseResult)
                    {
                        _parameters.Add(matchString);
                    }
                }
            }

            Expression = new Expression(expression);
        }

        public bool HasErrors()
        {
            return Expression.HasErrors();
        }
    }
}