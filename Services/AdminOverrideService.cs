namespace DeviceManager.Services
{
    public interface IAdminOverrideService
    {
        // This service can be expanded to include methods that allow admins to override
        // certain restrictions or perform actions on behalf of other users.
        bool IsEnabled(); 
    }

    public class AdminOverrideService : IAdminOverrideService
    {
        private readonly IConfiguration _config;

        public AdminOverrideService(IConfiguration config)
        {
            _config = config;
        }

        public bool IsEnabled()
        {
            var enabled = _config.GetValue<bool>("AdminOverride:Enabled");
            var expiresAt = _config.GetValue<DateTime?>("AdminOverride:ExpiresAt");

            if (!enabled) return false;

            if (expiresAt == null || expiresAt < DateTime.UtcNow)
            {
                _config["AdminOverride:Enabled"] = "false";
                return false;
            }

            return true;
        }

    }


}
