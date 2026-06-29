using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSecure.Mobile.Features.Auth.Models;

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public int License { get; set; }  // VALOR NUMÉRICO 0-4
    public string TransportCompany { get; set; } = string.Empty;
}