namespace Sitecore.Support.Shell.Applications.ContentEditor
{
    using System;
    public class DateTime : Date
    {
        // Methods
        public DateTime()
        {
            base.ShowTime = true;
        }

        protected override string GetCurrentDate()
        {
            return DateUtil.IsoNow;
        }
    }

}