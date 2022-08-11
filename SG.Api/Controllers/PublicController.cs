using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SG.Api.Models;

namespace SG.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PublicController : ControllerBase
    {
        [HttpPost]
        [Route("[action]")]
        public IActionResult HashCheck([FromBody] HashCheck model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(model.SecretKey));
            var hashResult = hmac.ComputeHash(Encoding.ASCII.GetBytes(model.Data));

            return Ok(Convert.ToBase64String(hashResult));
        }
    }
}