using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;

namespace Nop.Plugin.SMS.Verizon
{
    /// <summary>
    /// Represents the Verizon SMS provider
    /// </summary>
    public class VerizonSmsProvider : BasePlugin, IMiscPlugin
    {
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly VerizonSettings _verizonSettings;

        public VerizonSmsProvider(IEmailAccountService emailAccountService,
            ILocalizationService localizationService,
            ILogger logger,
            IQueuedEmailService queuedEmailService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            EmailAccountSettings emailAccountSettings,
            VerizonSettings verizonSettings)
        {
            this._emailAccountService = emailAccountService;
            this._localizationService = localizationService;
            this._logger = logger;
            this._queuedEmailService = queuedEmailService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._webHelper = webHelper;
            this._emailAccountSettings = emailAccountSettings;
            this._verizonSettings = verizonSettings;
        }

        /// <summary>
        /// Sends SMS
        /// </summary>
        /// <param name="text">SMS text</param>
        /// <returns>Result</returns>
        public bool SendSms(string text)
        {
            try
            {
                var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId) ?? _emailAccountService.GetAllEmailAccounts().FirstOrDefault();

                if (emailAccount == null)
                    throw new Exception("No email account could be loaded");

                var queuedEmail = new QueuedEmail
                {
                    Priority = QueuedEmailPriority.High,
                    From = emailAccount.Email,
                    FromName = emailAccount.DisplayName,
                    To = _verizonSettings.Email,
                    ToName = string.Empty,
                    Subject = _storeContext.CurrentStore.Name,
                    Body = text,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id
                };

                _queuedEmailService.InsertQueuedEmail(queuedEmail);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/SmsVerizon/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new VerizonSettings
            {
                Email = "yournumber@vtext.com",
            };
            _settingService.SaveSetting(settings);

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sms.Verizon.TestFailed", "Test message sending failed");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sms.Verizon.TestSuccess", "Test message was sent (queued)");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sms.Verizon.Fields.Enabled", "Enabled");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sms.Verizon.Fields.Enabled.Hint", "Check to enable SMS provider");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sms.Verizon.Fields.Email", "Email");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sms.Verizon.Fields.Email.Hint", "Verizon email address(e.g. your_phone_number@vtext.com)");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sms.Verizon.Fields.TestMessage", "Message text");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sms.Verizon.Fields.TestMessage.Hint", "Text of the test message");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sms.Verizon.SendTest", "Send");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sms.Verizon.SendTest.Hint", "Send test message");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<VerizonSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Sms.Verizon.TestFailed");
            _localizationService.DeletePluginLocaleResource("Plugins.Sms.Verizon.TestSuccess");
            _localizationService.DeletePluginLocaleResource("Plugins.Sms.Verizon.Fields.Enabled");
            _localizationService.DeletePluginLocaleResource("Plugins.Sms.Verizon.Fields.Enabled.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Sms.Verizon.Fields.Email");
            _localizationService.DeletePluginLocaleResource("Plugins.Sms.Verizon.Fields.Email.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Sms.Verizon.Fields.TestMessage");
            _localizationService.DeletePluginLocaleResource("Plugins.Sms.Verizon.Fields.TestMessage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Sms.Verizon.SendTest");
            _localizationService.DeletePluginLocaleResource("Plugins.Sms.Verizon.SendTest.Hint");

            base.Uninstall();
        }
    }
}
