namespace Application.DTOs.User
{
    public class UserInformationDetailDto
    {
        public required string Email { get; set; }          
        public required string FullName { get; set; }          
        public string? Bio { get; set; }    
        public string? Phone { get; set; }
        public string? PhoneRelative { get; set; }
        public string? Gender { get; set; } = "Không rõ";      
        public bool IsVerifiedEmail { get; set; }            
        public decimal TrustScore { get; set; }               
        public required string CreatedAt { get; set; }              
        public string? UpdatedAt { get; set; }               
    }
}
