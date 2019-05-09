﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using POYA.Areas.XUserFile.Models;
using POYA.Data;
using POYA.Unities.Helpers;

namespace POYA.Areas.XUserFile.Controllers
{

    [Area("LUserFile")]
    [Authorize]
    public class LDirsController : Controller
    {

        #region
        private readonly IHostingEnvironment _hostingEnv;
        private readonly IStringLocalizer<Program> _localizer;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly X_DOVEHelper _x_DOVEHelper;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LDirsController> _logger;
        public LDirsController(
            ILogger<LDirsController> logger,
            SignInManager<IdentityUser> signInManager,
            X_DOVEHelper x_DOVEHelper,
            RoleManager<IdentityRole> roleManager,
           IEmailSender emailSender,
           UserManager<IdentityUser> userManager,
           ApplicationDbContext context,
           IHostingEnvironment hostingEnv,
           IStringLocalizer<Program> localizer)
        {
            _hostingEnv = hostingEnv;
            _localizer = localizer;
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _x_DOVEHelper = x_DOVEHelper;
            _signInManager = signInManager;
        }
        #endregion

        // GET: LDirs
        public async Task<IActionResult> Index()
        {
            var UserId_ = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
            return View(await _context.LDir.Where(p=>p.UserId==UserId_).ToListAsync());
        }

        // GET: LDirs/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //  ReturnUrl = ReturnUrl ?? Url.Content("~/");
            var UserId_ = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
            var lDir = await _context.LDir.FirstOrDefaultAsync(m => m.Id == id &&m.UserId==UserId_);
            if (lDir == null)
            {
                return NotFound();
            }   //  lDir.ReturnUrl = ReturnUrl ?? Url.Content("~/");
            lDir.InDirName= (await _context.LDir.Select(p => new { p.Id, p.Name }).FirstOrDefaultAsync(p => p.Id == lDir.InDirId))?.Name ?? "root";
            lDir.InFullPath = _x_DOVEHelper.GetInPathOfFileOrDir(_context, lDir.InDirId);
            return View(lDir);  //   LocalRedirect(ReturnUrl); //  View(lDir);
        }

        // GET: LDirs/Create
        public async Task<IActionResult> Create(Guid? InDirId)
        {
            #region
            /*
            ViewData[nameof(InDirId)]= InDirId ?? Guid.Empty;
            var UserId_ = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
            var LDirs = await _context.LDir.Where(p => p.UserId == UserId_ && p.InDirId == InDirId).ToListAsync();
            */
            #endregion
            var LDir_ = new LDir
            {
                InDirId = InDirId ?? Guid.Empty,
                InDirName = (await _context.LDir.Where(p => p.InDirId == InDirId).Select(p => p.Name).FirstOrDefaultAsync()) ?? "root",
                //  ReturnUrl = ReturnUrl
            };
            return View(LDir_);
        }

        // POST: LDirs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,InDirId")] LDir lDir)
        {
            if (ModelState.IsValid)
            {
                var UserId_ = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
                lDir.Id = Guid.NewGuid();
                lDir.UserId = UserId_;
                await _context.LDir.AddAsync(lDir);
                await _context.SaveChangesAsync();
                return RedirectToAction(actionName: "Index", controllerName: "LUserFiles", routeValues: new { lDir.InDirId });
                //  return LocalRedirect(lDir.ReturnUrl);
            }
            return View();
        }

        // GET: LDirs/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var UserId_ = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
            var lDir = await _context.LDir.FirstOrDefaultAsync(p=>p.UserId==UserId_ && p.Id==id);
            if (lDir == null)
            {
                return NotFound();
            }
            #region MOVE AND COPY

            //  var UserSubDirs = await _context.LDir.Where(p => p.Id != Guid.Empty && p.UserId == UserId_).ToListAsync();
            var UserDirs = await _context.LDir.Where(p => p.UserId == UserId_).ToListAsync();

            foreach(var i in GetAllSubDirs(UserDirs, lDir.Id))
            {
                UserDirs.Remove(i);
            }
            UserDirs.Remove(lDir);

            lDir.UserAllSubDirSelectListItems = new List<SelectListItem>() { new SelectListItem {  Value=Guid.Empty.ToString(),Text="root/"} };

            lDir.UserAllSubDirSelectListItems.AddRange(UserDirs.Select(p => new SelectListItem { Text = $"{_x_DOVEHelper.GetInPathOfFileOrDir(_context, p.InDirId)}{p.Name}", Value = p.Id.ToString() }).OrderBy(p => p.Text).ToList());

            lDir.CopyMoveSelectListItems = new List<SelectListItem>() {
                new SelectListItem{Text=_localizer[ "Rename"],Value=((int)CopyMove.DoNoThing).ToString(),Selected=true},
                new SelectListItem{Text=_localizer[ "Also move"], Value=((int)CopyMove.Move).ToString()}
            };

            #endregion
            lDir.InFullPath = _x_DOVEHelper.GetInPathOfFileOrDir(_context,lDir.InDirId);

