﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;

public class LoginController : Controller
{
    private readonly IApiService _apiService;
    private readonly string _endpoint = "auth/login";

    public LoginController(IApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Index", model);

        var response = await _apiService.LoginPostAsync<LoginResponse>(_endpoint, model);

        if (response.Exito == true)
        {
            // Guardar la cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, response.UsuarioId.ToString()), //id usuario
                new Claim(ClaimTypes.Role, response.RolNombre), // "Admin"
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        //ModelState.AddModelError("", "Credenciales inválidas.");
        TempData["Mensaje"] = "Credenciales inválidas.";
        TempData["TipoMensaje"] = "error";
        return View("Index", model);
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Login");

    }

    [HttpGet("AccesoDenegado")]
    //[Route("AccesoDenegado")]
    public IActionResult AccesoDenegado()
    {
        TempData["Mensaje"] = "No tienes permisos para acceder a esta sección.";
        TempData["TipoMensaje"] = "error";
        return RedirectToAction("Index", "Home");
    }

}

