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
        [ValidateAntiForgeryToken] //  ДОБАВИЛ ЗАЩИТУ ОТ CSRF
        public IActionResult AddTask(string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                _todoService.Add(new ToDoItem { Title = title });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // ← ДОБАВИЛ ЗАЩИТУ ОТ CSRF
        public IActionResult CompleteTask(int id)
        {
            _todoService.MarkAsCompleted(id);
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken] // ← ДОБАВИЛ ЗАЩИТУ ОТ CSRF
        public IActionResult DeleteTask(int id)
        {
            _todoService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}