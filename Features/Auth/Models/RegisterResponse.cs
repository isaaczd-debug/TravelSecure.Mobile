using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSecure.Mobile.Features.Auth.Models;

public class RegisterResponse
{
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string TransportCompany { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}