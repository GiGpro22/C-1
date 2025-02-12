using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BiblioTech
{
    public abstract class User
    {
        public string Name { get; private set; }

        public User(string name)
        {
            Name = name;
        }

        public abstract void ShowMenu(Library library);
    }

    public class Librarian : User
    {
        public Librarian(string name) : base(name) { }

        public override void ShowMenu(Library library)
        {
            while (true)
            {
                Console.WriteLine("\nМеню библиотекаря:");
                Console.WriteLine("1. Добавить книгу");
                Console.WriteLine("2. Удалить книгу");
                Console.WriteLine("3. Зарегистрировать пользователя");
                Console.WriteLine("4. Показать всех пользователей");
                Console.WriteLine("5. Показать все книги");
                Console.WriteLine("6. Выйти");

                Console.Write("Выберите действие: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": AddBook(library); break;
                    case "2": RemoveBook(library); break;
                    case "3": RegisterUser(library); break;
                    case "4": ViewAllUsers(library); break;
                    case "5": ViewAllBooks(library); break;
                    case "6": return; // Выход из меню
                    default: Console.WriteLine("Неверный ввод."); break;
                }
            }
        }

        // Методы библиотекаря (инкапсуляция логики)
        private void AddBook(Library library)
        {
            Console.Write("Название книги: ");
            string title = Console.ReadLine();
            Console.Write("Автор: ");
            string author = Console.ReadLine();
            library.AddBook(new Book(title, author));
        }

        private void RemoveBook(Library library)
        {
            Console.Write("Название книги для удаления: ");
            string title = Console.ReadLine();
            library.RemoveBook(title);
        }

        private void RegisterUser(Library library)
        {
            Console.Write("Имя нового пользователя: ");
            string name = Console.ReadLine();
            library.AddUser(new RegularUser(name));
        }

        private void ViewAllUsers(Library library)
        {
            Console.WriteLine("\nВсе пользователи:");
            foreach (var user in library.Users)
            {
                Console.WriteLine(user.Name);
            }
        }

        private void ViewAllBooks(Library library)
        {
            Console.WriteLine("\nВсе книги:");
            foreach (var book in library.Books)
            {
                Console.WriteLine($"{book.Title} - {book.Author} - {(book.IsAvailable ? "Доступна" : "Выдана")}");
            }
        }
    }

    public class RegularUser : User
    {
        private List<string> borrowedBooks = new List<string>();
        public List<string> BorrowedBooks { get { return borrowedBooks; } }

        public RegularUser(string name) : base(name) { }

        public override void ShowMenu(Library library)
        {
            while (true)
            {
                Console.WriteLine("\nМеню пользователя:");
                Console.WriteLine("1. Показать доступные книги");
                Console.WriteLine("2. Взять книгу");
                Console.WriteLine("3. Вернуть книгу");
                Console.WriteLine("4. Показать мои книги");
                Console.WriteLine("5. Выйти");

                Console.Write("Выберите действие: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ViewAvailableBooks(library); break;
                    case "2": BorrowBook(library); break;
                    case "3": ReturnBook(library); break;
                    case "4": ViewBorrowedBooks(); break;
                    case "5": return; 
                    default: Console.WriteLine("Неверный ввод."); break;
                }
            }
        }

        // Методы пользователя (инкапсуляция логики)
        private void ViewAvailableBooks(Library library)
        {
            Console.WriteLine("\nДоступные книги:");
            foreach (var book in library.Books)
            {
                if (book.IsAvailable)
                {
                    Console.WriteLine($"{book.Title} - {book.Author}");
                }
            }
        }

        private void BorrowBook(Library library)
        {
            Console.Write("Название книги для взятия: ");
            string title = Console.ReadLine();
            Book book = library.Books.FirstOrDefault(b => b.Title == title);

            if (book == null)
            {
                Console.WriteLine("Книга не найдена.");
                return;
            }

            if (!book.IsAvailable)
            {
                Console.WriteLine("Книга уже выдана.");
                return;
            }

            book.IsAvailable = false;
            borrowedBooks.Add(title);
            Console.WriteLine($"Вы взяли книгу: {title}");
            library.SaveData();
        }

        private void ReturnBook(Library library)
        {
            Console.Write("Название книги для возврата: ");
            string title = Console.ReadLine();

            if (!borrowedBooks.Contains(title))
            {
                Console.WriteLine("Вы не брали эту книгу.");
                return;
            }

            Book book = library.Books.FirstOrDefault(b => b.Title == title);

            if (book == null)
            {
                Console.WriteLine("Книга не найдена в библиотеке.");
                return;
            }

            book.IsAvailable = true;
            borrowedBooks.Remove(title);
            Console.WriteLine($"Вы вернули книгу: {title}");
            library.SaveData();
        }

        private void ViewBorrowedBooks()
        {
            Console.WriteLine("\nВзятые книги:");
            if (borrowedBooks.Count == 0)
            {
                Console.WriteLine("Вы не брали книги.");
                return;
            }
            foreach (var title in borrowedBooks)
            {
                Console.WriteLine(title);
            }
        }
    }

    
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public bool IsAvailable { get; set; } = true; 

        public Book(string title, string author)
        {
            Title = title;
            Author = author;
        }
    }

  
    public class Library
    {
        private List<Book> books = new List<Book>();
        private List<User> users = new List<User>();
        private string booksFilePath = "books.txt";
        private string usersFilePath = "users.txt";

        public List<Book> Books { get { return books; } }
        public List<User> Users { get { return users; } }

        public Library()
        {
            LoadData();
        }

        public void AddBook(Book book)
        {
            books.Add(book);
            SaveData();
        }

        public void RemoveBook(string title)
        {
            Book bookToRemove = books.FirstOrDefault(b => b.Title == title);
            if (bookToRemove != null)
            {
                books.Remove(bookToRemove);
                SaveData();
                Console.WriteLine($"Книга '{title}' удалена.");
            }
            else
            {
                Console.WriteLine($"Книга '{title}' не найдена.");
            }
        }

        public void AddUser(User user)
        {
            users.Add(user);
            SaveData();
        }


        private void LoadData()
        {
            // Загрузка книг
            if (File.Exists(booksFilePath))
            {
                foreach (string line in File.ReadLines(booksFilePath))
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 3)
                    {
                        string title = parts[0];
                        string author = parts[1];
                        bool isAvailable = bool.Parse(parts[2]);
                        books.Add(new Book(title, author) { IsAvailable = isAvailable });
                    }
                }
            }

            // Загрузка пользователей
            if (File.Exists(usersFilePath))
            {
                foreach (string line in File.ReadLines(usersFilePath))
                {
                    string[] parts = line.Split('|');
                    if (parts.Length >= 2)
                    {
                        string userType = parts[0];
                        string name = parts[1];
                        if (userType == "Librarian")
                        {
                            users.Add(new Librarian(name));
                        }
                        else if (userType == "RegularUser")
                        {
                            RegularUser user = new RegularUser(name);
                            if (parts.Length > 2)
                            {
                                for (int i = 2; i < parts.Length; i++)
                                {
                                    user.BorrowedBooks.Add(parts[i]);
                                    Book book = books.FirstOrDefault(b => b.Title == parts[i]);
                                    if (book != null)
                                    {
                                        book.IsAvailable = false;
                                    }
                                }
                            }
                            users.Add(user);
                        }
                    }
                }
            }
        }

        public void SaveData()
        {
            // Сохранение книг
            using (StreamWriter writer = new StreamWriter(booksFilePath))
            {
                foreach (var book in books)
                {
                    writer.WriteLine($"{book.Title}|{book.Author}|{book.IsAvailable}");
                }
            }

            // Сохранение пользователей
            using (StreamWriter writer = new StreamWriter(usersFilePath))
            {
                foreach (var user in users)
                {
                    if (user is Librarian)
                    {
                        writer.WriteLine($"Librarian|{user.Name}");
                    }
                    else if (user is RegularUser)
                    {
                        RegularUser regularUser = (RegularUser)user;
                        string line = $"RegularUser|{user.Name}";
                        foreach (var bookTitle in regularUser.BorrowedBooks)
                        {
                            line += $"|{bookTitle}";
                        }
                        writer.WriteLine(line);
                    }
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Каталог приложения: {AppDomain.CurrentDomain.BaseDirectory}");
            Library library = new Library();

            while (true)
            {
                Console.WriteLine("\nВыберите роль:");
                Console.WriteLine("1. Библиотекарь");
                Console.WriteLine("2. Пользователь");
                Console.WriteLine("3. Выйти");

                Console.Write("Ваш выбор: ");
                string roleChoice = Console.ReadLine();

                switch (roleChoice)
                {
                    case "1":
                        Console.Write("Введите имя библиотекаря: ");
                        string librarianName = Console.ReadLine();
                        Librarian librarian = (Librarian)library.Users.FirstOrDefault(u => u is Librarian && u.Name == librarianName);
                        if (librarian == null)
                        {
                            Console.WriteLine("Библиотекарь не найден. Создание нового.");
                            librarian = new Librarian(librarianName);
                            library.AddUser(librarian);
                        }
                        librarian.ShowMenu(library);
                        break;
                    case "2":
                        Console.Write("Введите имя пользователя: ");
                        string userName = Console.ReadLine();
                        RegularUser user = (RegularUser)library.Users.FirstOrDefault(u => u is RegularUser && u.Name == userName);
                        if (user == null)
                        {
                            Console.WriteLine("Пользователь не найден. Создание нового.");
                            user = new RegularUser(userName);
                            library.AddUser(user);
                        }
                        user.ShowMenu(library);
                        break;
                    case "3":
                        library.SaveData();
                        Console.WriteLine("Выход из программы.");
                        return;
                    default:
                        Console.WriteLine("Неверный ввод. Попробуйте снова.");
                        break;
                }
            }
        }
    }
}
