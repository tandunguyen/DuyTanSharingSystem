using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Commands.StudyMaterials
{
    public class CountDowloadCommandHandler : IRequestHandler<CountDowloadCommand, ResponseModel<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CountDowloadCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseModel<int>> Handle(CountDowloadCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var material = await _unitOfWork.StudyMaterialRepository.GetByIdAsync(request.StudyMaterialId);
                if (material == null)
                {
                    return ResponseFactory.Fail<int>("Study Material not found", 404);
                }
                material.IncrementDownloadCount();
                 await _unitOfWork.StudyMaterialRepository.UpdateAsync(material);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return ResponseFactory.Success<int>("Count Success",200);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
