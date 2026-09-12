using ApnaRent.Data;
using ApnaRent.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;


public class AuthController : Controller
{

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _context;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IConfiguration config)
    {
        _userManager = userManager;
        _context = context;
        _config = config;
    }
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        return View(user);
    }
    public IActionResult Login(string returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
        {
            ViewBag.Error = "Invalid email or password";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        bool hasOtpRecord = _context.EmailOtps.Any(x => x.Email == email);
        if (hasOtpRecord && !user.IsOtpVerified)
        {
            ViewBag.Error = "Please verify your email first";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var token = await GenerateJwt(user);

        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false
        });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin"))
            return RedirectToAction("Index", "AdminHome");

        return RedirectToAction("Index", "Home");
    }

    public IActionResult ForgotPassword() => View();

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ViewBag.Error = "No account found with this email";
            return View();
        }

        string otp = new Random().Next(100000, 999999).ToString();

        _context.EmailOtps.Add(new EmailOtp
        {
            Email = email,
            Otp = otp,
            ExpiryTime = DateTime.Now.AddMinutes(5),
            IsUsed = false
        });
        await _context.SaveChangesAsync();

        SendOtpEmail(email, otp);

        return RedirectToAction("VerifyResetOtp", new { email });
    }

    public IActionResult VerifyResetOtp(string email)
    {
        ViewBag.Email = email;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> VerifyResetOtp(string email, string otp)
    {
        var record = _context.EmailOtps
            .Where(x => x.Email == email && x.Otp == otp && !x.IsUsed)
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();

        if (record == null || record.ExpiryTime < DateTime.Now)
        {
            ViewBag.Error = "Invalid or expired OTP";
            ViewBag.Email = email;
            return View();
        }

        record.IsUsed = true;
        await _context.SaveChangesAsync();

        return RedirectToAction("ResetPassword", new { email });
    }

    public IActionResult ResetPassword(string email)
    {
        ViewBag.Email = email;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(string email, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return RedirectToAction("Login");

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        if (!result.Succeeded)
        {
            ViewBag.Error = result.Errors.First().Description;
            ViewBag.Email = email;
            return View();
        }

        return RedirectToAction("Login");
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string fullName, string email, string password)
    {
        var user = new ApplicationUser
        {
            FullName = fullName,
            UserName = email,
            Email = email,
            IsOtpVerified = false
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            ViewBag.Error = result.Errors.First().Description;
            return View();
        }

        await _userManager.AddToRoleAsync(user, "User");

        string otp = new Random().Next(100000, 999999).ToString();

        _context.EmailOtps.Add(new EmailOtp
        {
            Email = email,
            Otp = otp,
            ExpiryTime = DateTime.Now.AddMinutes(5),
            IsUsed = false
        });

        await _context.SaveChangesAsync();

        SendOtpEmail(email, otp);

        return RedirectToAction("VerifyOtp", new { email });
    }

    public IActionResult VerifyOtp(string email)
    {
        ViewBag.Email = email;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOtp(string email, string otp)
    {
        var record = _context.EmailOtps
            .Where(x => x.Email == email && x.Otp == otp && !x.IsUsed)
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();

        if (record == null || record.ExpiryTime < DateTime.Now)
        {
            ViewBag.Error = "Invalid or expired OTP";
            ViewBag.Email = email;
            return View();
        }

        record.IsUsed = true;

        var user = await _userManager.FindByEmailAsync(email);
        user.IsOtpVerified = true;

        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    private void SendOtpEmail(string toEmail, string otp)
    {
        var smtpHost = _config["SmtpSettings:Host"];
        var smtpPort = int.Parse(_config["SmtpSettings:Port"]);
        var smtpUser = _config["SmtpSettings:Username"];
        var smtpPass = _config["SmtpSettings:Password"];
        var fromEmail = _config["SmtpSettings:FromEmail"];

        var smtp = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        var mail = new MailMessage(fromEmail, toEmail)
        {
            Subject = "ApnaRent OTP Verification",
            Body = $"Your OTP is: {otp}\nThis OTP will expire in 5 minutes."
        };

        smtp.Send(mail);
    }

    private async Task<string> GenerateJwt(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public IActionResult Logout()
    {
        bool wasAdmin = User.IsInRole("Admin");

        Response.Cookies.Delete("jwt");

        if (wasAdmin)
            return RedirectToAction("Login");

        return RedirectToAction("Index", "Home");
    }
}