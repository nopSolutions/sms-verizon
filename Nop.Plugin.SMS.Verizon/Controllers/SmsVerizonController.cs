using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.SMS.Verizon;
using Nop.Plugin.Sms.Verizon.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Plugins;

namespace Nop.Plugin.Sms.Verizon.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class SmsVerizonController : BasePluginController
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IPluginFinder _pluginFinder;
        private readonly ISettingService _settingService;
        private readonly VerizonSettings _verizonSettings;

        public SmsVerizonController(ILocalizationService localizationService,
            IPermissionService permissionService,
            IPluginFinder pluginFinder,
            ISettingService settingService,
            VerizonSettings verizonSettings)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._pluginFinder = pluginFinder;
            this._settingService = settingService;
            this._verizonSettings = verizonSettings;
        }
       
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new SmsVerizonModel
            {
                Enabled = _verizonSettings.Enabled,
                Email = _verizonSettings.Email
            };

            return View("~/Plugins/SMS.Verizon/Views/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public IActionResult ConfigurePOST(SmsVerizonModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Configure();
            }

            //save settings
            _verizonSettings.Enabled = model.Enabled;
            _verizonSettings.Email = model.Email;
            _settingService.SaveSetting(_verizonSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("test-sms")]
        public IActionResult TestSms(SmsVerizonModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            try
            {
                if (string.IsNullOrEmpty(model.TestMessage))
                {
                    ErrorNotification("Enter test message");
                }
                else
                {
                    var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("Mobile.SMS.Verizon");
                    if (pluginDescriptor == null)
                        throw new Exception("Cannot load the plugin");
                    var plugin = pluginDescriptor.Instance() as VerizonSmsProvider;
                    if (plugin == null)
                        throw new Exception("Cannot load the plugin");

                    if (!plugin.SendSms(model.TestMessage))
                    {
                        ErrorNotification(_localizationService.GetResource("Plugins.Sms.Verizon.TestFailed"));
                    }
                    else
                    {
                        SuccessNotification(_localizationService.GetResource("Plugins.Sms.Verizon.TestSuccess"));
                    }
                }
            }
            catch(Exception exc)
            {
                ErrorNotification(exc.ToString());
            }

            return View("~/Plugins/SMS.Verizon/Views/Configure.cshtml", model);
        }
    }
}