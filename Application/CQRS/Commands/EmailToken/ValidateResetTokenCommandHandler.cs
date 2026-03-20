using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Commands.EmailToken
{
    internal class ValidateResetTokenCommandHandler : IRequestHandler<ValidateResetTokenCommand, ResponseModel<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ValidateResetTokenCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel<bool>> Handle(ValidateResetTokenCommand request, CancellationToken cancellationToken)
        {
            var emailToken = await _unitOfWork.EmailTokenRepository.GetByTokenAsync(request.Token);
            if (emailToken == null || emailToken.IsUsed || emailToken.ExpiryDate < DateTime.UtcNow)
            {
                return ResponseFactory.Fail<bool>("Invalid or expired token", 400);
            }

            return ResponseFactory.Success(true, "Token is valid", 200);
        }
    }
}
