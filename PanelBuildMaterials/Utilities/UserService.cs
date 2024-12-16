using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;

namespace PanelBuildMaterials.Utilities
{
    public class UserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PanelDbContext _context;

        public UserService(IHttpContextAccessor httpContextAccessor, PanelDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public int? UserId => _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
        public string? UserLogin => _httpContextAccessor.HttpContext?.Session.GetString("UserLogin");

        public string? UserLaws
        {
            get
            {
                var userId = UserId;
                if (userId == null) return null;

                return _context.Users.FirstOrDefault(u => u.UserId == userId)?.UserLaws;
            }
        }

        public bool HasAccess(string requiredLaw)
        {
            if (string.IsNullOrEmpty(UserLaws)) return false;
            return UserLaws.Contains(requiredLaw, StringComparison.OrdinalIgnoreCase);
        }
    }
}
