using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZD.Common;

namespace ZDO.CHSite
{
    public partial class OneResultCtrl : System.Web.UI.UserControl
    {
        private static bool hanim = true;

        private readonly string query;
        private readonly CedictResult res;
        private readonly CedictAnnotation ann;
        private readonly ICedictEntryProvider prov;
        private readonly UiScript script;
        private readonly UiTones tones;
        private readonly bool isMobile;

        public OneResultCtrl()
        { }

        /// <summary>
        /// Ctor: regular lookup result
        /// </summary>
        public OneResultCtrl(CedictResult res, ICedictEntryProvider prov,
            UiScript script, UiTones tones, bool isMobile)
        {
            this.res = res;
            this.prov = prov;
            this.script = script;
            this.tones = tones;
            this.isMobile = isMobile;
        }

        /// <summary>
        /// Ctor: annotated Hanzi
        /// </summary>
        public OneResultCtrl(string query, CedictAnnotation ann, ICedictEntryProvider prov, UiTones tones, bool isMobile)
        {
            this.query = query;
            this.ann = ann;
            this.prov = prov;
            this.tones = tones;
            this.isMobile = isMobile;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void Render(HtmlTextWriter writer)
        {
            EntryRenderer er = new EntryRenderer(res, prov, script, tones, isMobile);
            er.Render(writer);
        }
    }
}