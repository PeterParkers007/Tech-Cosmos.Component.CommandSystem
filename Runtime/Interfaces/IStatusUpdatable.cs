using TechCosmos.CommandSystem.Runtime.Enums;

namespace TechCosmos.CommandSystem.Runtime.Interfaces
{
    public interface IStatusUpdatable
    {
        void UpdateStatus(CommandStatus newStatus);
    }
}