﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Interfaces;
using Common.Utils;
using DAL.Enums;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebApp.Interfaces;
using WebApp.Models.PrescriptionModels;

namespace WebApp.Controllers
{
    public class PrescriptionController : BaseController
    {
        #region Properties
        private readonly IPrescriptionRepo _prescriptionRepo;
        private readonly ILogger<PrescriptionController> _logger;
        private readonly ISelectService _selectService;
        #endregion

        public PrescriptionController(IPrescriptionRepo prescriptionRepo, ILogger<PrescriptionController> logger, ISelectService selectService)
        {
            _prescriptionRepo = prescriptionRepo;
            _logger = logger;
            _selectService = selectService;
        }

        public IActionResult Index()
        {
            DisplayErrorFromRedirectIfNecessary();

            var result = _prescriptionRepo.Get();

            if (result.IsSuccess)
            {
                var model = Mapper.Map<IEnumerable<PrescriptionViewModel>>(result.Value);
                return View(model);
            }

            AddLocalError(result.FailureMessage);
            return View(Enumerable.Empty<PrescriptionViewModel>());
        }

        public IActionResult CreatePrescription()
        {
            var doctors = _selectService.GetDoctorSelectList();
            var apothecaries = _selectService.GetApothecarySelectList();

            if (!doctors.IsSuccess) return RedirectToIndex(doctors.FailureMessage);
            if (!apothecaries.IsSuccess) return RedirectToIndex(apothecaries.FailureMessage);

            var model = new CreatePrescriptionViewModel
            {
                ApothecarySelect = apothecaries.Value,
                DoctorSelect = doctors.Value,
                PrescriptionDate = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult CreatePrescription(CreatePrescriptionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var action = AddSelectListToModelWithRedirectToIndex(model);
                return action ?? View(model);
            }

            var result = _prescriptionRepo.AddPrescription(model);

            if (!result.IsSuccess)
            {
                AddLocalError(result.FailureMessage);
                var action = AddSelectListToModelWithRedirectToIndex(model);
                return action ?? View(model);
            }

            return RedirectToPrescriptionDetails(result.Value);
        }

        private IActionResult AddSelectListToModelWithRedirectToIndex(CreatePrescriptionViewModel model)
        {
            var doctors = _selectService.GetDoctorSelectList();
            var apothecaries = _selectService.GetApothecarySelectList();

            if (!doctors.IsSuccess) return RedirectToIndex(doctors.FailureMessage);
            if (!apothecaries.IsSuccess) return RedirectToIndex(apothecaries.FailureMessage);

            model.ApothecarySelect = apothecaries.Value;
            model.DoctorSelect = doctors.Value;

            return null;
        }

        public IActionResult PrescriptionDetails(int id)
        {
            DisplayErrorFromRedirectIfNecessary();

            var result = _prescriptionRepo.Get(id);

            if (!result.IsSuccess) return RedirectToIndex(result.FailureMessage);

            var model = Mapper.Map<PrescriptionViewModel>(result.Value);
            return View(model);
        }

        public IActionResult EditPrescription(int id)
        {
            DisplayErrorFromRedirectIfNecessary();

            var doctors = _selectService.GetDoctorSelectList();
            var apothecaries = _selectService.GetApothecarySelectList();
            var result = _prescriptionRepo.Get(id);

            if (!doctors.IsSuccess) return RedirectToPrescriptionDetails(id, doctors.FailureMessage);
            if (!apothecaries.IsSuccess) return RedirectToPrescriptionDetails(id, apothecaries.FailureMessage);
            if (!result.IsSuccess) return RedirectToPrescriptionDetails(id, result.FailureMessage);

            var model = Mapper.Map<CreatePrescriptionViewModel>(result.Value);

            if (model.Status != PrescriptionStatusEnum.Created)
                return RedirectToPrescriptionDetails(id, $"Nie można modyfikować recepty o statusie {model.Status.GetDisplayName()}");

            model.ApothecarySelect = apothecaries.Value;
            model.DoctorSelect = doctors.Value;

            return View(model);
        }

        public IActionResult AcceptPrescription(int id)
        {

            var result = _prescriptionRepo.AcceptCreated(id);

            if (result.IsSuccess) return RedirectToPrescriptionDetails(id);

            return RedirectToPrescriptionDetails(id, result.FailureMessage);
        }

        [HttpPost]
        public IActionResult EditPrescription(CreatePrescriptionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var action = AddSelectListToModelWithRedirectToDetails(model);
                return action ?? View(model);
            }

            var result = _prescriptionRepo.EditPrescription(model);

            if (!result.IsSuccess)
            {
                AddLocalError(result.FailureMessage);
                var action = AddSelectListToModelWithRedirectToDetails(model);
                return action ?? RedirectToAction(nameof(EditPrescription), new { id = model.Id });
            }

            return RedirectToPrescriptionDetails(model.Id);
        }

