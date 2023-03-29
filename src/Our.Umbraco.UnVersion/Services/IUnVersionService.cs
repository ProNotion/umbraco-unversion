using Umbraco.Core.Models;

namespace Our.Umbraco.UnVersion.Services
{
    public interface IUnVersionService
    {
        IUnVersionConfig Config { get; }
        
        void UnVersion(IContent content);
    }
}