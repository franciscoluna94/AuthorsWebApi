using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers;

[ApiController]
[Route("api/cuentas")]
public class CuentasController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _config;
    private readonly SignInManager<IdentityUser> _signInManager;

    public CuentasController(UserManager<IdentityUser> userManager, IConfiguration config, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _config = config;
        _signInManager = signInManager;
    }

    [HttpPost]
    [Route("registrar")]
    public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsuario credencialesUsuario)
    {
        var usuario = new IdentityUser { UserName = credencialesUsuario.Email, Email = credencialesUsuario.Email };
        var resultado = await _userManager.CreateAsync(usuario, credencialesUsuario.Password);

        if (resultado.Succeeded)
        {
            return ConstruirToken(credencialesUsuario);
        } 
        else
        {
            return BadRequest(resultado.Errors);
        }
    }

    [HttpPost("login")]
    public async Task <ActionResult<RespuestaAutenticacion>> Login (CredencialesUsuario credencialesUsuario)
    {
        var resultado = await _signInManager.PasswordSignInAsync(credencialesUsuario.Email, 
            credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

        if (resultado.Succeeded)
        {
            return ConstruirToken(credencialesUsuario);
        } 
        else
        {
            return BadRequest("Login incorrecto");
        }
    }


    private RespuestaAutenticacion ConstruirToken (CredencialesUsuario credencialesUsuario)
    {
        var claims = new List<Claim>()
        {
            new Claim("email", credencialesUsuario.Email) // No poner datos privados
        };

        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["llavejwt"]));
        var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
        var expiracion = DateTime.UtcNow.AddYears(1);

        var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);

        return new RespuestaAutenticacion()
        { 
            Token = new JwtSecurityTokenHandler().WriteToken(securityToken), 
            Expiracion = expiracion 
        };
    }

}
