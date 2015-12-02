// � 2015 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Pathfinder.Diagnostics;

namespace Sitecore.Pathfinder.XPath
{
    /// <summary>Parses a string into an Sitecore XPath query.</summary>
    /// <remarks>
    /// Queries = Query | ( Queries "|" Query )
    /// Query = PathStep ( PathStep )*
    /// PathStep = Node | Children | Descendants 
    /// Children = "/" Node
    /// Descendants = "//" Node
    /// Node = Attribute | Element
    /// Attribute = "@" Name
    /// Element = [ Axis ] ( "*" | "." | ".." | Name ) [ Predicate ]
    /// Name = Identifier | Name Identifier
    /// Axis = ( "ancestor" | "ancestor-or-self" | "descendants" | "descendants-or-self" |
    ///        "child" | "parent" | "following" | "preceding" | "self" ) "::"
    /// Predicate = "[" Expression "]"
    /// Expression = Term3 [ ( "or" | "xor" ) Term3 ]
    /// Term3 = Term2 [ "and" Term2 ]
    /// Term2 = Term1 [ ( "=" | ">" | ">=" | "&lt;" | "&lt;=" | "!=" ) Term1 ]
    /// Term1 = Term0 [ ( "+" | "-" ) Term0 ]
    /// Term0 = Factor [ ( "*" | "div" | "mod" ) Factor ]
    /// Factor = Queries | Literal | Number | True | False | "(" Expression ")" | 
    ///          Attribute | Function | Parameter
    /// Parameter = "$" Identifier
    /// Function = Identifier "(" ExpressionList ")"
    /// ExpressionList = Expression | ( ExpressionList "," Expression )
    /// </remarks>
    public class QueryParser
    {
        [NotNull]
        QueryBuilder _builder;

        [NotNull]
        private Tokenizer _tokenizer;

        Token _token;

        [NotNull]
        public static Opcode Parse([NotNull] string query)
        {
            var queryParser = new QueryParser();

            return queryParser.DoParse(query);
        }

        [NotNull]
        public static Opcode ParsePredicate([NotNull] string query)
        {
            var queryParser = new QueryParser();

            return queryParser.DoParsePredicate(query);
        }

        [NotNull]
        public static Opcode ParseExpression([NotNull] string query)
        {
            var queryParser = new QueryParser();

            return queryParser.DoParseExpression(query);
        }

        [NotNull]
        protected virtual Opcode DoParse([NotNull] string query)
        {
            Initialize(query);

            Opcode result = GetQueries();

            Match(TokenType.End, "End of string expected");

            return result;
        }

        [NotNull]
        protected virtual Opcode DoParsePredicate([NotNull] string query)
        {
            Initialize(query);

            var result = _builder.Predicate(GetExpression());

            Match(TokenType.End, "End of string expected");

            return result;
        }

        [NotNull]
        protected virtual Opcode DoParseExpression([NotNull] string query)
        {
            Initialize(query);

            var result = GetExpression();

            Match(TokenType.End, "End of string expected");

            return result;
        }

        [NotNull]
        protected Step GetAncestorAxis()
        {
            Match(QueryTokenType.Ancestor, "\"ancestor::\" or \"ancestor-or-self::\" expected");

            Step result;

            if (_token.Type == TokenType.Minus)
            {
                Match();
                Match(QueryTokenType.Or, "\"or\" expected");
                Match(TokenType.Minus, "\"-\" expected");
                Match(QueryTokenType.Self, "\"self\" expected");
                Match(TokenType.Axis, "\"::\" expected");

                ElementBase element = GetElement();

                result = _builder.AncestorOrSelf(element);
            }
            else
            {
                Match(TokenType.Axis, "\"::\" expected");

                ElementBase element = GetElement();

                result = _builder.Ancestor(element);
            }

            return result;
        }

        [NotNull]
        protected ElementBase GetAttribute()
        {
            Match(TokenType.At, "\"@\" expected");

            var name = "";

            if (_token.Type == TokenType.At)
            {
                Match();
                name = "@";
            }

            name += _token.Value;

            Match(TokenType.Identifier, "Identifier expected");

            while (_token.Type == TokenType.Identifier || _token.Type == TokenType.Number)
            {
                name += _token.Whitespace + _token.Value;
                Match();
            }

            var result = _builder.FieldElement(name);

            return result;
        }

