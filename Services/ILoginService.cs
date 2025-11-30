namespace ToDoList.Services
{
    public interface ILoginService
    {
        bool IsAccountLocked(string username);
        void RecordFailedAttempt(string username);
        void ResetFailedAttempts(string username);
        int GetRemainingAttempts(string username);
    }
}
