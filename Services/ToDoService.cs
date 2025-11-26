using ToDoList.Models;

namespace ToDoList.Services
{
    public class ToDoService : IToDoService
    {
        private readonly List<ToDoItem> _items = new();
        private int _nextId = 1;

        public List<ToDoItem> GetAll() => _items;

        public void Add(ToDoItem item)
        {
            item.Id = _nextId++;
            _items.Add(item);
        }

        public void MarkAsCompleted(int id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.IsCompleted = true;
            }
        }

        public void Delete(int id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                _items.Remove(item);
            }
        }
    }
}
