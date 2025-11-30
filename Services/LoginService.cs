using ToDoList.Models;
using ToDoList.Services;

namespace TodoApp.Services;

public class LoginService : ILoginService
{
    private readonly Dictionary<string, LoginAttempt> _loginAttempts = new();
    private const int MAX_ATTEMPTS = 3;
    private readonly TimeSpan LOCKOUT_DURATION = TimeSpan.FromMinutes(5);

    public bool IsAccountLocked(string username)
    {
        if (_loginAttempts.ContainsKey(username))
        {
            var attempt = _loginAttempts[username];
            if (attempt.LockoutEnd.HasValue && DateTime.UtcNow < attempt.LockoutEnd.Value)
            {
                return true;
            }

            // Сброс блокировки если время вышло
            if (attempt.LockoutEnd.HasValue && DateTime.UtcNow >= attempt.LockoutEnd.Value)
            {
                ResetFailedAttempts(username);
            }
        }
        return false;
    }

    public void RecordFailedAttempt(string username)
    {
        if (!_loginAttempts.ContainsKey(username))
        {
            _loginAttempts[username] = new LoginAttempt { Username = username };
        }

        var attempt = _loginAttempts[username];
        attempt.FailedAttempts++;
        attempt.LastAttempt = DateTime.UtcNow;

        // Блокировка после MAX_ATTEMPTS неудачных попыток
        if (attempt.FailedAttempts >= MAX_ATTEMPTS)
        {
            attempt.LockoutEnd = DateTime.UtcNow.Add(LOCKOUT_DURATION);
        }
    }

    public void ResetFailedAttempts(string username)
    {
        if (_loginAttempts.ContainsKey(username))
        {
            _loginAttempts.Remove(username);
        }
    }

    public int GetRemainingAttempts(string username)
    {
        if (_loginAttempts.ContainsKey(username))
        {
            var attempt = _loginAttempts[username];
            return Math.Max(0, MAX_ATTEMPTS - attempt.FailedAttempts);
        }
        return MAX_ATTEMPTS;
    }
}