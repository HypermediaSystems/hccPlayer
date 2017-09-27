using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hccPlayer
{
    class JsUtil
    {
        string name;
        string id;
        string jsSelector;
        string jsFunction;
        string jsReturn;
        string jsReturnFunction;

        public Boolean addTryCatch = false;
        public Boolean returnJSON = false;

        public Boolean hasError = false;
        public string errStr;

        public JsUtil()
        {
            this.name = null;
            this.id = null;
            this.jsFunction = "node.click()";
            this.setReturn(false, "node.name");
            this.jsSelector = "";
            this.jsReturnFunction = "";
        }
        public void clear()
        {
            this.jsFunction = "";
            this.jsReturn = "";
            this.jsReturnFunction = "";
        }
        public static JsUtil createByName(string name)
        {
            JsUtil ju = new JsUtil();

            ju.selectByAttribute("name", name);

            return ju;
        }
        public static JsUtil createByNames(string[] names)
        {
            JsUtil ju = new JsUtil();
            ju.jsSelector = "";
            foreach(var n in names)
                ju.addSelectByAttribute("name", n);

            return ju;
        }
        public static JsUtil createById(string name)
        {
            JsUtil ju = new JsUtil();
            ju.selectByAttribute("id", name);

            return ju;
        }
        public static JsUtil createByIds(string[] ids)
        {
            JsUtil ju = new JsUtil();
            ju.jsSelector = "";
            foreach (var n in ids)
                ju.addSelectByAttribute("id", n);

            return ju;
        }
        public void selectByAttribute(string name, string value)
        {
            this.jsSelector = "[" + name + "=\"" + value + "\"]";
        }
        public void addSelectByAttribute(string name, string value)
        {
            if (this.jsSelector != "")
                this.jsSelector += ",";
            this.jsSelector += "[" + name + "=\"" + value + "\"]";
        }
        public void selectByAttribute(string name)
        {
            this.jsSelector = "[" + name + "]";
        }
        public void addSelectByAttribute(string name)
        {
            if (this.jsSelector != "")
                this.jsSelector += ",";
            this.jsSelector += "[" + name + "]";
        }

        public void addSelect(string name)
        {
            if (this.jsSelector != "")
                this.jsSelector += ",";
            this.jsSelector += name;
        }
        /// <summary>
        /// set the expression to be execute for each found node<para/>
        /// node is an element found by document.querySelectorAll
        /// </summary>
        /// <param name="fct"></param>
        public void setFunction(string fct)
        {
            this.jsFunction = "";
            if (!string.IsNullOrEmpty(fct))
            {
                if (this.addTryCatch == true)
                {
                    this.jsFunction += "try{";
                    this.jsFunction += fct;
                    this.jsFunction += "}catch(e){ return  e; }";
                }
                else
                {
                    this.jsFunction += fct;
                }
            }
        }
        public void setReturnFunction(string fct)
        {
            this.jsReturnFunction = "";
            if (!string.IsNullOrEmpty(fct))
            {
                if (this.addTryCatch == true)
                {
                    this.jsReturnFunction += "try{";
                    this.jsReturnFunction += fct;
                    this.jsReturnFunction += "}catch(e){ return  e; }";
                }
                else
                {
                    this.jsReturnFunction += fct;
                }
            }
        }
        public void addFunction(string info, string fct)
        {
            if (this.addTryCatch == true)
            {
                this.jsFunction += "try{";
                this.jsFunction += fct;
                this.jsFunction += "}catch(e){ return " + info + " + ' ' + e; }";
            }
            else
            {
                this.jsFunction += fct;
            }
        }

        /// <summary>
        /// set the argument for document.querySelectorAll()
        /// </summary>
        /// <param name="selector"></param>
        public void setSelector(string selector)
        {
            this.jsSelector = selector;
        }
        /// <summary>
        /// set the expression used to generate a string from a selected node<para/>
        /// node is an element found by document.querySelectorAll
        /// </summary>
        /// <param name="value"></param>
        public void setReturn(Boolean isJSON, string value)
        {
            returnJSON = false;
            if( isJSON == true )
                this.jsReturn = value;
            else
                this.jsReturn = "{\"return\" : \"" + value + "\"}";
        }
        public void setReturnOneOf(string[] fields)
        {
            returnJSON = true;
            this.jsReturnFunction = "var ret='';";
            string del = "";
            foreach (var fld in fields)
            {
                if (fld.StartsWith("css."))
                {
                    string retName = toRetName(fld);
                    this.jsReturnFunction += del + " if ( computedStyle['" + fld.Substring(4) + "']) ret = '\"" + retName + "\":\"' computedStyle['" + fld.Substring(4) + "']\"';";
                }
                else
                {
                    this.jsReturnFunction += del + " if ( node." + fld + ") ret = '\"" + fld + "\":\"' + node." + fld + "+'\"';";
                }
                del = " else ";
            }
            this.jsReturn = "'{\"return\" : \"' + ret + '\" }'";
            this.jsReturn = "{ret}";

            this.jsReturn = "'{' + ret + '}'";
        }
        public void setReturnAllOf(string[] fields)
        {
            returnJSON = true;
            this.jsReturnFunction = "var ret='';var del='';";
            string del = "";
            foreach (var fld in fields)
            {
                if (fld.StartsWith("css."))
                {
                    string retName = toRetName(fld);
                    this.jsReturnFunction += del + " if ( computedStyle['" + fld.Substring(4) + "']) { ret += del + '\"" + retName + "\":\"' computedStyle['" + fld.Substring(4) + "']\"'; del=','}";
                }
                else
                {
                    this.jsReturnFunction += del + " if ( node." + fld + "){ ret += del +'\"" + fld + "\":\"' + node." + fld + "+'\"'; del=','};";
                }
                del = " else ";
            }
            this.jsReturn = "'{\"return\" : \"' + ret + '\" }'";
            this.jsReturn = "ret";

            this.jsReturn = "JSON.stringify( { ret })";
        }
        /// <summary>
        /// set the fields to be returned
        /// </summary>
        /// <param name="fields"></param>
        public void setJSONReturn(string[] fields)
        {
            returnJSON = true;
            string str = "";
            string del = "";
            Boolean returnStyle = false;
            this.jsReturnFunction = "var computedStyle = window.getComputedStyle(node, null);";

            foreach (var fld in fields)
            {
                if (fld.StartsWith("css.") )
                {
                    string retName = toRetName(fld);
                    str += del + retName + ": computedStyle['" + fld.Substring(4) + "']";
                    del = ",";
                    returnStyle = true;
                }
                else
                {
                    str += del + fld + ": node." + fld;
                    del = ",";
                }
            }

            if (returnStyle == false)
            {
                this.jsReturnFunction = "";
            }
            // 
            this.jsReturn = "JSON.stringify({" +  str + "})";
        }
        public void queryStyle()
        {
            string qsCmd = "var computedStyle = window.getComputedStyle(node, null);";
            if (!this.jsFunction.Contains(qsCmd))
            {
                this.jsFunction = qsCmd + this.jsFunction;
            }
        }
        public string toRetName(string str)
        {
            string retName = str.Replace(".", "_");
            retName = retName.Replace("-", "_");

            return retName;
        }
        public async Task<string> execute(HybridWebView hybridWebView)
        {
            hybridWebView.Focus();
            string jsCommand = this.ToString();

            // Task<string> ret;
            string ret;
            try
            {
                ret = await  hybridWebView.EvaluateJavascript(jsCommand);
            }
            catch (Exception ex)
            {
                this.hasError = true;
                this.errStr = ex.Message;
                ret = this.errStr; // Task.FromResult();
            }

            return ret;
        }
        public async Task<JArray> executeJSON(HybridWebView hybridWebView)
        {
            this.returnJSON = true;
            string jsCommand = this.ToString();
            string jsResult = "";

            try
            {
                jsResult = await hybridWebView.EvaluateJavascript(jsCommand);
            }
            catch (Exception ex)
            {
                this.hasError = true;
                this.errStr = ex.Message;
            }

            if (!string.IsNullOrEmpty(jsResult) &&  !jsResult.StartsWith("{"))
            {
                // unescape the quotes and backslashes
                jsResult = jsResult.Replace("\\\\", "\\");
                jsResult = jsResult.Replace("\\\"", "\"");
                // remove the leading and trailing quotes
                jsResult = jsResult.Substring(1, jsResult.Length - 2);
            }
            // return an array 
            if ( !jsResult.StartsWith("["))
                return JArray.Parse("[" + jsResult + "]");
            else
                return JArray.Parse(jsResult);
        }

        public override string ToString()
        {
            string joinDel = "||";
            if( this.returnJSON == true )
            {
                 joinDel = ",";
            }
            string ret = "[].map.call(document.querySelectorAll('" + jsSelector + "'), ";
            ret += "function(node){ ";
            if (!string.IsNullOrEmpty(this.jsFunction))
            {
                ret += this.jsFunction + "; ";
            }
            if (!string.IsNullOrEmpty(this.jsReturnFunction))
            {
                if (this.addTryCatch == true)
                {
                    ret += "try{";
                    ret += this.jsReturnFunction + ";";
                    ret += "}catch(e){ return jsReturnFunction + ' ' + e; }";
                }
                else
                {
                    ret += this.jsReturnFunction + ";";
                }
            }
            if (this.addTryCatch == true)
            {
                ret += "try{";
                ret += " return " + this.jsReturn + "; ";
                ret += "}catch(e){ return jsReturnFunction + ' ' + e; }";
            }
            else
            {
                ret += " return " + this.jsReturn + "; ";
            }

            ret += " }).join('" + joinDel + "');";

            
            return ret;
        }
    }
}
