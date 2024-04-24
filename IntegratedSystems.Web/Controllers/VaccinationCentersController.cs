using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IntegratedSystems.Domain.Domain_Models;
using IntegratedSystems.Repository;
using IntegratedSystems.Domain.DTO;
using IntegratedSystems.Service.Interface;

namespace IntegratedSystems.Web.Controllers
{
    public class VaccinationCentersController : Controller
    {
        private readonly IVaccinationCenterService _vaccinationCenterService;
        private readonly IPatientService _patientService;
        private readonly IVaccinationService _vaccinationService;

        public VaccinationCentersController(IVaccinationCenterService vaccCenterService, IPatientService patientService, IVaccinationService vaccinationService)
        {
            _vaccinationCenterService = vaccCenterService;
            _patientService = patientService;
            _vaccinationService = vaccinationService;
        }

        // GET: VaccinationCenters
        public IActionResult Index()
        {
            return View(_vaccinationCenterService.GetVaccinationCenters());
        }

        // GET: VaccinationCenters/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            CenterDetailsDTO dto = new CenterDetailsDTO();

            dto.vaccCenter = _vaccinationCenterService.GetVaccinationCenterById(id);
            dto.vaccines = _vaccinationService.GetVaccinesForCenter(id);

            if (dto.vaccCenter == null)
            {
                return NotFound();
            }

            return View(dto);
        }

        // GET: VaccinationCenters/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VaccinationCenters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Name,Address,MaxCapacity,Id")] VaccinationCenter vaccinationCenter)
        {
            if (ModelState.IsValid)
            {
                vaccinationCenter.Id = Guid.NewGuid();
                _vaccinationCenterService.CreateNewVaccinationCenter(vaccinationCenter);
                return RedirectToAction(nameof(Index));
            }
            return View(vaccinationCenter);
        }

        // GET: VaccinationCenters/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vaccinationCenter = _vaccinationCenterService.GetVaccinationCenterById(id);
            if (vaccinationCenter == null)
            {
                return NotFound();
            }
            return View(vaccinationCenter);
        }

        // POST: VaccinationCenters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Name,Address,MaxCapacity,Id")] VaccinationCenter vaccinationCenter)
        {
            if (id != vaccinationCenter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _vaccinationCenterService.UpdateVaccinationCenter(vaccinationCenter);
                return RedirectToAction(nameof(Index));
            }
            return View(vaccinationCenter);
        }

        // GET: VaccinationCenters/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vaccinationCenter = _vaccinationCenterService.GetVaccinationCenterById(id);
            if (vaccinationCenter == null)
            {
                return NotFound();
            }

            return View(vaccinationCenter);
        }

        // POST: VaccinationCenters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var vaccinationCenter = _vaccinationCenterService.GetVaccinationCenterById(id);
            if (vaccinationCenter != null)
            {
                _vaccinationCenterService.DeleteVaccinationCenter(id);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VaccinationCenterExists(Guid id)
        {
            return _vaccinationCenterService.GetVaccinationCenterById(id) != null;
        }

        // Schedule Vaccination 
        public IActionResult ScheduleVaccination(Guid id)
        {
            var vaccCenter = _vaccinationCenterService.GetVaccinationCenterById(id);
            if (vaccCenter.MaxCapacity <= 0)
            {
                return RedirectToAction(nameof(NoMoreCapacity));
            }
            VaccinationDTO dto = new VaccinationDTO();
            dto.vaccCenterId = id;
            dto.patients = _patientService.GetPatients();
            dto.manufacturers = new List<string>()
            {
                "Sputnik", "Astra Zeneca", "Phizer"
            };

            return View(dto);
        }

        // POST for schedule vaccination

        [HttpPost, ActionName("Schedule")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmSchedule(VaccinationDTO dto)
        {
            if (ModelState.IsValid)
            {
                _vaccinationCenterService.LowerCapacity(dto.vaccCenterId);
                _vaccinationService.ScheduleVaccine(dto);
                return RedirectToAction(nameof(Index));
            }

            return View(dto);
        }

        public IActionResult NoMoreCapacity()
        {
            return View();
        }
    }
}
