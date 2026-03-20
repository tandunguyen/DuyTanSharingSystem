using Application.DTOs.Accommodation;
using Application.Interface;
using Application.Interface.Api;
using Domain.Entities;
using Domain.Interface; // IUnitOfWork
using System.Linq.Expressions;

namespace Application.Services
{
    public class AccommodationService : IAccommodationService
    {
        private readonly IMapService _mapService;
        private readonly IUnitOfWork _unitOfWork;


        public AccommodationService(IMapService mapService, IUnitOfWork unitOfWork
            /*, IpythonGeminiService geminiService */)
        {
            _mapService = mapService;
            _unitOfWork = unitOfWork;
            // _geminiService = geminiService;
        }

        
    }
}