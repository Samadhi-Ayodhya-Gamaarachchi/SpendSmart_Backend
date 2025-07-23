namespace SpendSmart_Backend.Configuration
{
    public static class UserConfiguration
    {
        // Change this value to update the default userId throughout the backend
        public static int DefaultUserId { get; set; } = 1;

        // Helper method to get the current default user ID
        public static int GetDefaultUserId()
        {
            return DefaultUserId;
        }

        // Method to update the default user ID if needed
        public static void SetDefaultUserId(int newUserId)
        {
            DefaultUserId = newUserId;
        }
    }
}
