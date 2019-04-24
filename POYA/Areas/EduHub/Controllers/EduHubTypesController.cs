﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using POYA.Areas.EduHub.Models;
using POYA.Data;

namespace POYA.Areas.EduHub.Controllers
{
    [Area("EduHub")]
    public class EduHubTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EduHubTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EduHub/EduHubTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.EduHubType.ToListAsync());
        }

        // GET: EduHub/EduHubTypes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eduHubType = await _context.EduHubType
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eduHubType == null)
            {
                return NotFound();
            }

            return View(eduHubType);
        }

        // GET: EduHub/EduHubTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EduHub/EduHubTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,SubjectId")] EduHubType eduHubType)
        {
            if (ModelState.IsValid)
            {
                eduHubType.Id = Guid.NewGuid();
                _context.Add(eduHubType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eduHubType);
        }

        // GET: EduHub/EduHubTypes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eduHubType = await _context.EduHubType.FindAsync(id);
            if (eduHubType == null)
            {
                return NotFound();
            }
            return View(eduHubType);
        }

        // POST: EduHub/EduHubTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,SubjectId")] EduHubType eduHubType)
        {
            if (id != eduHubType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eduHubType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EduHubTypeExists(eduHubType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eduHubType);
        }

        // GET: EduHub/EduHubTypes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eduHubType = await _context.EduHubType
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eduHubType == null)
            {
                return NotFound();
            }

            return View(eduHubType);
        }

        // POST: EduHub/EduHubTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var eduHubType = await _context.EduHubType.FindAsync(id);
            _context.EduHubType.Remove(eduHubType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EduHubTypeExists(Guid id)
        {
            return _context.EduHubType.Any(e => e.Id == id);
        }
    }
}