        private IActionResult AddSelectListToModelWithRedirectToDetails(CreatePrescriptionViewModel model)
        {
            var doctors = _selectService.GetDoctorSelectList();
            var apothecaries = _selectService.GetApothecarySelectList();

            if (!doctors.IsSuccess) return RedirectToPrescriptionDetails(model.Id, doctors.FailureMessage);
            if (!apothecaries.IsSuccess) return RedirectToPrescriptionDetails(model.Id, apothecaries.FailureMessage);

            model.ApothecarySelect = apothecaries.Value;
            model.DoctorSelect = doctors.Value;

            return null;
        }

        public IActionResult AddPrescriptionItem(int id)
        {
            var result = _prescriptionRepo.Get(id);

            if (!result.IsSuccess) return RedirectToIndex(result.FailureMessage);
            var prescription = (PrescriptionViewModel)result.Value;
            if (prescription.Status != PrescriptionStatusEnum.Created)
                return RedirectToPrescriptionDetails(id, $"Nie można dodać leku do recepty o statusie: {prescription.Status.GetDisplayName()}");

            var model = new AddPrescriptionItemViewModel { PrescriptionId = id };

            var action = AddSelectListToModelWithRedirectToDetails(model);

            return action ?? View(model);
        }

        [HttpPost]
        public IActionResult AddPrescriptionItem(AddPrescriptionItemViewModel model)
        {
            if(!ModelState.IsValid)
            {
                var action = AddSelectListToModelWithRedirectToDetails(model);
                return action ?? View(model);
            }

            var result = _prescriptionRepo.AddPrescriptionItem(model.PrescriptionId, model);

            if (!result.IsSuccess) AddErrorForRedirect(result.FailureMessage);

            return RedirectToPrescriptionDetails(model.PrescriptionId);
        }

        private IActionResult AddSelectListToModelWithRedirectToDetails(AddPrescriptionItemViewModel model)
        {
            var medicine = _selectService.GetMedicineSelectList();

            if (!medicine.IsSuccess) return RedirectToPrescriptionDetails(model.Id, medicine.FailureMessage);

            model.MedicineList = medicine.Value;

            return null;
        }

        public IActionResult DeletePrescription(int id)
        {
            var result = _prescriptionRepo.DeletePrescription(id);

            if (!result.IsSuccess) return RedirectToIndex(result.FailureMessage);

            return RedirectToIndex();
        }

        public IActionResult RemovePrescriptionItem(int prescriptionId, int itemId)
        {
            var result = _prescriptionRepo.DeletePrescriptionItem(prescriptionId, itemId);

            if (!result.IsSuccess) AddErrorForRedirect(result.FailureMessage);

            return RedirectToPrescriptionDetails(prescriptionId);
        }

        public IActionResult EditPrescriptonItem(int prescriptionId, int itemId)
        {
            DisplayErrorFromRedirectIfNecessary();

            var result = _prescriptionRepo.GetItem(itemId);

            if (!result.IsSuccess) return RedirectToPrescriptionDetails(prescriptionId, result.FailureMessage);

            var model = Mapper.Map<PrescriptionItemViewModel>(result.Value);

            return View(model);
        }

        [HttpPost]
        public IActionResult EditPrescriptonItem(PrescriptionItemViewModel model)
        {

            if (!ModelState.IsValid) return View(model);

            var result = _prescriptionRepo.EditPrescriptionItem(model);

            if(!result.IsSuccess)
            {
                AddErrorForRedirect(result.FailureMessage);
                return RedirectToAction(nameof(EditPrescriptonItem), new { prescriptionId = model.PrescriptionId, itemId = model.Id });
            }

            return RedirectToPrescriptionDetails(model.PrescriptionId);
        }

        public IActionResult BuyAll(int id)
        {
            var result = _prescriptionRepo.BuyAll(id);

            if (!result.IsSuccess) AddErrorForRedirect(result.FailureMessage);

            return RedirectToPrescriptionDetails(id);
        }

        public IActionResult BuyView(int id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult BuyView(PrescriptionViewModel id)
        {
            return View();
        }

        private IActionResult RedirectToPrescriptionDetails(int id)
        {
            return RedirectToAction(nameof(PrescriptionDetails), new { id });
        }

        private IActionResult RedirectToPrescriptionDetails(int id, string message)
        {
            AddErrorForRedirect(message);
            return RedirectToAction(nameof(PrescriptionDetails), new { id });
        }
    }
}