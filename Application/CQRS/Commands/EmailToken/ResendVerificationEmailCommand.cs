using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Commands.EmailToken
{
    public class ResendVerificationEmailCommand : IRequest<ResponseModel<string>>
    {
        public string Email { get; set; }

        public ResendVerificationEmailCommand(string email)
        {
            Email = email;
        }
    }
}
