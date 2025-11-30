using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ToDoList.Models;
using ToDoList.Services;
using System.Data;
using Microsoft.Extensions.Configuration;


namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private readonly IToDoService _todoService;
        private readonly IConfiguration _configuration;
        private readonly ILoginService _loginService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IToDoService todoService, IConfiguration configuration, ILoginService loginService, ILogger<HomeController> logger)
        {
            _todoService = todoService;
            _configuration = configuration;
            _loginService = loginService;
            _logger = logger;
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Index()
        {
            var items = _todoService.GetAll();
            return View(items);
        }

        public IActionResult Search()
        {
            return View();
        }

        public IActionResult LoginSecure()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            ViewBag.IsVulnerable = true;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Message = "Please enter both username and password";
                return View();
            }

            // УЯЗВИМЫЙ КОД - имитация SQL инъекции
            bool isAuthenticated = CheckCredentialsVulnerable(username, password);

            if (isAuthenticated)
            {
                ViewBag.Message = " Login successful! (This demonstrates SQL injection vulnerability)";
                ViewBag.Username = username;
                ViewBag.Password = password;
                ViewBag.Query = $"SELECT * FROM Users WHERE Username = '{username}' AND Password = '{password}'";
            }
            else
            {
                ViewBag.Message = "❌ Login failed";
            }

            return View();
        }

        private bool CheckCredentialsVulnerable(string username, string password)
        {
            // Имитируем уязвимый SQL запрос
            // В реальном приложении это выглядело бы так:
            // string query = $"SELECT * FROM Users WHERE Username = '{username}' AND Password = '{password}'";

            // Для демонстрации создаем "базу данных" 
            var users = new Dictionary<string, string>
    {
        { "admin", "password123" },
        { "user", "123456" },
        { "test", "test" }
    };

            // УЯЗВИМОСТЬ: если ввести ' OR '1'='1 в пароль, то проверка всегда вернет true
            try
            {
                // Имитируем уязвимый SQL запрос
                if (users.ContainsKey(username) && users[username] == password)
                {
                    return true;
                }

                // Демонстрация SQL инъекции: если пароль содержит инъекцию, обходим проверку
                if (password.Contains("' OR '1'='1") || password.Contains("' OR 1=1--"))
                {
                    return true; // SQL инъекция успешна!
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"SQL Error: {ex.Message}";
            }

            return false;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(string searchTerm)
        {
            var results = new List<ToDoItem>();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                //УЯЗВИМЫЙ КОД - конкатенация строк (SQL Injection)
                var vulnerableResults = ExecuteVulnerableQuery(searchTerm);
                results = vulnerableResults;
            }

            return View("SearchResults", results);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoginSecure(string username, string password)
        {
            ViewBag.IsSecure = true;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Message = "Please enter both username and password";
                return View();
            }

            // ЗАЩИЩЕННЫЙ КОД - параметризованные запросы
            bool isAuthenticated = CheckCredentialsSecure(username, password);

            if (isAuthenticated)
            {
                ViewBag.Message = "✅ Login successful!";
                ViewBag.Username = username;
            }
            else
            {
                ViewBag.Message = "❌ Login failed";

                // Логируем попытку SQL инъекции
                if (IsSqlInjectionAttempt(username) || IsSqlInjectionAttempt(password))
                {
                    ViewBag.SecurityWarning = "🚨 SQL Injection attempt detected and blocked!";
                }
            }

            return View("Login"); // Используем то же представление
        }

        //  ЗАЩИЩЕННЫЙ МЕТОД ПРОВЕРКИ КРЕДЕНЦИАЛОВ
        private bool CheckCredentialsSecure(string username, string password)
        {
            // Та же "база данных"
            var users = new Dictionary<string, string>
    {
        { "admin", "password123" },
        { "user", "123456" },
        { "test", "test" }
    };

            // ЗАЩИТА: проверяем на SQL инъекции
            if (IsSqlInjectionAttempt(username) || IsSqlInjectionAttempt(password))
            {
                return false; // Блокируем попытку инъекции
            }

            // Безопасная проверка
            return users.ContainsKey(username) && users[username] == password;
        }

        //  МЕТОД ОБНАРУЖЕНИЯ SQL ИНЪЕКЦИЙ
        private bool IsSqlInjectionAttempt(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            var sqlInjectionPatterns = new[]
            {
        "' OR '1'='1", "' OR 1=1--", "';", "--", "/*", "*/", "@@", "char(",
        "union select", "insert into", "drop table", "update ", "delete from"
    };

            return sqlInjectionPatterns.Any(pattern =>
                input.ToLower().Contains(pattern.ToLower()));
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

        //УЯЗВИМЫЙ МЕТОД - демонстрация SQL инъекции

        private List<ToDoItem> ExecuteVulnerableQuery(string searchTerm)
        {
            var results = new List<ToDoItem>();

            var allItems = _todoService.GetAll();
            results = allItems.Where(item =>
                item.Title != null &&
                item.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return results;
        }

        //ЗАЩИЩЕННЫЙ МЕТОД - исправленный
        private List<ToDoItem> ExecuteSecureQuery(string searchTerm)
        {
            var allItems = _todoService.GetAll();

            var results = allItems.Where(item =>
                item.Title != null &&
                item.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return results;
        }


        // 🔐 ЗАЩИЩЕННАЯ ФОРМА С БЛОКИРОВКОЙ ОТ BRUTE-FORCE
        public IActionResult LoginProtected()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoginProtected(string username, string password)
        {
            ViewBag.IsProtected = true;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Message = "Please enter both username and password";
                return View("Login");
            }

            // 🔒 Проверка блокировки аккаунта
            if (_loginService.IsAccountLocked(username))
            {
                ViewBag.Message = " Account temporarily locked due to too many failed attempts. Please try again in 5 minutes.";
                _logger.LogWarning($"Account locked for user: {username}");
                return View("Login");
            }

            // Проверка SQL инъекций
            if (IsSqlInjectionAttemptProtected(username) || IsSqlInjectionAttemptProtected(password))
            {
                _loginService.RecordFailedAttempt(username);
                ViewBag.Message = " Login failed - security violation detected";
                ViewBag.SecurityWarning = " SQL Injection attempt detected!";
                _logger.LogWarning($"SQL injection attempt from IP: {HttpContext.Connection.RemoteIpAddress}");
                return View("Login");
            }

            // Проверка учетных данных
            var users = new Dictionary<string, string>
        {
            { "admin", "Password123!" },
            { "user", "UserPass123!" },
            { "test", "TestPass123!" }
        };

            bool isAuthenticated = users.ContainsKey(username) && users[username] == password;

            if (isAuthenticated)
            {
                _loginService.ResetFailedAttempts(username);
                ViewBag.Message = " Login successful!";
                ViewBag.Username = username;
                _logger.LogInformation($"Successful login for user: {username}");
            }
            else
            {
                _loginService.RecordFailedAttempt(username);
                var remainingAttempts = _loginService.GetRemainingAttempts(username);

                if (remainingAttempts <= 0)
                {
                    ViewBag.Message = " Account locked due to too many failed attempts. Please try again in 5 minutes.";
                }
                else
                {
                    ViewBag.Message = $" Login failed. {remainingAttempts} attempts remaining.";
                }

                _logger.LogWarning($"Failed login attempt for user: {username}. Remaining attempts: {remainingAttempts}");
            }

            return View("Login");
        }

        // Метод для сброса блокировки (для тестирования)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetLockout(string username)
        {
            _loginService.ResetFailedAttempts(username);
            ViewBag.Message = $"✅ Lockout reset for user: {username}";
            return View("Login");
        }

        // ОТДЕЛЬНЫЙ МЕТОД ДЛЯ BRUTE-FORCE ЗАЩИТЫ (чтобы избежать конфликта)
        private bool IsSqlInjectionAttemptProtected(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            var sqlInjectionPatterns = new[]
            {
            "' OR '1'='1", "' OR 1=1--", "';", "--", "/*", "*/", "@@", "char(",
            "union select", "insert into", "drop table", "update ", "delete from"
        };

            return sqlInjectionPatterns.Any(pattern =>
                input.ToLower().Contains(pattern.ToLower()));
        }
    }
}