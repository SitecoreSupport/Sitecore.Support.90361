namespace Sitecore.Support.Shell.Applications.ContentManager.Dialogs.SetPublishing
{
    using Sitecore.Data;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Support.Web.UI.HtmlControls;
    using Sitecore.Text;
    using Sitecore.Web;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Pages;
    using Sitecore.Web.UI.Sheer;
    using System;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    public class SetPublishingForm : DialogForm
    {
        protected Checkbox NeverPublish;

        protected Sitecore.Support.Web.UI.HtmlControls.DateTimePicker Publish;

        protected Border PublishingTargets;

        protected Border PublishPanel;

        protected Sitecore.Support.Web.UI.HtmlControls.DateTimePicker Unpublish;

        protected Border Versions;

        protected Border Warning;

        private bool ReadOnly
        {
            get
            {
                return MainUtil.GetBool(base.ServerProperties["ReadOnly"], false);
            }
            set
            {
                base.ServerProperties["ReadOnly"] = value;
            }
        }

        private void ChangeDateTimePickerState(Sitecore.Web.UI.HtmlControls.Control parent, string controlId, bool state)
        {
            Sitecore.Support.Web.UI.HtmlControls.DateTimePicker supportDateTimePicker = parent.FindControl(controlId) as Sitecore.Support.Web.UI.HtmlControls.DateTimePicker;
            bool flag = supportDateTimePicker != null;
            if (flag)
            {
                supportDateTimePicker.Disabled = state;
                Context.ClientPage.ClientResponse.Refresh(supportDateTimePicker);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            bool flag = !Context.ClientPage.IsEvent;
            if (flag)
            {
                bool flag2 = WebUtil.GetQueryString("ro") == "1";
                if (flag2)
                {
                    this.ReadOnly = true;
                }
                Item itemFromQueryString = UIUtil.GetItemFromQueryString(Context.ContentDatabase);
                Error.AssertItemFound(itemFromQueryString);
                this.RenderItemTab(itemFromQueryString);
                this.RenderVersions(itemFromQueryString);
                this.RenderTargetTab(itemFromQueryString);
                bool readOnly = this.ReadOnly;
                if (readOnly)
                {
                    this.SetReadonly();
                }
            }
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            Item itemFromQueryString = UIUtil.GetItemFromQueryString(Context.ContentDatabase);
            Error.AssertItemFound(itemFromQueryString);
            ListString listString = new ListString();
            using (new StatisticDisabler(StatisticDisablerState.ForItemsWithoutVersionOnly))
            {
                itemFromQueryString.Editing.BeginEdit();
                itemFromQueryString.Publishing.NeverPublish = !this.NeverPublish.Checked;
                itemFromQueryString.Publishing.PublishDate = DateUtil.ParseDateTime(this.Publish.Value, DateTime.MinValue);
                itemFromQueryString.Publishing.UnpublishDate = DateUtil.ParseDateTime(this.Unpublish.Value, DateTime.MaxValue);
                foreach (string text in Context.ClientPage.ClientRequest.Form.Keys)
                {
                    bool flag = text != null && text.StartsWith("pb_", StringComparison.InvariantCulture);
                    if (flag)
                    {
                        string value = ShortID.Decode(StringUtil.Mid(text, 3));
                        listString.Add(value);
                    }
                }
                itemFromQueryString[FieldIDs.PublishingTargets] = listString.ToString();
                itemFromQueryString.Editing.EndEdit();
            }
            Log.Audit(this, "Set publishing targets: {0}, targets: {1}", new string[]
            {
                AuditFormatter.FormatItem(itemFromQueryString),
                listString.ToString()
            });
            foreach (string text2 in Context.ClientPage.ClientRequest.Form.Keys)
            {
                bool flag2 = text2 != null && text2.StartsWith("pb_", StringComparison.InvariantCulture);
                if (flag2)
                {
                    string value2 = ShortID.Decode(StringUtil.Mid(text2, 3));
                    listString.Add(value2);
                }
            }
            Item[] versions = itemFromQueryString.Versions.GetVersions();
            for (int i = 0; i < versions.Length; i++)
            {
                Item item = versions[i];
                bool flag3 = StringUtil.GetString(new string[]
                {
                    Context.ClientPage.ClientRequest.Form["hide_" + item.Version.Number]
                }).Length <= 0;
                Sitecore.Support.Web.UI.HtmlControls.DateTimePicker supportDateTimePicker = this.Versions.FindControl("validfrom_" + item.Version.Number) as Sitecore.Support.Web.UI.HtmlControls.DateTimePicker;
                Sitecore.Support.Web.UI.HtmlControls.DateTimePicker supportDateTimePicker2 = this.Versions.FindControl("validto_" + item.Version.Number) as Sitecore.Support.Web.UI.HtmlControls.DateTimePicker;
                Assert.IsNotNull(supportDateTimePicker, "Version valid from datetime picker");
                Assert.IsNotNull(supportDateTimePicker2, "Version valid to datetime picker");
                DateTime dateTime = DateUtil.IsoDateToDateTime(supportDateTimePicker.Value, DateTime.MinValue);
                DateTime dateTime2 = DateUtil.IsoDateToDateTime(supportDateTimePicker2.Value, DateTime.MaxValue);
                bool flag4 = flag3 != item.Publishing.HideVersion || DateUtil.CompareDatesIgnoringSeconds(dateTime, item.Publishing.ValidFrom) != 0 || DateUtil.CompareDatesIgnoringSeconds(dateTime2, item.Publishing.ValidTo) != 0;
                if (flag4)
                {
                    item.Editing.BeginEdit();
                    item.Publishing.ValidFrom = dateTime;
                    item.Publishing.ValidTo = dateTime2;
                    item.Publishing.HideVersion = flag3;
                    item.Editing.EndEdit();
                    Log.Audit(this, "Set publishing valid: {0}, from: {1}, to:{2}, hide: {3}", new string[]
                    {
                        AuditFormatter.FormatItem(item),
                        dateTime.ToString(),
                        dateTime2.ToString(),
                        MainUtil.BoolToString(flag3)
                    });
                }
            }
            SheerResponse.SetDialogValue("yes");
            base.OnOK(sender, args);
        }

        private void RenderItemTab(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            this.NeverPublish.Checked = !item.Publishing.NeverPublish;
            this.Publish.Value = ((item.Publishing.PublishDate == DateTimeOffset.MinValue.UtcDateTime) ? string.Empty : DateUtil.ToIsoDate(DateUtil.ToServerTime(item.Publishing.PublishDate)));
            this.Unpublish.Value = ((item.Publishing.UnpublishDate == DateTimeOffset.MaxValue.UtcDateTime) ? string.Empty : DateUtil.ToIsoDate(DateUtil.ToServerTime(item.Publishing.UnpublishDate)));
            this.SetNeverPublish();
        }

        private void RenderTargetTab(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            Field field = item.Fields[FieldIDs.PublishingTargets];
            bool flag = field != null;
            if (flag)
            {
                Item item2 = Context.ContentDatabase.Items["/sitecore/system/publishing targets"];
                bool flag2 = item2 != null;
                if (flag2)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    string value = field.Value;
                    foreach (Item item3 in item2.Children)
                    {
                        string text = (value.IndexOf(item3.ID.ToString(), StringComparison.InvariantCulture) >= 0) ? " checked=\"true\"" : string.Empty;
                        string text2 = string.Empty;
                        bool readOnly = this.ReadOnly;
                        if (readOnly)
                        {
                            text2 = " disabled=\"true\"";
                        }
                        stringBuilder.Append(string.Concat(new string[]
                        {
                            "<input id=\"pb_",
                            ShortID.Encode(item3.ID),
                            "\" name=\"pb_",
                            ShortID.Encode(item3.ID),
                            "\" class=\"scRibbonCheckbox\" type=\"checkbox\"",
                            text,
                            text2,
                            " style=\"vertical-align:middle\"/>"
                        }));
                        stringBuilder.Append(item3.DisplayName);
                        stringBuilder.Append("<br/>");
                    }
                    this.PublishingTargets.InnerHtml = stringBuilder.ToString();
                }
            }
        }

        private void RenderVersions(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            Item[] versions = item.Versions.GetVersions();
            StringBuilder stringBuilder = new StringBuilder("<table class='scListControl scVersionsTable'>");
            stringBuilder.Append("<tr>");
            stringBuilder.Append("<td nowrap=\"nowrap\"><b>" + Translate.Text("Version") + "</b></td>");
            stringBuilder.Append("<td nowrap=\"nowrap\"><b>" + Translate.Text("Publishable") + "</b></td>");
            stringBuilder.Append("<td width=\"50%\"><b>" + Translate.Text("Publishable from") + "</b></td>");
            stringBuilder.Append("<td width=\"50%\"><b>" + Translate.Text("Publishable to") + "</b></td>");
            stringBuilder.Append("</tr>");
            this.Versions.Controls.Add(new LiteralControl(stringBuilder.ToString()));
            string text = string.Empty;
            bool flag = item.Publishing.NeverPublish || this.ReadOnly || !item.Access.CanWriteLanguage() || !this.NeverPublish.Checked;
            bool flag2 = flag;
            if (flag2)
            {
                text = " disabled=\"true\"";
            }
            for (int i = 0; i < versions.Length; i++)
            {
                stringBuilder = new StringBuilder();
                Item item2 = versions[i];
                string text2 = item2.Version.Number.ToString();
                stringBuilder.Append("<tr" + ((item2.Version == item.Version) ? " style=\"background-color:#D0EBF6\"" : string.Empty) + ">");
                stringBuilder.AppendFormat("<td class='scVersionNumber'><b>{0}.</b></td>", item2.Version.Number);
                stringBuilder.AppendFormat(string.Concat(new string[]
                {
                    "<td class='scPublishable'><input id=\"hide_",
                    text2,
                    "\" type=\"checkbox\"",
                    item2.Publishing.HideVersion ? string.Empty : " checked=\"checked\"",
                    text,
                    "/></td>"
                }), new object[0]);
                stringBuilder.Append("<td>");
                this.Versions.Controls.Add(new LiteralControl(stringBuilder.ToString()));
                Sitecore.Support.Web.UI.HtmlControls.DateTimePicker supportDateTimePicker = new Sitecore.Support.Web.UI.HtmlControls.DateTimePicker
                {
                    ID = "validfrom_" + text2,
                    Width = new Unit(100.0, UnitType.Percentage),
                    Value = ((item2.Publishing.ValidFrom == DateTime.MinValue) ? string.Empty : DateUtil.ToIsoDate(DateUtil.ToServerTime(item2.Publishing.ValidFrom)))
                };
                this.Versions.Controls.Add(supportDateTimePicker);
                this.Versions.Controls.Add(new LiteralControl("</td><td>"));
                Sitecore.Support.Web.UI.HtmlControls.DateTimePicker supportDateTimePicker2 = new Sitecore.Support.Web.UI.HtmlControls.DateTimePicker
                {
                    ID = "validto_" + text2,
                    Width = new Unit(100.0, UnitType.Percentage),
                    Value = ((item2.Publishing.ValidTo == DateTime.MaxValue) ? string.Empty : DateUtil.ToIsoDate(DateUtil.ToServerTime(item2.Publishing.ValidTo)))
                };
                this.Versions.Controls.Add(supportDateTimePicker2);
                bool flag3 = flag;
                if (flag3)
                {
                    supportDateTimePicker2.Disabled = true;
                    supportDateTimePicker.Disabled = true;
                }
                this.Versions.Controls.Add(new LiteralControl("</td></tr>"));
            }
            this.Versions.Controls.Add(new LiteralControl("</table>"));
        }

        protected void SetNeverPublish()
        {
            bool flag = !this.NeverPublish.Checked;
            this.Publish.Disabled = flag;
            this.Unpublish.Disabled = flag;
            Item itemFromQueryString = UIUtil.GetItemFromQueryString(Context.ContentDatabase);
            Error.AssertItemFound(itemFromQueryString);
            bool isEvent = Context.ClientPage.IsEvent;
            if (isEvent)
            {
                Item[] versions = itemFromQueryString.Versions.GetVersions();
                for (int i = 0; i < versions.Length; i++)
                {
                    Item item = versions[i];
                    this.UpdateVersionState(item.Version.Number, flag);
                }
            }
            Context.ClientPage.ClientResponse.Refresh(this.Publish);
            Context.ClientPage.ClientResponse.Refresh(this.Unpublish);
        }

        private void SetReadonly()
        {
            this.ReadOnly = true;
            this.NeverPublish.Disabled = true;
            this.Publish.Disabled = true;
            this.Unpublish.Disabled = true;
            this.Warning.Attributes.Remove("hidden");
        }

        private void UpdateVersionState(int versionNumber, bool isDisabled)
        {
            string value = isDisabled ? "true" : "false";
            SheerResponse.SetAttribute("hide_" + versionNumber, "disabled", value);
            this.ChangeDateTimePickerState(this.Versions, "validto_" + versionNumber, isDisabled);
            this.ChangeDateTimePickerState(this.Versions, "validfrom_" + versionNumber, isDisabled);
        }
    }
}