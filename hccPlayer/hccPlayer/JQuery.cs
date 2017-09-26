using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hccPlayer
{
    class JQuery
    {
        private HybridWebView hybridWebView;
        private JsUtil ju;
        // JQuery("#btnK,#btnG").css("background-color","red");
        public JQuery(HybridWebView hybridWebView,string selector)
        {
            this.hybridWebView = hybridWebView;
            // remove surroundig quotes
            if(selector.StartsWith("\"") == true && selector.EndsWith("\"") == true)
            {
                selector = selector.Substring(1, selector.Length - 2);
            }
            ju = new JsUtil();
            // ToDo what about colon inside a selector?
            string[] parts = selector.Split(',');

            foreach (var p in parts)
            {
                string useP = p.Trim();
                // ToDo: what about classes, ... ?
                if (useP.StartsWith("#"))
                {
                    ju.addSelectByAttribute("id", useP.Substring(1));
                }
                else if (useP.StartsWith("@"))
                {
                    ju.addSelectByAttribute(useP.Substring(1));
                }

                else if (useP.StartsWith("."))
                {
                    ju.addSelectByAttribute("class*", useP.Substring(1));
                }
                else
                {
                    ju.addSelect(useP);

                }
            }
           

        }
        public async Task<JArray> css(string cssName, string cssValue)
        {
            this.ju.clear();
            // replace the - sign/lower letter            
            cssName = this.prepareCssName(cssName);

            this.ju.setFunction("node.style." + cssName + "='" + cssValue + "';");

            this.ju.setReturnOneOf(new string[] { "name", "id" });
            return await this.ju.executeJSON(this.hybridWebView);

        }
        public async Task<JArray> css(string cssName)
        {
            this.ju.clear();
            if (cssName == "*")
            {
                this.ju.setFunction("var ret = '{'; var del='';" +
"for (var name in computedStyle) {" +
// "  if (computedStyle.hasOwnProperty(name)) {" +
"    ret += del + '\"' + name + '\": \"' +  computedStyle[name] + '\"';" +
"    del = ','; "+
// "  }" +
"}" +
"ret += '}';");
                this.ju.setReturn(true, "ret");
            }
            else
            {
                cssName = this.prepareCssName(cssName);
                this.ju.setReturn(true, "'{ \"" + cssName + "\" : \"' + computedStyle." + cssName + " + '\"}'");
            }
            this.ju.queryStyle();
            this.ju.returnJSON = true;
            return await this.ju.executeJSON(this.hybridWebView);
        }
        public async Task<string> cssStr(string cssName)
        {
            this.ju.clear();
            if (cssName == "*")
            {
                this.ju.setFunction("var ret = '{';" +
"for (var name in computedStyle) {" +
// "  if (computedStyle.hasOwnProperty(name)) {" +
"    ret += name + ' ' +  computedStyle[name];" +
// "  }" +
"}" +
"ret += '}';");
                this.ju.setReturn(true, "ret");
            }
            else
            {
                cssName = this.prepareCssName(cssName);
                this.ju.setReturn(false, "computedStyle." + cssName + ";");
            }
            this.ju.queryStyle();
            return await this.ju.execute(this.hybridWebView);
        }

        public async Task<JArray> attr(string attrName, string attrValue)
        {
            this.ju.clear();

            // ToDo attribute with namespace
            this.ju.setFunction("node.setAttribute('" + attrName + "','" + attrValue + "');");
            this.ju.setReturnOneOf(new string[] { "name", "id" });
            return await this.ju.executeJSON(this.hybridWebView);

            // await this.ju.execute(this.hybridWebView);
            // return this;
        }
        public async Task<JArray> attr(string attrName)
        {
            this.ju.clear();

            // ToDo attribute with namespace
            this.ju.setFunction(
                "if( !node.hasAttribute ) return 'not node.hasAttribute';"+
                "if( !node.hasAttribute('" + attrName + "'))  return '{\"" + attrName + "\":\"\"}'; "
                );

            this.ju.setReturn(true, "'{\"" + attrName + "\":\"' + node.getAttribute('" + attrName + "') + '\" }'");

            return await this.ju.executeJSON(this.hybridWebView);
        }


        public async Task<JArray> enumerate(string enumList)
        {
            string[] parts = enumList.Split(',');

            this.ju.clear();
            ju.setJSONReturn(parts); // new string[] { "name", "id", "css.line-height" });
            return await ju.executeJSON(hybridWebView);

        }
        public async Task<string> enumerateStr(string enumList)
        {
            string[] parts = enumList.Split(',');

            this.ju.clear();
            ju.setJSONReturn(parts); // new string[] { "name", "id", "css.line-height" });
            JArray jArray = await ju.executeJSON(hybridWebView);
            string ret = "";
            foreach (var d in jArray)
            {
                // ret += d["name"] + "-" + d["id"] + "-" + d[ju.toRetName("css.line-height")] + Environment.NewLine;
                for (int p = 0; p < parts.Length; p++)
                {
                    ret += d[ju.toRetName(parts[p])] + "/-/";
                }
                ret += Environment.NewLine;
            }
            return ret;
        }
        public async Task<JArray> text(string newtext)
        {
            this.ju.clear();
            this.ju.setFunction("node.focus(); node.value = '" + newtext + "'; node.focus(); "); // node.click();

            this.ju.setReturnOneOf(new string[] { "name", "id" });
            return await this.ju.executeJSON(this.hybridWebView);

            // return await this.ju.execute(this.hybridWebView);
        }
        public async Task<JArray> text()
        {
            this.ju.clear();
            this.ju.setJSONReturn(new string[] { "node.value" });
            return await this.ju.executeJSON(this.hybridWebView);
        }
        public async Task<JArray> focus()
        {
            this.ju.clear();
            ju.setFunction("node.focus();  ");
            // this.ju.setReturn(false, "node.value");
            // return await this.ju.execute(this.hybridWebView);
            this.ju.setReturnOneOf(new string[] { "name", "id" });
            return await this.ju.executeJSON(this.hybridWebView);

        }
        public async Task<JArray> click()
        {
            this.ju.clear();
            ju.setFunction("node.click(); ");
            this.ju.setReturnOneOf(new string[] { "name", "id" });
            return await this.ju.executeJSON(this.hybridWebView);
        }
        public async Task<JArray> submit()
        {
            this.ju.clear();
            ju.setFunction("if( !node.closest){ return '!node.closest'} var frm = node.closest('form'); if( frm ){ frm.submit(); }else{ return 'frm is null';}");
            // this.ju.setReturn(false, "node.value");
            // return await this.ju.execute(this.hybridWebView);
            this.ju.setReturnOneOf(new string[] { "name", "id" });
            return await this.ju.executeJSON(this.hybridWebView);

        }
        /// <summary>
        /// replace the -sign/lower letter   
        /// </summary>
        /// <param name="cssName"></param>
        /// <returns></returns>
        private string prepareCssName(string cssName)
        {
            string[] parts = cssName.Split('-');

            for (int p = 1; p < parts.Length; p++)
            {
                parts[p] = char.ToUpper(parts[p][0]) + parts[p].Substring(1);
            }
            return String.Join("", parts);
        }

        public string errStr
        {
            get { return this.ju.errStr; }
        }
        public Boolean hasErr
        {
            get { return this.ju.hasError; }
        }


    }
}
