using System;
using JetBrains.ActionManagement;
using JetBrains.ActionManagement.Impl;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings.UserInterface;
using JetBrains.DataFlow;
using JetBrains.UI.Settings;
using JetBrains.Util;
using DataConstants = JetBrains.UI.Settings.DataConstants;
using System.Linq;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Settings
{
    [ShellComponent]
    public class ActionOverrideRegistrar
    {
        public ActionOverrideRegistrar(Lifetime lifetime, IActionManager actionManager)
        {
            AddOverridingHandler(lifetime, actionManager, typeof(DeleteInjectedLayerAction), new PreventDeleteInjectedLayerAction());
            AddOverridingHandler(lifetime, actionManager, typeof(ResetSelectedSettingsLayersAction), new PreventResetSettingsLayerAction());
        }

        private static void AddOverridingHandler(Lifetime lifetime, IActionManager actionManager, Type actionType,
                                                 IActionHandler actionHandler)
        {
            var action = ActionInfo.GetActionFromActionHandler(actionType, actionManager);
            action.AddHandler(lifetime, actionHandler);
        }

        private class PreventDeleteInjectedLayerAction : IActionHandler
        {
            public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
            {
                var layers = context.GetData(DataConstants.SelectedUserFriendlySettingsLayers);
                if (layers == null || layers.IsEmpty())
                    return false;

                // Action is disabled if *all* layers have deletion blocked
                return layers.Any(CanDelete) && nextUpdate();
            }

            public void Execute(IDataContext context, DelegateExecute nextExecute)
            {
                // Just let the "real" handler do its stuff. We either won't be called
                // (because we returned false to Update) or there is more than one layer
                // being removed. Since we do nothing in response to a delete request,
                // it doesn't matter
                nextExecute();
            }

            private static bool CanDelete(UserFriendlySettingsLayer.Identity layer)
            {
                if (layer.CharacteristicMount == null)
                    return false;

                // Look for our metadata id to see if we should prevent deletion
                var metadata = layer.CharacteristicMount.Metadata;
                return !metadata.TryGet(SettingsLoader.IsNonUserEditable);
            }
        }

        private class PreventResetSettingsLayerAction : IActionHandler
        {
            public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
            {
                var layers = context.GetData(DataConstants.SelectedUserFriendlySettingsLayers);
                if (layers == null || layers.IsEmpty())
                    return false;

                // Action is disabled if *any* layers have reset blocked
                return layers.All(CanReset) && nextUpdate();
            }

            public void Execute(IDataContext context, DelegateExecute nextExecute)
            {
                // Safely call next, we won't get called if any layers have reset blocked
                nextExecute();
            }

            private static bool CanReset(UserFriendlySettingsLayer.Identity layer)
            {
                if (layer.CharacteristicMount == null)
                    return false;

                // Look for our metadata id to see if we should prevent reset
                var metadata = layer.CharacteristicMount.Metadata;
                return !metadata.TryGet(SettingsLoader.IsNonUserEditable);
            }
        }
    }
}