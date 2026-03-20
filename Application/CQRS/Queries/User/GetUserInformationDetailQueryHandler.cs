using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Queries.User
{
    public class GetUserInformationDetailQueryHandler : IRequestHandler<GetUserInformationDetailQuery, ResponseModel<UserInformationDetailDto>>
    {
        private readonly IUserContextService _userContextService;

        private readonly IUnitOfWork _unitOfWork;
        public GetUserInformationDetailQueryHandler(IUserContextService userContextService, IUnitOfWork unitOfWork)
        {
            _userContextService = userContextService;

            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseModel<UserInformationDetailDto>> Handle(GetUserInformationDetailQuery request, CancellationToken cancellationToken)
        {
            var userId = _userContextService.UserId();
            if (userId == Guid.Empty)
            {
                return ResponseFactory.Fail<UserInformationDetailDto>("Unauthorized", 401);
            }

            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return ResponseFactory.Fail<UserInformationDetailDto>("User not found", 404);
            }
            return ResponseFactory.Success(Mapping.MapToUserInformationDetailDto(user), "Get user information detail success", 200);
        }
    }
}

