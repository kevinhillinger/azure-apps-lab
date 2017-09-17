using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Shared.Services;
using System;
using System.Security.Cryptography;

namespace ContosoEvents.Shared.Services
{
    public class ServiceLocationService : IServiceLocationService
    {
        private static RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();
        private static Random _rnd = new Random();

        public TServiceInterface Create<TServiceInterface>(Uri serviceName) where TServiceInterface : IService
        {
            return ServiceProxy.Create<TServiceInterface>(serviceName, new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(GetNextInt64Simple()));
        }

        public TServiceInterface Create<TServiceInterface>(long partitionKey, Uri serviceName) where TServiceInterface : IService
        {
            return ServiceProxy.Create<TServiceInterface>(serviceName, new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionKey));
        }

        public TServiceInterface Create<TServiceInterface>(string partitionKey, Uri serviceName) where TServiceInterface : IService
        {
            return ServiceProxy.Create<TServiceInterface>(serviceName, new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionKey));
        }

        private long GetNextInt64(long low = -9223372036854775808, long hi = 9223372036854775807)
        {
            if (low >= hi)
                throw new ArgumentException("low must be < hi");

            byte[] buf = new byte[8];
            double num;

            //Generate a random double
            _rng.GetBytes(buf);
            num = Math.Abs(BitConverter.ToDouble(buf, 0));

            //We only use the decimal portion
            num = num - Math.Truncate(num);

            //Return a number within range
            long result = (long)(num * ((double)hi - (double)low) + low);
            return result;
        }

        private long GetNextInt64Simple(long low = -9223372036854775808, long hi = 9223372036854775807)
        {
            if (low >= hi)
                throw new ArgumentException("low must be < hi");

            double num = _rnd.NextDouble();

            //Return a number within range
            long result = (long)(num * ((double)hi - (double)low) + low);
            return result;
        }
    }
}
