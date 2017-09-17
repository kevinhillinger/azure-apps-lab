using System;

namespace ContosoEvents.Shared.Services
{
    public interface IUriBuilderService
    {
        string ApplicationInstance { get; set; }
        string ServiceInstance { get; set; }
        Uri ToUri();
    }
}
