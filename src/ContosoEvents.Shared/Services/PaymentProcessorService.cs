using ContosoEvents.Models;
using ContosoEvents.Shared.Handlers;
using System;
using System.Threading.Tasks;

namespace ContosoEvents.Shared.Services
{
    public class PaymentProcessorService : IPaymentProcessorService
    {
        const string LOG_TAG = "PaymentProcessorService";

        private ISettingService _settingService;
        private ILoggerService _loggerService;

        private static Random _random = new Random();

        public PaymentProcessorService(ISettingService setting, ILoggerService logger)
        {
            _settingService = setting;
            _loggerService = logger;
        }

        public async Task<string> Authorize(TicketOrder order)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "Authorize", null);
            var confirmation = "";

            try
            {
                if (order == null)
                    throw new Exception("No Order");

                // Charge against a payment processor
                confirmation = GenerateRandomNumber(12);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                handler.Stop(error);
            }

            return confirmation;
        }

        public async Task<bool> Refund(TicketOrder order)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "Refund", null);
            var isSuccess = false;

            try
            {
                if (order == null)
                    throw new Exception("No Order");

                // Refund against a payment processor
                isSuccess = true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                handler.Stop(error);
            }

            return isSuccess;
        }

        // PRIVATE
        private static string GenerateRandomNumber(int nLength)
        {
            char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            int charsNo = 36;
            int length = nLength;
            String rndString = "";

            for (int i = 0; i < length; i++)
                rndString += chars[_random.Next(charsNo)];

            return rndString.ToUpper();
        }
    }
}
