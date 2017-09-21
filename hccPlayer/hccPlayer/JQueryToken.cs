using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hccPlayer
{
    enum JQueryTokenType
    {
        Object,
        Function,
        Variable,
        Operator,
        String,
        Number,
        End
    }
    class JQueryToken
    {
        public JQueryTokenType tokenType;
        public string token;
        public string[] returns;
        char delimiter;

        public int argsCount;
        public JQueryToken(string token, JQueryTokenType type)
        {
            this.argsCount = 0;
            this.token = token;
            this.delimiter = '\0';
            this.tokenType = type;
            this.token = token;

        }
        public JQueryToken(string token, char delimiter)
        {
            this.argsCount = 0;
            this.token = token;
            this.delimiter = delimiter;
            if (this.delimiter == '.')
                this.tokenType = JQueryTokenType.Object;
            else if (this.delimiter == '(')
                this.tokenType = JQueryTokenType.Function;
            else if (this.token.StartsWith("\""))
            {
                this.tokenType = JQueryTokenType.String;
                if (this.token.StartsWith("\"") == this.token.EndsWith("\""))
                {
                    this.token = this.token.Substring(1, this.token.Length - 2);
                }
            }
            else if (this.token.StartsWith("'"))
            {
                this.tokenType = JQueryTokenType.String;
                if (this.token.StartsWith("'") == this.token.EndsWith("'"))
                {
                    this.token = this.token.Substring(1, this.token.Length - 2);
                }
            }
            else if (this.delimiter == ')')
                this.tokenType = JQueryTokenType.Number;
            else if (this.delimiter == '\0' && this.token == "")
                this.tokenType = JQueryTokenType.End;

        }
        public override string ToString()
        {
            return this.tokenType.ToString() + ":" + this.token;
        }

    }
}
