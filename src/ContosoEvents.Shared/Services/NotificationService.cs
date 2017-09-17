using ContosoEvents.Models;
using ContosoEvents.Shared.Handlers;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ContosoEvents.Shared.Services
{
    public class NotificationService : INotificationService
    {
        const string LOG_TAG = "NotificationService";

        private ISettingService _settingService;
        private ILoggerService _loggerService;
        private IDataStoreService _dataStoreService;

        public NotificationService(ISettingService setting, ILoggerService logger, IDataStoreService dataStore)
        {
            _settingService = setting;
            _loggerService = logger;
            _dataStoreService = dataStore;
        }

        public async Task Notify(TicketOrder order)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "Notify", null);

            try
            {
                if (order == null)
                    throw new Exception("No Order");

                if (string.IsNullOrEmpty(order.Email))
                    throw new Exception("No Email in order!");

                TicketEvent ev = await _dataStoreService.GetEventById(order.EventId);
                if (ev == null)
                    throw new Exception("No associated Event");

                var templateBody = "";
                if (order.IsCancelled)
                    templateBody = ev.FailedEmailTemplate;
                else if (order.IsFulfilled)
                    templateBody = ev.SuccessEmailTemplate;
                else if (!order.IsFulfilled)
                    templateBody = ev.FailedEmailTemplate;

                if (string.IsNullOrEmpty(templateBody))
                    throw new Exception("No Template available");

                if (
                    string.IsNullOrEmpty(_settingService.GetEmailServerUrl()) ||
                    string.IsNullOrEmpty(_settingService.GetEmailServerUserName()) ||
                    string.IsNullOrEmpty(_settingService.GetEmailServerPassword())
                    )
                {
                    return; // Exit gracefully
                }

                // Produce the body by binding the template and the model
                string body = Engine.Razor.RunCompile(templateBody, "templateCahe", order.GetType(), order);

                SmtpClient client = new SmtpClient(_settingService.GetEmailServerUrl(), _settingService.GetEmailServerPort());
                client.EnableSsl = true;
                client.Credentials = new System.Net.NetworkCredential(_settingService.GetEmailServerUserName(), _settingService.GetEmailServerPassword());

                MailMessage message = new MailMessage();
                message.From = new MailAddress("info@contoseoevents.com", "info@contoseoevents.com", System.Text.Encoding.UTF8);
                message.To.Add(order.Email);
                message.Body = body;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = true;
                message.Subject = "ContosoEvents [" + ev.Name + "] Order " + (order.IsFulfilled ? "Confirmation" : "Failure");
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                client.Send(message);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                // Ignore expeptions as the account crdentials can be wrong and we don't want to cause health issues
                //throw new Exception(error);
            }
            finally
            {
                handler.Stop(error);
            }
        }
    }
}
