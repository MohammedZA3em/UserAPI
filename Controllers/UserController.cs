using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheLastUserProject.DataDTO;
using UserProjectTest.Data;
using UserProjectTest.Data.Model;
using UserProjectTest.DataDTO;
using static System.Net.Mime.MediaTypeNames;

namespace TheLastUserProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<Roles> _roleManager;

        public UserController(UserDbContext context, UserManager<Users> userManager, RoleManager<Roles> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // إنشاء مستخدم
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDTO user)
        {

            var User = new Users()
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(User, user.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync(user.role))
            {
                return BadRequest();
            }
            await _userManager.AddToRoleAsync(User, user.role);

            var DTO = new ResponsUserDTO()
            {
                Id = User.Id,
                UserName = User.UserName,
                Email = User.Email,
                PhoneNumber = User.PhoneNumber,
                role = user.role,
                Image = User.ImagesUrl
            };

            return Ok(DTO);
        }

        // جلب كل المستخدمين
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            //var DTOs = await 
            //    (
            //    from user in _context.Users
            //    join userRole in _context.UserRoles on user.Id equals userRole.UserId
            //    join role in _context.Roles on userRole.RoleId equals role.Id
            //    select new ResponsUserDTO
            //    {
            //        Id = user.Id,
            //        UserName = user.UserName,
            //        Email = user.Email,
            //        PhoneNumber = user.PhoneNumber,
            //        role = role.Name, // اسم الدور من جدول Roles
            //        Image = user.ImagesUrl
            //    }).ToListAsync();

            var DTOs = await _context.Users
      .Join(_context.UserRoles,
            u => u.Id,
            ur => ur.UserId,
            (u, ur) => new { u, ur })
      .Join(_context.Roles,
            temp => temp.ur.RoleId,
            r => r.Id,
            (temp, r) => new ResponsUserDTO
            {
                Id = temp.u.Id,
                UserName = temp.u.UserName,
                Email = temp.u.Email,
                PhoneNumber = temp.u.PhoneNumber,
                role = r.Name,
                Image = temp.u.ImagesUrl
            })
      .ToListAsync();

            return Ok(DTOs);
        }

        // جلب مستخدم حسب Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {

            var userDTO = await (from u in _context.Users
                                 join ur in _context.UserRoles on u.Id equals ur.UserId
                                 join r in _context.Roles on ur.RoleId equals r.Id
                                 where u.Id == id
                                 select new ResponsUserDTO
                                 {
                                     Id = u.Id,
                                     UserName = u.UserName,
                                     Email = u.Email,
                                     PhoneNumber = u.PhoneNumber,
                                     role = r.Name,
                                     Image = u.ImagesUrl
                                 }).FirstOrDefaultAsync(); // نستخدم FirstOrDefault لجلب مستخدم واحد

            if (userDTO == null)
                return NotFound(); // إذا لم يوجد المستخدم

            return Ok(userDTO);
        }

        //// تحديث مستخدم
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(int id, [FromBody] UserDTO user)
        //{
        //    if (id != user.Id) return BadRequest("User ID mismatch");

        //    var existingUser = await _userManager.FindByIdAsync(id.ToString());
        //    if (existingUser == null) return NotFound();

        //    existingUser.UserName = user.UserName;
        //    existingUser.Email = user.Email;
        //    existingUser.PhoneNumber = user.PhoneNumber;
        //    existingUser.Image = user.Image;

        //    var result = await _userManager.UpdateAsync(existingUser);
        //    if (!result.Succeeded) return BadRequest(result.Errors);

        //    return Ok(existingUser);
        //}

        //    // حذف مستخدم
        //    [HttpDelete("{id}")]
        //    public async Task<IActionResult> Delete(int id)
        //    {
        //        var user = await _userManager.FindByIdAsync(id.ToString());
        //        if (user == null) return NotFound();

        //        var result = await _userManager.DeleteAsync(user);
        //        if (!result.Succeeded) return BadRequest(result.Errors);

        //        return Ok();
        //    }
        //}

    }
}
