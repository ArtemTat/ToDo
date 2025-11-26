using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ToDoList.Models;
using ToDoList.Services;

namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private readonly IToDoService _todoService;

        public HomeController(IToDoService todoService)
        {
            _todoService = todoService;
        }

        public IActionResult Index()
        {
            var items = _todoService.GetAll();
            return View(items);
        }

        [HttpPost]
        public IActionResult AddTask(string title) // Уязвимость: нет проверки ввода!
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                _todoService.Add(new ToDoItem { Title = title });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CompleteTask(int id) // Уязвимость: нет защиты от CSRF!
        {
            _todoService.MarkAsCompleted(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteTask(int id) // Уязвимость: нет защиты от CSRF!
        {
            _todoService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}