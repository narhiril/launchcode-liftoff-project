﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HouseholdManager.Models;
using Microsoft.AspNetCore.Authorization;
using HouseholdManager.Areas.Identity.Data;
using HouseholdManager.Data.API;
using HouseholdManager.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using HouseholdManager.Models.ViewModels;

namespace HouseholdManager.Controllers
{
    [Authorize(Roles = "Administrator,User")]
    public class HouseholdController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Member> _userManager;
        private readonly IQueryMembers _memberService;
        private readonly IRequestIcons _iconService;

        public HouseholdController(ApplicationDbContext context,
                                  UserManager<Member> userManager,
                                  IQueryMembers memberService,
                                  IRequestIcons iconService)
                                  
        {
            _userManager = userManager;
            _context = context;
            _memberService = memberService;
            _iconService = iconService;
        }

        //GET: Household
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        //GET: Household/AddOrJoinHousehold
        public async Task<IActionResult> AddOrJoinHousehold() 
        {
            return await UserHasHousehold() ? RedirectToAction("Index")
                                            : View();
        }

        //GET: Household/JoinExisting
        public async Task<IActionResult> JoinExisting()
        {
            return await UserHasHousehold() ? RedirectToAction("Index")
                                            : View(); //TODO: ViewModel
        }

        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinExisting(JoinHouseholdViewModel model)
        {
        // TODO
        }
        */

        // GET: Household/ViewAll
        public async Task<IActionResult> ViewAll()
        {
            var dataQuery = _context.Households;
            var list = await dataQuery.ToListAsync();
            return View(list);
        }

        //GET: Household/Setup
        public async Task<IActionResult> Setup()
        {
            ViewBag.Icons = await _iconService.PopulateIcons();
            return View(new EditHouseholdViewModel());
        }

        // POST: Household/Setup
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup([Bind("Name,Icon")] EditHouseholdViewModel model)
        {
            if (await UserHasHousehold()) return RedirectToAction("Index");
            if (ModelState.IsValid)
            {
                Household household = new Household()
                {
                    Name = model.Name,
                    Icon = model.Icon
                };
                //Set the current user to household administrator
                var member = await _userManager.GetUserAsync(User);
                //TODO: Eventually rework this to user IdentityRoles
                member.MemberType = "Administrator";
                household.Members.Add(member);
                member.Household = household;
                //Save to database
                _context.Add(household);
                _context.Update(member);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Icons = await _iconService.PopulateIcons();
                return View(model);
            }
        }

        // GET: Household/Edit/{id}
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Icons = await _iconService.PopulateIcons();
            if (id < 1)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var member = await _userManager.GetUserAsync(User);
                /* TODO: Ideally a future admin role should bypass this check, commented out for now
                if (id != member.HouseholdId)
                {
                    return Forbid();
                }
                */
                var household = _context.Households.Find(id);
                if (household is null) return NotFound();
                return View(new EditHouseholdViewModel
                {
                    Icon = household.Icon,
                    Name = household.Name
                });
            }
        }

        // POST: Household/Edit
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Name,Icon")] EditHouseholdViewModel model)
        {
            if (ModelState.IsValid)
            {
                var member = await _userManager.GetUserAsync(User);
                var household = member.Household;
                if (household is null) //this is unlikely, but just in case
                {
                    household = new Household();
                    household.Name = model.Name;
                    household.Icon = model.Icon;
                    _context.Add(household);
                }
                else
                {
                    household.Name = model.Name;
                    household.Icon = model.Icon;
                    _context.Update(household);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Icons = await _iconService.PopulateIcons();
            return View(model);
        }

        // POST: Household/Delete/{id}
        [Authorize(Roles = "Administrator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Households == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Households' is null.");
            }
            var household = await _context.Households.FindAsync(id);
            if (household != null)
            {
                _context.Households.Remove(household);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [NonAction]
        public async Task<bool> UserHasHousehold()
        {
            try
            {
                _ = await _memberService.GetCurrentHousehold();
                return true;
            }
            //user has no household
            catch (KeyNotFoundException) {
                return false;
            }
        }
    }    
}
