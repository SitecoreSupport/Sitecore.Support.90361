namespace Sitecore.Support.Shell.Applications.ContentEditor
{
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Shell.Applications.ContentEditor;
    using Sitecore.Support.Web.UI.HtmlControls;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Sheer;
    using System;
    public class Date : Input, IContentField
    {
        // Fields
        private Sitecore.Support.Web.UI.HtmlControls.DateTimePicker picker;

        // Methods
        public Date()
        {
            this.Class = "scContentControl";
            base.Change = "#";
            base.Activation = true;
            this.ShowTime = false;
        }

        private void ClearField()
        {
            this.SetRealValue(string.Empty);
        }

        protected virtual string GetCurrentDate()
        {
            return DateUtil.ToIsoDate(DateUtil.ToServerTime(System.DateTime.UtcNow).Date);
        }

        protected override Item GetItem()
        {
            return Client.ContentDatabase.GetItem(this.ItemID);
        }

        public string GetValue()
        {
            string realValue;
            if (!this.IsModified)
            {
                return this.Value;
            }
            if (this.picker == null)
            {
                realValue = this.RealValue;
            }
            else
            {
                realValue = this.IsModified ? this.picker.Value : this.RealValue;
            }
            return DateUtil.IsoDateToUtcIsoDate(realValue);
        }

        public override void HandleMessage(Message message)
        {
            string str;
            Assert.ArgumentNotNull(message, "message");
            base.HandleMessage(message);
            if ((message["id"] == this.ID) && ((str = message.Name) != null))
            {
                if (str == "contentdate:today")
                {
                    this.Today();
                }
                else if (str == "contentdate:clear")
                {
                    this.ClearField();
                }
            }
        }

        protected override bool LoadPostData(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = DateUtil.IsoDateToServerTimeIsoDate(value);
            }
            if (!base.LoadPostData(value))
            {
                return false;
            }
            this.picker.Value = value ?? string.Empty;
            return true;
        }

        protected override void OnInit(EventArgs e)
        {
            this.picker = new Sitecore.Support.Web.UI.HtmlControls.DateTimePicker();
            this.picker.ID = this.ID + "_picker";
            this.Controls.Add(this.picker);
            if (!string.IsNullOrEmpty(this.RealValue))
            {
                this.picker.Value = this.RealValue;
            }
            this.picker.Changed += (param0, param1) => this.SetModified();
            this.picker.ShowTime = this.ShowTime;
            this.picker.Disabled = this.Disabled;
            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            base.ServerProperties["Value"] = base.ServerProperties["Value"];
            base.ServerProperties["RealValue"] = base.ServerProperties["RealValue"];
        }

        protected override void SetModified()
        {
            base.SetModified();
            this.IsModified = true;
            if (base.TrackModified)
            {
                Sitecore.Context.ClientPage.Modified = true;
            }
        }

        protected void SetRealValue(string realvalue)
        {
            realvalue = DateUtil.IsoDateToServerTimeIsoDate(realvalue);
            if (realvalue != this.RealValue)
            {
                this.SetModified();
            }
            this.RealValue = realvalue;
            this.picker.Value = realvalue;
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, "value");
            if (this.Value != value)
            {
                this.SetModified();
            }
            this.Value = value;
            value = DateUtil.IsoDateToServerTimeIsoDate(value);
            this.RealValue = value;
            if (this.picker != null)
            {
                this.picker.Value = DateUtil.IsoDateToServerTimeIsoDate(value);
            }
        }

        private void Today()
        {
            this.SetRealValue(this.GetCurrentDate());
        }

        // Properties
        public bool IsModified
        {
            get
            {
                return Convert.ToBoolean(base.ServerProperties["IsModified"]);
            }
            protected set
            {
                base.ServerProperties["IsModified"] = value;
            }
        }

        public string ItemID
        {
            get
            {
                return base.GetViewStateString("ItemID");
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                base.SetViewStateString("ItemID", value);
            }
        }

        public string RealValue
        {
            get
            {
                return base.GetViewStateString("RealValue");
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                string str = DateUtil.IsoDateToServerTimeIsoDate(value);
                base.SetViewStateString("RealValue", str);
            }
        }

        public bool ShowTime
        {
            get
            {
                return base.GetViewStateBool("Showtime", false);
            }
            set
            {
                base.SetViewStateBool("Showtime", value);
            }
        }
    }


}