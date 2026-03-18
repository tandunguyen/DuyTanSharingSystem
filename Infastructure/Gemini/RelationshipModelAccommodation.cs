using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DuyTanSharingSystem.Infastructure.Gemini
{
    // Lớp chứa thông tin mối quan hệ (FK)
    public class RelationshipModelAccommodation
    {
        public string SourceColumn { get; set; } = string.Empty;
        public string TargetTable { get; set; } = string.Empty;
        public string TargetColumn { get; set; } = string.Empty;
        public string RelationshipType { get; set; } = string.Empty;
        public string OnDelete { get; set; } = string.Empty;
        public string NavigationName { get; set; } = string.Empty;
    }

    // Lớp chứa thông tin cột
    public class ColumnModel
    {
        public string Name { get; set; } = string.Empty;
        public string SqlType { get; set; } = string.Empty;

        // Đổi tên DotNetType thành DotNetDataType để tránh nhầm lẫn nếu cần
        [JsonPropertyName("DotNetType")]
        public string DotNetDataType { get; set; } = string.Empty;

        public bool IsPrimaryKey { get; set; }
        public bool IsNullable { get; set; }
        public bool IsForeignKey { get; set; }
        public string References { get; set; } = string.Empty; // Tên bảng tham chiếu
        public string Default { get; set; } = string.Empty;
        public string Range { get; set; } = string.Empty; // Dùng cho ràng buộc Check (vd: 1-5)
        public string Description { get; set; } = string.Empty;
    }

    // Lớp chính chứa thông tin của một bảng (Users, AccommodationPosts,...)
    public class TableModel
    {
        public string TableName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;             
        public List<ColumnModel> Columns { get; set; } = new List<ColumnModel>();
        public List<RelationshipModelAccommodation> Relationships { get; set; } = new List<RelationshipModelAccommodation>();
    }
}