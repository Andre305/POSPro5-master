using System;
using System.Linq;
using Samba.Localization.Properties;

namespace Samba.Modules.SettingsModule.BrowserViews
{
    class SambaPosWiki : BrowserViewModel
    {
        public SambaPosWiki()
        {
            Header = "Remote Support";
            Url = "https://hattech.screenconnect.com/";
        }
    }
}