        protected Step GetChildAxis()
        {
            Match(QueryTokenType.Child, "\"child::\" expected");
            Match(TokenType.Axis, "\"::\" expected");

            return _builder.Children(GetElement());
        }

        protected Step GetChildren()
        {
            Match(TokenType.Slash, "\"/\" expected");

            var result = GetNode();

            if (result is ElementBase)
            {
                result = _builder.Children(result as ElementBase);
            }

            if (!(result is Step))
            {
                Raise("Syntax error");
            }

            return result as Step;
        }

        protected Step GetDescendants()
        {
            Match(TokenType.DoubleSlash, "\"//\" expected");

            var result = GetNode();

            if (result is ElementBase)
            {
                result = _builder.Descendants(result as ElementBase);
            }

            if (!(result is Step))
            {
                Raise("Syntax error");
            }

            return result as Step;
        }

        protected Step GetDescendantsAxis()
        {
            Match(QueryTokenType.Descendant, "\"descendants::\" or \"descendants-or-self::\"expected");

            if (_token.Type == TokenType.Minus)
            {
                Match();
                Match(QueryTokenType.Or, "\"or\" expected");
                Match(TokenType.Minus, "\"-\" expected");
                Match(QueryTokenType.Self, "\"self\" expected");
                Match(TokenType.Axis, "\"::\" expected");

                return _builder.DescendantsOrSelf(GetElement());
            }
            Match(TokenType.Axis, "\"::\" expected");

            return _builder.Descendants(GetElement());
        }

        protected Step GetDotDot()
        {
            Match(TokenType.DotDot, "\"..\" expected");

            return _builder.Parent();
        }

        protected ElementBase GetElement()
        {
            Predicate predicate = null;
            var name = "";

            switch (_token.Type)
            {
                case TokenType.Identifier:
                    name = _token.Value;
                    Match();

                    while (_token.Type == TokenType.Identifier || _token.Type == TokenType.Number)
                    {
                        name += _token.Whitespace + _token.Value;
                        Match();
                    }

                    break;

                case TokenType.Number:
                    name = _token.Value;
                    Match();

                    while (_token.Type == TokenType.Identifier || _token.Type == TokenType.Number)
                    {
                        name += _token.Whitespace + _token.Value;
                        Match();
                    }

                    break;

                case TokenType.Guid:
                    name = _token.Value;
                    Match();
                    break;

                case TokenType.Star:
                    name = "*";
                    Match();
                    break;

                default:
                    Raise("Identifier, GUID or \"*\" expected");
                    break;
            }

            if (_token.Type == TokenType.StartSquareBracket)
            {
                predicate = GetPredicate();
            }

            var result = _builder.ItemElement(name, predicate);

            return result;
        }

        [NotNull]
        protected Opcode GetExpression()
        {
            Opcode result = GetTerm3();

            while (true)
            {
                switch (_token.Type)
                {
                    case QueryTokenType.Or:
                        Match();
                        result = _builder.Or(result, GetTerm3());
                        break;

                    case QueryTokenType.Xor:
                        Match();
                        result = _builder.Xor(result, GetTerm3());
                        break;

                    default:
                        return result;
                }
            }
        }

        [NotNull]
        protected Opcode GetFactor()
        {
            Opcode result = null;

            switch (_token.Type)
            {
                case TokenType.Literal:
                    result = _builder.Literal(_token.Value);
                    Match();
                    break;

                case TokenType.Number:
                    result = _builder.Number(_token.NumberValue);
                    Match();
                    break;

                case TokenType.Identifier:
                    if (Peek() == TokenType.StartParentes)
                    {
                        result = GetFunction();
                    }
                    else
                    {
                        result = GetQuery();
                    }
                    break;

                case QueryTokenType.Ancestor:
                case QueryTokenType.Child:
                case QueryTokenType.Descendant:
                case TokenType.Dot:
                case TokenType.DotDot:
                case TokenType.DoubleSlash:
                case QueryTokenType.Following:
                case TokenType.Guid:
                case QueryTokenType.Parent:
                case QueryTokenType.Preceding:
                case QueryTokenType.Self:
                case TokenType.Slash:
                case TokenType.Star:
                    result = GetQuery();
                    break;

                case QueryTokenType.True:
                    result = _builder.BooleanValue(true);
                    Match();
                    break;

                case QueryTokenType.False:
                    result = _builder.BooleanValue(false);
                    Match();
                    break;

                case TokenType.At:
                    result = GetAttribute();
                    break;

                case TokenType.StartParentes:
                    Match();

                    result = GetExpression();

                    Match(TokenType.EndParentes, "\")\" expected");
                    break;

                case TokenType.Dollar:
                    result = GetParameter();
                    break;

                default:
                    Raise("Expression expected");
                    break;
            }

            return result;
        }

