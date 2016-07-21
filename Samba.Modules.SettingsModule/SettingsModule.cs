using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Modules.SettingsModule.BrowserViews;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.SettingsModule
{
    [ModuleExport(typeof(SettingsModule))]
    public class SettingsModule : ModuleBase
    {
        [ImportingConstructor]
        public SettingsModule()
        {
            AddDashboardCommand<SettingsViewModel>(Resources.LocalSettings, Resources.Settings, 20);
            AddDashboardCommand<TerminalListViewModel>(Resources.Terminals, Resources.Settings, 21);
            AddDashboardCommand<EntityCollectionViewModelBase<NumeratorViewModel, Numerator>>(Resources.Numerators, Resources.Settings, 21);
            AddDashboardCommand<EntityCollectionViewModelBase<ForeignCurrencyViewModel, ForeignCurrency>>(string.Format(Resources.List_f, Resources.Currency), Resources.Settings, 21);
            AddDashboardCommand<EntityCollectionViewModelBase<StateViewModel, State>>(Resources.State.ToPlural(), Resources.Settings, 21);
            AddDashboardCommand<ProgramSettingsViewModel>(Resources.ProgramSettings, Resources.Settings, 22);
            AddDashboardCommand<SambaPosWebsite>(Resources.HattechPosWebsite, Resources.HattechNetwork, 90);
            AddDashboardCommand<SambaPosDocumentation>("How to files", Resources.HattechNetwork, 91);
            AddDashboardCommand<SambaPosForum>("How to Videos", Resources.HattechNetwork, 92);
            AddDashboardCommand<SambaPosDevelopment>("Support Chat", Resources.HattechNetwork, 93);
            AddDashboardCommand<SambaPosWiki>("Remote support", Resources.HattechNetwork, 94);

            AddDashboardCommand<ProductActivation>("Product Activation", Resources.HattechNetwork, 95);
            AddDashboardCommand<POSHardware>("POS Hardware", Resources.HattechNetwork, 96);

            AddDashboardCommand<SambaPosDatabaseSupport>(Resources.DatabaseSupport, Resources.HattechNetwork, 97);
        }
    }
}
