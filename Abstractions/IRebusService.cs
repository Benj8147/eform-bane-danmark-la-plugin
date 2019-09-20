using Rebus.Bus;

namespace BaneDanmarkLa.Pn.Abstractions
{
    public interface IRebusService
    {
        void Start(string connectionString);
        IBus GetBus();

    }
}