        [NotNull]
        protected Step GetFollowingAxis()
        {
            Match(QueryTokenType.Following, "\"following::\" expected");
            Match(TokenType.Axis, "\"::\" expected");

            return _builder.Following(GetElement());
        }

        [NotNull]
        protected Opcode GetFunction()
        {
            var name = _token.Value;

            Match(TokenType.Identifier, "Function name expected");
            Match(TokenType.StartParentes, "\"(\" expected");

            Function result = _builder.Function(name);

            if (_token.Type != TokenType.EndParentes)
            {
                bool more;
                do
                {
                    more = false;

                    Opcode expression = GetExpression();
                    result.Add(expression);

                    if (_token.Type == TokenType.Comma)
                    {
                        Match();
                        more = true;
                    }
                }
                while (more);
            }

            Match(TokenType.EndParentes, "\")\" expected");

            return result;
        }

        [NotNull]
        protected object GetNode()
        {
            object result;

            switch (_token.Type)
            {
                case QueryTokenType.Ancestor:
                    result = GetAncestorAxis();
                    break;

                case TokenType.At:
                    result = GetAttribute();
                    break;

                case QueryTokenType.Child:
                    result = GetChildAxis();
                    break;

                case QueryTokenType.Descendant:
                    result = GetDescendantsAxis();
                    break;

                case TokenType.DotDot:
                    result = GetDotDot();
                    break;

                case TokenType.Dot:
                    result = GetSelf();
                    break;

                case QueryTokenType.Following:
                    result = GetFollowingAxis();
                    break;

                case QueryTokenType.Parent:
                    result = GetParentAxis();
                    break;

                case QueryTokenType.Preceding:
                    result = GetPrecedingAxis();
                    break;

                case QueryTokenType.Self:
                    result = GetSelfAxis();
                    break;

                default:
                    result = GetElement();
                    break;
            }

            return result;
        }

        [NotNull]
        protected ElementBase GetParameter()
        {
            Match(TokenType.Dollar, "\"$\" expected");

            var name = _token.Value;

            Match(TokenType.Identifier, "Identifier expected");

            return _builder.QueryParameter(name);
        }

        [NotNull]
        protected Step GetParentAxis()
        {
            Match(QueryTokenType.Parent, "\"parent::\" expected");
            Match(TokenType.Axis, "\"::\" expected");

            Step result = _builder.Parent();

            result.NextStep = _builder.Children(GetElement());

            return result;
        }

        [CanBeNull]
        protected Step GetPathStep()
        {
            object result;

            if (_token.Type == TokenType.Slash)
            {
                result = GetChildren();
            }
            else if (_token.Type == TokenType.DoubleSlash)
            {
                result = GetDescendants();
            }
            else
            {
                result = GetNode();

                if (result is ElementBase)
                {
                    result = _builder.Children(result as ElementBase);
                }

                if (!(result is Step))
                {
                    Raise("Syntax error");
                }
            }

            return result as Step;
        }

        [NotNull]
        protected Step GetPrecedingAxis()
        {
            Match(QueryTokenType.Preceding, "\"preceding::\" expected");
            Match(TokenType.Axis, "\"::\" expected");

            return _builder.Preceding(GetElement());
        }

        [NotNull]
        protected Predicate GetPredicate()
        {
            Match(TokenType.StartSquareBracket, "\"[\" expected");

            Predicate result = _builder.Predicate(GetExpression());

            Match(TokenType.EndSquareBracket, "\"]\" expected");

            return result;
        }

        [NotNull]
        protected Opcode GetQueries()
        {
            Opcode result = GetQuery();

            while (_token.Type == TokenType.Pipe)
            {
                Match();
                result = _builder.Add(result, GetQuery());
            }

            return result;
        }

