namespace ToDoList.Models
{
    public class LoginAttempt
    {
        public string Username { get; set; } = string.Empty;
        public int FailedAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public DateTime LastAttempt { get; set; } = DateTime.UtcNow;
    }
}
