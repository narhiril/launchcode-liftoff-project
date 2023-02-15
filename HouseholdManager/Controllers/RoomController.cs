using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HouseholdManager.Models;
using HouseholdManager.Data.API;
using HouseholdManager.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using HouseholdManager.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using HouseholdManager.Models.ViewModels;

namespace HouseholdManager.Controllers
{
    [Authorize(Roles = "Administrator,User")]
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Member> _userManager;
        private readonly IQueryMembers _memberService;
        private readonly IRequestIcons _iconService;

        public RoomController(ApplicationDbContext context, 
                              UserManager<Member> userManager,
                              IQueryMembers memberService,
                              IRequestIcons iconService)
        {
            _context = context;
            _userManager = userManager;
            _memberService = memberService;
            _iconService = iconService;
        }

        // GET: Room
        public async Task<IActionResult> Index()
        {
            Household household;
            try
            {
                household = await _memberService.GetCurrentHousehold();
            }
            //Exception thrown if user has no household
            catch (KeyNotFoundException e)
            {
                Console.Error.WriteLine($"Caught exception: {e.Message}");
                return Forbid();
            }
            var roomsQuery = from room in _context.Rooms
                             where room.HouseholdId == household.Id
                             select new EditRoomViewModel(room);
            return View(roomsQuery.ToList());
        }

        // POST: Room/DirtHandler
        /// <summary>
        /// Processes AJAX request from dirtometerAjax.js
        /// </summary>
        /// <param name="data"></param>
        public async Task<IActionResult> DirtHandler([FromBody]Dictionary<int, int> data)
        {
            //Validation
            if (data.Count == 0) return Json("No data to update.");
            
            foreach(KeyValuePair<int, int> kvp in data)
            {
                if (kvp.Key < 0 || kvp.Value > 10 || kvp.Value < 0)
                {
                    data.Remove(kvp.Key);
                }
            }
            //Get matching rooms and stage update
            var allRooms = await _context.Rooms.ToListAsync();
            List<Room> roomsToUpdate = new List<Room>();
            foreach (Room rm in allRooms)
            {
                if (data.ContainsKey(rm.Id))
                {
                    if (rm.DirtLevel != data[rm.Id])
                    {
                        rm.DirtLevel = data[rm.Id];
                        roomsToUpdate.Add(rm);
                    }
                }
            }
            if (roomsToUpdate.Count == 0) return Json("No room dirt updates required.");
            //Update database
            _context.UpdateRange(roomsToUpdate);
            _context.SaveChanges();
            return Json("Room dirt values updated.");
        }

        // GET: Room/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Rooms == null)
            {
                return NotFound();
            }
            else if (!await RoomInHousehold((int)id))
            {
                return Forbid();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(new EditRoomViewModel(room));
        }

        // GET: Room/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Icons = await _iconService.PopulateIcons();
            return View();
        }

        // POST: Room/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Icon")] EditRoomViewModel model)
        {
            if (ModelState.IsValid)
            {
                var household = await _memberService.GetCurrentHousehold();
                if (household is null)
                {
                    //This is inelegant and should probably be reworked later
                    return Redirect("Household/AddOrJoinHousehold");
                }
                var room = new Room()
                {
                    Name = model.Name,
                    Icon = model.Icon,
                    Household = household,
                    HouseholdId = household.Id
                };
                household.Rooms.Add(room);
                _context.Add(room);
                _context.Update(household);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Icons = await _iconService.PopulateIcons();
            return View(new EditRoomViewModel(model.Name, model.Icon));
        }

        // GET: Room/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Rooms == null)
            {
                return NotFound();
            }
            else if (!await RoomInHousehold((int)id))
            {
                return Forbid();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            ViewBag.Icons = await _iconService.PopulateIcons();
            return View(new EditRoomViewModel(room));
        }

        // POST: Room/Edit/{id}
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Icon")] EditRoomViewModel model)
        {
            var household = await _memberService.GetCurrentHousehold();
            var room = (from rm in _context.Rooms
                        where rm.Id == id && rm.HouseholdId == household.Id
                        select rm).FirstOrDefault();
            if (room is null || id != room.Id)
            {
                return NotFound();
            } 
            else if (!await RoomInHousehold(room.Id))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    room.Icon = model.Icon;
                    room.Name = model.Name;
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
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
            ViewBag.Icons = await _iconService.PopulateIcons();
            return View(new EditRoomViewModel(room));
        }

        // GET: Room/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Rooms == null)
            {
                return NotFound();
            }
            else if (!await RoomInHousehold((int)id))
            {
                return Forbid();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(new EditRoomViewModel(room));
        }

        // POST: Room/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Rooms == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Rooms' is null.");
            }
            else if (!await RoomInHousehold((int)id))
            {
                return Forbid();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room is null) return NotFound();
            var missionQuery = from mission in _context.Missions
                           where mission.RoomId == id
                           select mission;
            //Check for missions in room and remove Id from those missions
            if (missionQuery.Any())
            {
                foreach (Mission mission in missionQuery)
                {
                    mission.RoomId = null;
                    _context.Update(mission);
                }
                await _context.SaveChangesAsync();
            }
            //Delete room
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [NonAction]
        private bool RoomExists(int id)
        {
          return _context.Rooms.Any(e => e.Id == id);
        }

        [NonAction]
        private async Task<bool> RoomInHousehold(int id)
        {
            var household = await _memberService.GetCurrentHousehold();
            if (household is null) return false;
            var found = from room in _context.Rooms
                        where room.Id == id && room.HouseholdId == household.Id
                        select room;
            return found.ToList().Any();
        }

        [NonAction]
        public async Task PopulateIcons()
        {
            IconRequestor req = new IconRequestor();
            List<Icon> icons = await req.GetIconsFromApi();
            ViewBag.Icons = icons;
        }

        [NonAction]
        private Room UpdateDirt(Room room, int value)
        {
            if (room.DirtLevel == value) return room;
            room.DirtLevel = value;
            return room;
        }

    }


}
