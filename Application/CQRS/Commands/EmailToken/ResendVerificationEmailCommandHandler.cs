using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Commands.EmailToken
{
    public class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand, ResponseModel<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public ResendVerificationEmailCommandHandler(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<ResponseModel<string>> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 🔍 Check if email exists
                var user = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<string>("User with this email does not exist.", 404);
                }

                // 🔎 Check if email is already verified
                if (user.IsVerifiedEmail)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<string>("Email already verified", 400);
                }

                // 📩 Generate and send new verification email
                var token = await _userService.SendVerifiEmailAsync(user.Id, request.Email);
                if (token == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<string>("Failed to send verification email.", 400);
                }

                // 💾 Save new token
                var newToken = new EmailVerificationToken(user.Id, token, DateTime.UtcNow.AddHours(1));
                await _unitOfWork.EmailTokenRepository.AddAsync(newToken);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Truyền giá trị data kiểu string
                return ResponseFactory.Success<string>("Verification email sent", "New verification email sent successfully.", 200);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Error<string>("Failed to resend verification email", 400, ex);
            }
        }
    }
}