            return View(lDir);
        }

        // POST: LDirs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,DOCreate,InDirId,CopyMove")] LDir lDir)
        {
            if (id != lDir.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var UserId_ = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
                    var _LDir = await _context.LDir.FirstOrDefaultAsync(p => p.Id == lDir.Id && p.UserId == UserId_);
                    if(_LDir==null)
                    {
                        return NotFound();
                    }

                    if (lDir.InDirId!=Guid.Empty &&!await _context.LDir.AnyAsync(p => p.Id == lDir.InDirId && p.UserId == UserId_))
                    {
                        ModelState.AddModelError(nameof(lDir.InDirId), _localizer["Sorry! the directory can't be found"]);
                        return View(lDir);
                    }

                    #region COPY AND MOVE
                    //  Move
                    if (lDir.CopyMove == CopyMove.Move)
                    {
                        _LDir.InDirId = lDir.InDirId;
                    }
                    //  Copy
                    else if (lDir.CopyMove == CopyMove.Copy)
                    {
                        var _AllUserDirs = await _context.LDir.Where(p => p.UserId == UserId_).ToListAsync();
                        var _AllUserFiles = await _context.LUserFile.Where(p => p.UserId == UserId_).ToListAsync();
                        #region GET A MAP
                        var IncludedDirs = _AllUserDirs.Where(p => IsFileOrDirInDir(_AllUserDirs, p.Id, lDir.Id)).ToList();
                        var IncludedFiles = _AllUserFiles.Where(p => IsFileOrDirInDir(_AllUserDirs, p.Id, lDir.Id)).ToList();
                        //  The first value is the new id 
                        var IDMap = new Dictionary<Guid, Guid>();
                        var NewDirs = new List<LDir>();
                        IncludedDirs.ForEach(d => {
                            var _d = new LDir {  Id=Guid.NewGuid(), Name=d.Name, UserId=UserId_ };
                            IDMap.Add(_d.Id,d.Id);
                        });
                        NewDirs.ForEach(_d => {
                            ////////////
                        });
                        #endregion

                    }

                    #endregion

                    else
                    {
                        _LDir.Name = lDir.Name;
                        //  _lUserFile.ContentType = _mimeHelper.GetMime(lUserFile.Name, _hostingEnv).Last();
                        //  _lUserFile.ContentType = _mimeHelper.GetMime(lUserFile.Name).Last();   // new Mime().Lookup(lUserFile.Name);
                        _context.Update(_LDir);
                        await _context.SaveChangesAsync();
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(actionName: "Index", controllerName: "LUserFiles", routeValues: new { _LDir.InDirId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LDirExists(lDir.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(lDir);
        }

        // GET: LDirs/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var UserId_ = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
            var lDir = await _context.LDir.FirstOrDefaultAsync(m => m.Id == id && m.UserId==UserId_);
            //  lDir.ReturnUrl = ReturnUrl ?? Url.Content("~/");
            if (lDir == null)
            {
                return NotFound();
            }
            lDir.InDirName = (await _context.LDir.Select(p => new { p.Id, p.Name }).FirstOrDefaultAsync(p => p.Id == lDir.InDirId))?.Name ?? "root";
            lDir.InFullPath = _x_DOVEHelper.GetInPathOfFileOrDir(_context, lDir.InDirId);

            return View(lDir);
        }

        // POST: LDirs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var UserId_ = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
            var lDir = await _context.LDir.FirstOrDefaultAsync(p=>p.Id==id && p.UserId==UserId_);
            if (lDir == null)
            {
                return NotFound();
            }
            var InDirId = lDir.InDirId;
            var UserDirs = await _context.LDir.Where(p => p.UserId == UserId_).ToListAsync();
            var _RemoveDirs = new List<LDir>() { lDir};

            _RemoveDirs.AddRange(GetAllSubDirs(UserDirs, id));

            _context.LDir.RemoveRange(_RemoveDirs);
            await _context.SaveChangesAsync();
            return RedirectToAction(actionName:"Index",controllerName:"LUserFiles",routeValues:new { InDirId});  //   LocalRedirect(ReturnUrl??Url.Content("~/"));
        }

        private bool LDirExists(Guid id)
        {
            return _context.LDir.Any(e => e.Id == id);
        }

        #region DEPOLLUTION
        /// <summary>
        /// Get a copy of dir with new id and includ files and directories
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task GetDirCopy(Guid id)
        {
            
        }

        private bool IsFileOrDirInDir(List<LDir> UserDirs, Guid id, Guid DirId)
        {
            if (!UserDirs.Select(p => p.Id).Contains(id)) return false;
            var _InDirId = UserDirs.FirstOrDefault(p => p.Id == id).InDirId;
            var _i = 0;
            while (_InDirId != Guid.Empty && _i < 30)
            {
                if (_InDirId == DirId)
                {
                    return true;
                }
                _InDirId = UserDirs.Where(p => p.Id == _InDirId).Select(p => p.InDirId).FirstOrDefault();
                _i++;
            }
            return false;
        }
        private List<LDir> GetAllSubDirs(List<LDir> UserDirs ,Guid DirId)
        {
            var SubDirs = new List<LDir>();
            foreach(var i in UserDirs) {
                if (IsFileOrDirInDir(UserDirs, i.Id, DirId))
                {
                    SubDirs.Add(i);
                }
            }
            return SubDirs;
        }
        #endregion
    }
}
