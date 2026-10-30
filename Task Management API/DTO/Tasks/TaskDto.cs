namespace Task_Management_API.DTO.Tasks
{
    public class TaskDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public long? StatusId { get; set; }
        public string? StatusName { get; set; }
        public long? UserId { get; set; }
    }
}
