using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Sitecore.Support.Web.UI.HtmlControls
{
    public class TimePicker : ComboboxBase
    {
        private EventHandler POnChanged;

        public event EventHandler OnChanged;

        public string Format
        {
            get
            {
                return base.GetViewStateString("Format", "t");
            }
            set
            {
                base.SetViewStateString("Format", value, "t");
            }
        }

        public TimePicker()
        {
            this.Class += " scTimePickerComboBox";
            this.Width = Unit.Pixel(110);
        }

        protected override void Changed()
        {
            bool flag = !this.Disabled;
            if (flag)
            {
                string @string = StringUtil.GetString(new string[]
                {
                    Sitecore.Context.ClientPage.ClientRequest.Form[this.ID]
                });
                bool flag2 = this.Value.Length == 0;
                if (flag2)
                {
                    SheerResponse.SetReturnValue(true);
                    this.DoChanged();
                }
                else
                {
                    bool flag3 = this.SetTime(@string);
                    if (flag3)
                    {
                        SheerResponse.SetAttribute(this.ID + "_edit", "value", this.GetDisplayValue());
                        SheerResponse.SetReturnValue(true);
                        this.DoChanged();
                    }
                    else
                    {
                        SheerResponse.Alert(Translate.Text("\"{0}\" is not a valid time.", new object[]
                        {
                            @string
                        }), new string[0]);
                    }
                }
            }
        }

        private void DoChanged()
        {
            bool flag = this.POnChanged != null;
            if (flag)
            {
                this.POnChanged(this, EventArgs.Empty);
            }
        }

        protected override void DropDown()
        {
            System.Web.UI.Control hiddenHolder = UIUtil.GetHiddenHolder(this);
            TimePickerPopup timePickerPopup = new TimePickerPopup();
            hiddenHolder.Controls.Add(timePickerPopup);
            timePickerPopup.ID = this.ID + "_timepicker";
            timePickerPopup.SendOnClose = "datepicker:select(time=1)";
            timePickerPopup.Format = this.Format;
            bool flag = this.Value.Length > 0;
            if (flag)
            {
                timePickerPopup.Time = DateUtil.IsoDateToDateTime(this.Value).TimeOfDay;
            }
            SheerResponse.ShowPopup(this.ID, "below-right", timePickerPopup);
        }

        protected override string GetDisplayValue()
        {
            bool flag = this.Value.Length == 0;
            string result;
            if (flag)
            {
                result = string.Empty;
            }
            else
            {
                bool flag2 = this.Value.StartsWith("$");
                if (flag2)
                {
                    result = this.Value;
                }
                else
                {
                    TimeSpan t = DateUtil.ParseTimeSpan(this.Value, TimeSpan.Zero);
                    CultureInfo cultureInfo = Sitecore.Context.Culture;
                    bool isNeutralCulture = cultureInfo.IsNeutralCulture;
                    if (isNeutralCulture)
                    {
                        cultureInfo = Language.CreateSpecificCulture(cultureInfo.Name);
                    }
                    result = (DateTime.MinValue + t).ToString(this.Format, cultureInfo);
                }
            }
            return result;
        }

        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            base.HandleMessage(message);
            bool flag = message.Name == "datepicker:select" && (message.Sender as System.Web.UI.Control).ID == this.ID + "_timepicker";
            if (flag)
            {
                this.UpdateFromCalendar(message.Sender as TimePickerPopup);
            }
        }

        private bool SetTime(string value)
        {
            bool flag = value.Length == 0;
            bool result;
            if (flag)
            {
                this.Value = string.Empty;
                result = true;
            }
            else
            {
                bool flag2 = value.StartsWith("$");
                if (flag2)
                {
                    this.Value = value;
                    result = true;
                }
                else
                {
                    TimeSpan t = DateUtil.ParseTimeSpan(value, TimeSpan.Zero);
                    bool flag3 = t != TimeSpan.Zero || t == TimeSpan.Zero;
                    if (flag3)
                    {
                        this.Value = t.ToString();
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        private void UpdateFromCalendar(TimePickerPopup calendar)
        {
            bool flag = this.Value != calendar.Value;
            if (flag)
            {
                this.Value = calendar.Value;
                SheerResponse.SetAttribute(this.ID + "_edit", "value", this.GetDisplayValue());
                SheerResponse.Focus(this.ID + "_edit");
                this.DoChanged();
            }
        }
    }
}
