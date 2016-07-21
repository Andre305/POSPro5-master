using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule.BrowserViews
{
    class SambaPosDatabaseSupport : VisibleViewModelBase
    {
        public SambaPosDatabaseSupport()
        {

        }

        protected override string GetHeaderInfo()
        {
            return Resources.DatabaseSupport;
        }

        public override Type GetViewType()
        {
            return typeof(DatabaseSupportView);
        }
    }
}
