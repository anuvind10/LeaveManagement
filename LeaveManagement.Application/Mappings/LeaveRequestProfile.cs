using AutoMapper;
using LeaveManagement.Application.DTOs;
using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Application.Mappings
{
    public class LeaveRequestProfile : Profile
    {
        public LeaveRequestProfile()
        {
            CreateMap<LeaveRequest, LeaveRequestDto>();
            CreateMap<LeaveAudit, LeaveAuditDto>();
            CreateMap<LeaveRequest, LeaveRequestSummaryDto>();
        }
    }
}
