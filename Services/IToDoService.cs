using ToDoList.Models;

namespace ToDoList.Services
{
    public interface IToDoService
    {
        List<ToDoItem> GetAll();
        void Add(ToDoItem item);
        void MarkAsCompleted(int id);
        void Delete(int id);
    }
}
