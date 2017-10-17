using Sitecore.Diagnostics;
using Sitecore.Web.UI.HtmlControls;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Sitecore.Support.Web.UI.HtmlControls
{
    public class DateTimePicker : Border
    {
        private DatePicker date;

        private System.Web.UI.WebControls.Label dateLabel;

        private TimePicker time;

        private System.Web.UI.WebControls.Label timeLabel;

        public event EventHandler Changed;

        public string Date
        {
            get
            {
                return this.date.Value;
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.date.Value = value;
            }
        }

        public string DateLabel
        {
            get
            {
                return this.dateLabel.Text;
            }
            set
            {
                this.dateLabel.Text = value;
            }
        }

        public bool ShowLabels
        {
            get;
            set;
        }

        public bool ShowTime
        {
            get
            {
                return base.GetViewStateBool("Showtime", true);
            }
            set
            {
                base.SetViewStateBool("Showtime", value);
            }
        }

        public string Time
        {
            get
            {
				// Modified Code Begin
                return this.time.Value;
				// Modified Code End
			}
			set
            {
                this.time.Value = value;
            }
        }

        public string TimeLabel
        {
            get
            {
                return this.timeLabel.Text;
            }
            set
            {
                this.timeLabel.Text = value;
            }
        }

		//Modified Code begin
        public override string Value
        {
            get
            {
                bool flag = this.date == null || this.time == null;
                string result;
                if (flag)
                {
                    result = base.Value;
                }
                else
                {
                    bool flag2 = this.Date.Length == 0 && this.Time.Length == 0;
                    if (flag2)
                    {
                        result = string.Empty;
                    }
                    else
                    {
                        bool flag3 = this.Date.StartsWith("$", StringComparison.InvariantCulture);
                        if (flag3)
                        {
                            result = this.Date;
                        }
                        else
                        {
                            bool flag4 = this.Date.Length != 0;
                            DateTime dateTime;
                            if (flag4)
                            {
                                dateTime = DateUtil.IsoDateToDateTime(this.Date);
                            }
                            else
                            {
                                dateTime = DateUtil.ToServerTime(DateTime.UtcNow).Date;
                            }
                            TimeSpan value = DateUtil.ParseTimeSpan(this.time.Value, TimeSpan.Zero);
                            result = DateUtil.ToIsoDate(dateTime.Date.Add(value));
                        }
                    }
                }
                return result;
            }
            set
            {
                bool flag = DateUtil.IsIsoDateUtc(value);
                if (flag)
                {
                    throw new ArgumentException("UTC date is not supported.");
                }
                bool flag2 = this.date != null && this.time != null;
                if (flag2)
                {
                    this.SetValue(value);
                }
                else
                {
                    base.Value = value;
                }
            }
        }
		//Modified Code begin

		protected override void DoRender(HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, "output");
            output.Write("<div ID=\"" + this.ID + "\" class=\"scDateTimePickerContainer\">");
            bool flag = !this.ShowTime;
            if (flag)
            {
                this.time.Visible = false;
            }
            this.date.Disabled = this.Disabled;
            this.time.Disabled = this.Disabled;
            bool showLabels = this.ShowLabels;
            if (showLabels)
            {
                output.Write("<table><tr><td>");
                this.dateLabel.RenderControl(output);
                output.Write("</td><td>");
            }
            this.date.RenderControl(output);
            new Space("8px", "1px").RenderControl(output);
            bool showLabels2 = this.ShowLabels;
            if (showLabels2)
            {
                output.Write("</td><td>");
                this.timeLabel.RenderControl(output);
                output.Write("</td><td>");
            }
            this.time.RenderControl(output);
            bool showLabels3 = this.ShowLabels;
            if (showLabels3)
            {
                output.Write("</td></tr></table>");
            }
            output.Write("</div>");
        }

        protected void OnChanged()
        {
            bool flag = this.Changed != null;
            if (flag)
            {
                this.Changed(this, EventArgs.Empty);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            this.date = new DatePicker();
            this.Controls.Add(this.date);
            this.date.ID = this.ID + "_date";
            this.date.Width = Unit.Pixel(120);
            this.time = new TimePicker();
            this.Controls.Add(this.time);
            this.time.ID = this.ID + "_time";
            this.date.OnChanged += delegate (object param0, EventArgs param1)
            {
                this.OnChanged();
            };
            this.time.OnChanged += delegate (object param0, EventArgs param1)
            {
                this.OnChanged();
            };
            bool flag = !string.IsNullOrEmpty(base.Value);
            if (flag)
            {
                this.SetValue(base.Value);
            }
            this.dateLabel = new System.Web.UI.WebControls.Label();
            this.timeLabel = new System.Web.UI.WebControls.Label();
            base.OnInit(e);
        }

        private void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, "value");
            bool flag = value.Length == 0;
            if (flag)
            {
                this.Date = string.Empty;
                this.Time = string.Empty;
            }
            else
            {
                bool flag2 = value.StartsWith("$", StringComparison.InvariantCulture);
                if (flag2)
                {
                    this.Date = value;
                    this.Time = string.Empty;
                }
                else
                {
                    DateTime datetime = DateUtil.IsoDateToDateTime(value);
                    this.Date = DateUtil.ToIsoDate(datetime.Date);
                    this.Time = DateUtil.ToIsoTime(datetime);
                }
            }
        }
    }
}
