﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using DAL;
using DAL.Repos;
using Microsoft.AspNetCore.Mvc;

namespace WebApp
{
    public class ApothecaryController : Controller
    {
        private readonly ApothecaryEfRepo _repo;

        public ApothecaryController()
        {
            _repo = new ApothecaryEfRepo();
        }

        public IActionResult Index()
        {
            ViewBag.ErrorMsg = string.Empty;
            if(TempData.ContainsKey("ErrorMsg"))
            {
                ViewBag.ErrorMsg = TempData["ErrorMsg"];
                TempData.Remove("ErrorMsg");
            }
            return View(_repo.Get());
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(Apothecary apothecary)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _repo.Add(apothecary.FirstName, apothecary.LastName, apothecary.MonthlySalary);

                    return RedirectToAction("Index");
                }

                return View(apothecary);
            }

            catch (Exception ex) when (ex.InnerException != null)
            {
                ViewBag.ErrorMsg = ex.InnerExceptionMessageExtractor();
                return View(apothecary);
            }
        }

        public IActionResult Fire(int id)
        {
            try
            {
                _repo.Fire(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex) when (ex.InnerException != null)
            {
                TempData["ErrorMsg"] = ex.InnerExceptionMessageExtractor();
                return RedirectToAction("Index");
            }
        }

        public IActionResult Details(int id)
        {
            var apothecary = _repo.Get(id);
            if(apothecary != null)
            {
                return View(apothecary);
            }
            else
            {
                TempData["ErrorMsg"] = $"Nie ma aptekarza o id {id}";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Edit(int id)
        {
            return View(_repo.Get(id));
        }

        [HttpPost]
        public IActionResult Edit(Apothecary apothecary)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _repo.Update(apothecary.Id, apothecary.FirstName, apothecary.LastName, apothecary.MonthlySalary);

                    return RedirectToAction("Index");
                }

                return View(apothecary);
            }

            catch (Exception ex) when (ex.InnerException != null)
            {
                ViewBag.ErrorMsg = ex.InnerExceptionMessageExtractor();
                return View(apothecary);
            }
        }

        public IActionResult Delete(int id)
        {
            _repo.Remove(id);
            return RedirectToAction("Index");
        }
    }
}