        [NotNull]
        protected Opcode GetQuery()
        {
            Step result = null;

            if (_token.Type == TokenType.Slash || _token.Type == TokenType.DoubleSlash)
            {
                result = _builder.Root();
            }

            var step = GetPathStep();

            if (result != null)
            {
                result.NextStep = step;
            }
            else
            {
                result = step;
            }

            while (_token.Type == TokenType.Slash || _token.Type == TokenType.DoubleSlash

                //_token.Type == QueryTokenType.Star ||
                //_token.Type == QueryTokenType.At ||
                //_token.Type == QueryTokenType.Guid ||
                //_token.Type == QueryTokenType.Identifier
                )
            {
                if (step == null)
                {
                    throw new ParseException("Step expected");
                }

                step.NextStep = GetPathStep();
                step = step.NextStep;
            }

            if (result == null)
            {
                throw new ParseException("Query expected");
            }

            return result;
        }

        [NotNull]
        protected Step GetSelf()
        {
            Match(TokenType.Dot, "\".\" expected");

            Predicate predicate = null;
            if (_token.Type == TokenType.StartSquareBracket)
            {
                predicate = GetPredicate();
            }

            if (predicate == null)
            {
                throw new ParseException("Predicate expected");
            }

            return _builder.Self(predicate);
        }

        [NotNull]
        protected Step GetSelfAxis()
        {
            Match(QueryTokenType.Self, "\"self::\" expected");
            Match(TokenType.Axis, "\"::\" expected");

            Predicate predicate = null;
            if (_token.Type == TokenType.StartSquareBracket)
            {
                predicate = GetPredicate();
            }

            if (predicate == null)
            {
                throw new ParseException("Predicate expected");
            }

            return _builder.Self(predicate);
        }

        [NotNull]
        protected Opcode GetTerm0()
        {
            Opcode result = GetFactor();

            while (true)
            {
                switch (_token.Type)
                {
                    case TokenType.Star:
                        Match();
                        result = _builder.Multiply(result, GetFactor());
                        break;

                    case QueryTokenType.Div:
                        Match();
                        result = _builder.Divide(result, GetFactor());
                        break;

                    case QueryTokenType.Mod:
                        Match();
                        result = _builder.Modulus(result, GetFactor());
                        break;

                    default:
                        return result;
                }
            }
        }

        [NotNull]
        protected Opcode GetTerm1()
        {
            Opcode result = GetTerm0();

            while (true)
            {
                switch (_token.Type)
                {
                    case TokenType.Plus:
                        Match();
                        result = _builder.Add(result, GetTerm0());
                        break;

                    case TokenType.Minus:
                        Match();
                        result = _builder.Minus(result, GetTerm0());
                        break;

                    default:
                        return result;
                }
            }
        }

        [NotNull]
        protected Opcode GetTerm2()
        {
            Opcode result = GetTerm1();

            while (true)
            {
                switch (_token.Type)
                {
                    case TokenType.Equal:
                        Match();
                        result = _builder.Equals(result, GetTerm1());
                        break;

                    case TokenType.Unequals:
                        Match();
                        result = _builder.Unequals(result, GetTerm1());
                        break;

                    case TokenType.Smaller:
                        Match();
                        result = _builder.Smaller(result, GetTerm1());
                        break;

                    case TokenType.SmallerOrEquals:
                        Match();
                        result = _builder.SmallerOrEquals(result, GetTerm1());
                        break;

                    case TokenType.GreaterOrEquals:
                        Match();
                        result = _builder.GreaterOrEquals(result, GetTerm1());
                        break;

                    case TokenType.Greater:
                        Match();
                        result = _builder.Greater(result, GetTerm1());
                        break;

                    default:
                        return result;
                }
            }
        }

        [NotNull]
        protected Opcode GetTerm3()
        {
            Opcode result = GetTerm2();

            while (true)
            {
                switch (_token.Type)
                {
                    case QueryTokenType.And:
                        Match();
                        result = _builder.And(result, GetTerm2());
                        break;

                    default:
                        return result;
                }
            }
        }

        protected void Match()
        {
            _token = _tokenizer.NextToken();
        }

        protected void Match(int type, [NotNull] string error)
        {
            MatchCheck(type, error);
            Match();
        }

        protected void MatchCheck(int type, [NotNull] string error)
        {
            if (_token.Type != type)
            {
                Raise(error);
            }
        }

        protected int Peek()
        {
            var t = _tokenizer.NextToken();

            _tokenizer.UngetToken(t);

            return t.Type;
        }

        void Initialize([NotNull] string query)
        {
            _tokenizer = new Tokenizer(new QueryTokenBuilder(), query);
            _builder = new QueryBuilder();

            Match();
        }

        void Raise([NotNull] string error)
        {
            throw new ParseException(error + " at position " + _token.Index + ".");
        }
    }
}
