using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers;

[ApiController]
[Route("api/cuentas")]
public class CuentasController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _config;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly HashService _hashService;
    private readonly IDataProtector _dataProtector;

    public CuentasController(UserManager<IdentityUser> userManager, IConfiguration config, SignInManager<IdentityUser> signInManager, 
        IDataProtectionProvider dataProtectionProvider, HashService hashService)
    {
        _userManager = userManager;
        _config = config;
        _signInManager = signInManager;
        _hashService = hashService;
        _dataProtector = dataProtectionProvider.CreateProtector("valor_unico_y_quizas_secreto");
    }

    [HttpGet("hash/{textoPlano}")]
    public ActionResult RealizarHash(string textoPlano)
    {
        var resultado1 = _hashService.Hash(textoPlano);
        var resultado2 = _hashService.Hash(textoPlano);

        return Ok(new
        {
            textoPlano = textoPlano,
            hash1 = resultado1,
            hash2 = resultado2
        });
    }


    [HttpPost]
    [Route("registrar")]
    public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsuario credencialesUsuario)
    {
        var usuario = new IdentityUser { UserName = credencialesUsuario.Email, Email = credencialesUsuario.Email };
        var resultado = await _userManager.CreateAsync(usuario, credencialesUsuario.Password);

        if (resultado.Succeeded)
        {
            return await ConstruirToken(credencialesUsuario);
        } 
        else
        {
            return BadRequest(resultado.Errors);
        }
    }

    [HttpPost("login", Name = "loginUsuario")]
    public async Task <ActionResult<RespuestaAutenticacion>> Login (CredencialesUsuario credencialesUsuario)
    {
        var resultado = await _signInManager.PasswordSignInAsync(credencialesUsuario.Email, 
            credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

        if (resultado.Succeeded)
        {
            return await ConstruirToken(credencialesUsuario);
        } 
        else
        {
            return BadRequest("Login incorrecto");
        }
    }

    [HttpGet("RenovarToken", Name = "renivarToken")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<RespuestaAutenticacion>> RenovarToken()
    {
        var emailClaim = HttpContext.User.Claims.Where(x => x.Type == "email").FirstOrDefault();
        var email = emailClaim.Value;
        var credencialesUsuario = new CredencialesUsuario()
        {
            Email = email
        };

        return await ConstruirToken(credencialesUsuario);
    }

    [HttpPost("HacerAdmin", Name = "hacerAdmin")]
    public async Task<ActionResult> HacerAdmin(EditarAdminDto editarAdminDto)
    {
        var usuario = await _userManager.FindByEmailAsync(editarAdminDto.Email);
        await _userManager.AddClaimAsync(usuario, new Claim("EsAdmin", "1"));
        return NoContent();
    }

    [HttpPost("RemoverAdmin", Name = "removerAdmin")]
    public async Task<ActionResult> RemoverAdmin(EditarAdminDto editarAdminDto)
    {
        var usuario = await _userManager.FindByEmailAsync(editarAdminDto.Email);
        await _userManager.RemoveClaimAsync(usuario, new Claim("EsAdmin", "1"));
        return NoContent();
    }

    private async Task<RespuestaAutenticacion> ConstruirToken (CredencialesUsuario credencialesUsuario)
    {
        var claims = new List<Claim>()
        {
            new Claim("email", credencialesUsuario.Email) // No poner datos privados
        };

        var usuario = await _userManager.FindByEmailAsync(credencialesUsuario.Email);
        var claimsDb = await _userManager.GetClaimsAsync(usuario);

        claims.AddRange(claimsDb);

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
