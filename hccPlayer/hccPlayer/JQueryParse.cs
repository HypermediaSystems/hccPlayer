using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hccPlayer
{
    [Flags]
    enum JQueryParseExecuteFlag
    {
        NONE=0,
        DEBUG=1,
        TRACE=2,
        WAIT = 4,
    }
    enum JQueryParseStepReturn
    {
        NONE,
        END,
        CONTINUE,
        ERROR
    }
    class JQueryParse

    {
        int tokenNr = 0;
        List<JQueryToken> tokens;

        JQuery jQuery;
        public string returnString;

        private Boolean stepIsQuery;
        HybridWebView hybridWebView;
        public JQueryParse(HybridWebView hybridWebView, string text)
        {
            this.hybridWebView = hybridWebView;
            tokens = new List<JQueryToken>();

            if (!string.IsNullOrEmpty(text))
            {
                // JQuery.text("acb").css("background-color")
                string token = "";
                for (int i = 0; i < text.Length; i++)
                {
                    if (char.IsWhiteSpace(text[i]))
                    {
                        ;
                    }
                    else if (text[i] == ';')
                    {
                        ;
                    }
                    else if (text[i] == '.')
                    {
                        tokens.Add(new JQueryToken(token, text[i]));
                        token = "";
                    }
                    else if (text[i] == '(')
                    {
                        tokens.Add(new JQueryToken(token, text[i]));
                        token = "";
                    }
                    else if (text[i] == ',')
                    {
                        if (token != "")
                            tokens.Add(new JQueryToken(token, text[i]));
                        token = "";
                    }
                    else if (text[i] == ')')
                    {
                        if (token != "")
                            tokens.Add(new JQueryToken(token, text[i]));
                        token = "";
                        // find the function call
                        for (int j = tokens.Count - 1; j >= 0; j--)
                        {
                            if (tokens[j].tokenType == JQueryTokenType.Function)
                            {
                                tokens[j].argsCount = tokens.Count - 1 - j;
                                break;
                            }
                        }
                    }
                    else if (text[i] == '"' || text[i] == '\'')
                    {
                        char del = text[i];
                        token = token + text[i];
                        while (i < text.Length-1)
                        {
                            i++;
                            token = token + text[i];
                            if (text[i] == del && text[i - 1] != '\\')
                            {
                                break;
                            }
                        }
                        tokens.Add(new JQueryToken(token, text[i]));
                        token = "";
                    }
                    else
                    {
                        token = token + text[i];
                    }
                }
            }
            tokens.Add(new JQueryToken("", '\0'));
        }
        public JQueryParse(HybridWebView hybridWebView, JObject jObject)
        {            
            this.hybridWebView = hybridWebView;
            tokens = new List<JQueryToken>();

            foreach (var x in jObject)
            {
                string name = x.Key;
                if (name == "step")
                {
                    JArray value = (JArray)x.Value;
                    JQueryParseStep( value);
                }
            }
        }
        public void JQueryParseStep( JArray jArray)
        {
            for(int t=0;t<jArray.Count;t++)
            {
                JToken jToken = jArray[t];
                JQueryParseFunction((JObject)jToken);
            }
        }
        public void JQueryParseFunction( JObject jObject)
        {
            string function = "";
            Boolean isQuery = false;
            List<string> args = new List<string>();
            List<string> returns = new List<string>();
            foreach (var x in jObject)
            {
                string name = x.Key;
                if (name == "function")
                {
                    function = (string)x.Value;
                }
                else if (name == "args")
                {
                    JArray jArray = (JArray)x.Value;
                    for (int t = 0; t < jArray.Count; t++)
                    {
                        JToken jToken = jArray[t];
                        if (jToken.Type.ToString() == "String" )
                        {
                            args.Add(jToken.ToString());
                        }
                        else
                        {
                            // ToDo: another function call
                        }
                    }
                }
                else if (name == "returns")
                {
                    JArray jArray = (JArray)x.Value;
                    for (int t = 0; t < jArray.Count; t++)
                    {
                        JToken jToken = jArray[t];
                        if (jToken.Type.ToString() == "String")
                        {
                            returns.Add(jToken.ToString());
                        }
                        else
                        {
                            // ToDo: another function call
                        }
                    }
                }
                else if (name == "isQuery")
                {
                    isQuery = (Boolean)x.Value;
                }
            }
            // add a token
            System.Diagnostics.Debug.WriteLine("JQueryParseFunction " + function + "(" + String.Join(",", args.ToArray()) + ")");
            JQueryToken jqt = new JQueryToken(function, JQueryTokenType.Function);
            jqt.argsCount = args.Count;
            jqt.returns = returns.ToArray();
            tokens.Add(jqt);
            foreach (var arg in args)
            {
                tokens.Add(new JQueryToken(arg, ','));
            }
        }
        public Boolean endOfList()
        {
            return !(this.tokenNr < this.tokens.Count);
        }
        public async Task<JQueryParseStepReturn> execute(JQueryParseExecuteFlag flag, Action<JQueryParseExecuteFlag , string> callback)
        {
            JQueryParseStepReturn stepReturn = JQueryParseStepReturn.NONE;
            while ( this.tokenNr < this.tokens.Count)
            {
                stepReturn = await step(flag, callback);
                if (stepReturn == JQueryParseStepReturn.ERROR)
                    break;
                if ( this.stepIsQuery== true)
                {
                    callback(JQueryParseExecuteFlag.NONE, this.returnString);
                }
            }

            return stepReturn;
        }
        public async Task<JQueryParseStepReturn> step(JQueryParseExecuteFlag flag,  Action<JQueryParseExecuteFlag, string> callback)
        {
            stepIsQuery = false;
            if (this.tokenNr < this.tokens.Count)
            {
                JQueryToken theToken = this.tokens[this.tokenNr];

                System.Diagnostics.Debug.WriteLine(theToken.ToString());
                if(theToken.tokenType == JQueryTokenType.Object)
                {
                    if( this.jQuery == null )
                    {
                        this.jQuery = new JQuery(hybridWebView, this.tokens[this.tokenNr+1].token);
                    }
                }
                else if (theToken.tokenType == JQueryTokenType.Function)
                {
                    returnString = "";
                    switch ( theToken.token)
                    {
                        case "JQuery":
                            if (this.tokenNr < this.tokens.Count - 1)
                            {
                                this.jQuery = new JQuery(hybridWebView, this.tokens[this.tokenNr + 1].token);
                                this.tokenNr += 1;
                            }
                            else
                            {
                                // ERROR: no argument for function JQuery
                                this.tokenNr += 1;
                                return JQueryParseStepReturn.ERROR;
                            }
                            break;
                        case "navigate":
                            hybridWebView.Navigating = true;
                            await hybridWebView.EvaluateJavascript("window.location ='" + this.tokens[this.tokenNr + 1].token + "';");
                            this.tokenNr += 1;
                            // ToDo add timeout
                            while(hybridWebView.Navigating == true)
                            {
                                System.Diagnostics.Debug.WriteLine("navigating ...");
                                await Task.Delay(100);
                            }

                            break;
                        case "wait":
                            if (this.tokens[this.tokenNr + 1].token == "???")
                            {
                                // var answer = await DisplayAlert("Question?", "Would you like to play a game", "Yes", "No");
                                callback(JQueryParseExecuteFlag.WAIT, "???");
                                this.tokenNr += this.tokens[this.tokenNr].argsCount;
                                return JQueryParseStepReturn.CONTINUE;
                            }
                            else
                            {
                                int msec = 500;
                                int.TryParse(this.tokens[this.tokenNr + 1].token, out msec);
                                await Task.Delay(msec);
                            }
                            this.tokenNr += 1;
                            break;
                        case "enum":
                            string enumStr = "";
                            string del = "";
                            for (int a = 0; a < theToken.argsCount; a++)
                            {
                                enumStr += del + this.tokens[this.tokenNr + 1 + a].token;
                                del = ",";
                            }
                            this.tokenNr += theToken.argsCount;
                            returnString = (await this.jQuery.enumerate(enumStr)).ToString();
                            stepIsQuery = true;
                            break;
                        case "echo":

                            string echoStr = "";
                            string echoDel = "";
                            for (int a = 0; a < theToken.argsCount; a++)
                            {
                                echoStr += echoDel + this.tokens[this.tokenNr + 1 + a].token;
                                echoDel = ",";
                            }
                            callback(JQueryParseExecuteFlag.NONE, "echo " + echoStr);
                            break;
                        case "focus":
                            returnString = (await this.jQuery.focus()).ToString();
                            break;
                        case "click":
                            returnString = (await this.jQuery.click()).ToString();
                            break;
                        case "submit":
                            returnString = (await this.jQuery.submit()).ToString();
                            break;
                        case "text":
                            if (theToken.argsCount > 0)
                            {
                                await this.jQuery.text(this.tokens[this.tokenNr + 1].token);
                                this.tokenNr += 1;
                            }
                            else
                            {
                                returnString = (await this.jQuery.text()).ToString();
                                stepIsQuery = true;
                            }
                            break;
                        case "css":
                            if (theToken.argsCount > 1)
                            {
                                await this.jQuery.css(this.tokens[this.tokenNr + 1].token, this.tokens[this.tokenNr + 2].token);
                                this.tokenNr += 2;
                            }
                            else
                            {
                                returnString = (await this.jQuery.css(this.tokens[this.tokenNr + 1].token)).ToString();
                                this.tokenNr += 1;
                                stepIsQuery = true;
                            }
                            break;
                        case "attr":
                            if (theToken.argsCount > 1)
                            {
                                await this.jQuery.attr(this.tokens[this.tokenNr + 1].token, this.tokens[this.tokenNr + 2].token);
                                this.tokenNr += 2;
                            }
                            else
                            {
                                returnString = (await this.jQuery.attr(this.tokens[this.tokenNr + 1].token)).ToString();
                                this.tokenNr += 1;
                                stepIsQuery = true;
                            }
                            break;
                    }
                    callback(JQueryParseExecuteFlag.TRACE, theToken.token + ": " + returnString);

                }
                this.tokenNr++;
                if (this.jQuery != null )
                {
                    if (this.jQuery.hasErr == true )
                        return JQueryParseStepReturn.ERROR;
                    return JQueryParseStepReturn.NONE;
                }
                return JQueryParseStepReturn.NONE;
            }
            return JQueryParseStepReturn.NONE;
        }
        public string errStr
        {
            get {
                if (this.jQuery == null)
                    return "";
                return this.jQuery.errStr;
            }
        }
        public Boolean hasErr
        {
            get {
                if (this.jQuery == null)
                    return false;
                return this.jQuery.hasErr;
            }
        }
    }
   
}
