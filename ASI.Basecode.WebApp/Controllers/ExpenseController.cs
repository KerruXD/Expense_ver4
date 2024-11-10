using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly ICategoryService _categoryService;

        public ExpenseController(IExpenseRepository expenseRepository, ICategoryService categoryService)
        {
            _expenseRepository = expenseRepository;
            _categoryService = categoryService;
        }

        public IActionResult Index(string startDate, string endDate, int? category)
        {
            var expenses = _expenseRepository.ViewExpenses();

            // Apply filters if any
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate);
                expenses = expenses.Where(e => e.Date >= start).ToList();
            }

            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate);
                expenses = expenses.Where(e => e.Date <= end).ToList();
            }

            if (category.HasValue)
            {
                expenses = expenses.Where(e => e.CategoryID == category.Value).ToList();
            }

            ViewBag.Categories = _categoryService.GetCategories();
            return PartialView("Index", expenses);
        }
        public IActionResult Create()
        {
            ViewBag.Categories = _categoryService.GetCategories(); // Ensure categories are set
            return View();
        }

        [HttpPost]
        public IActionResult Create(Expense expense)
        {
            if (ModelState.IsValid)
            {
                _expenseRepository.AddExpense(expense);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Categories = _categoryService.GetCategories(); // Reload categories in case of validation error
            return View(expense);
        }

        public IActionResult Edit(int id)
        {
            var expense = _expenseRepository.ViewExpenses().FirstOrDefault(e => e.ExpenseID == id);
            if (expense == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _categoryService.GetCategories();
            return View(expense);
        }

        [HttpPost]
        public IActionResult Edit(Expense expense)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _categoryService.GetCategories();
                return View(expense);
            }

            try
            {
                _expenseRepository.UpdateExpense(expense);
                return Redirect(Url.Action("Index", "Home"));

            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "The record you attempted to edit was modified by another user after you got the original value. Please reload the page and try again.");
                ViewBag.Categories = _categoryService.GetCategories();
                return View(expense);
            }
        }

        public IActionResult Delete(int id)
        {
            var expense = _expenseRepository.ViewExpenses().FirstOrDefault(e => e.ExpenseID == id);
            if (expense != null)
            {
                _expenseRepository.DeleteExpense(expense);
            }
            return RedirectToAction("Index");
        }
    }
}
